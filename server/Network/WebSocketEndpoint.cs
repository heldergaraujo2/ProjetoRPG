using System.Net.WebSockets;
using ProjetoRpg.Server.Accounts;
using ProjetoRpg.Server.World;
using ProjetoRpg.Shared.Protocol;
using ProjetoRpg.Shared.World;

namespace ProjetoRpg.Server.Network;

public static class WebSocketEndpoint
{
    public static async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("WebSocket upgrade required.", context.RequestAborted);
            return;
        }

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ProjetoRpg.Network.WebSocket");
        var auth = context.RequestServices.GetRequiredService<AuthService>();
        var token = AuthService.GetBearerToken(context.Request);
        var hasAuthorizationHeader = context.Request.Headers.ContainsKey("Authorization");
        var accountId = token is null ? null : await auth.GetAuthenticatedAccountAsync(token, context.RequestAborted);
        if (hasAuthorizationHeader && accountId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var cancellationToken = context.RequestAborted;
        var sessions = context.RequestServices.GetRequiredService<WorldSessionService>();
        var registry = context.RequestServices.GetRequiredService<WorldConnectionRegistry>();
        var connectionId = Guid.NewGuid();
        Guid? attachedSessionId = null;
        try
        {
            var hello = await ReceiveEnvelopeAsync(socket, cancellationToken);
            if (hello is null)
            {
                await CloseIfOpenAsync(socket, WebSocketCloseStatus.InvalidPayloadData, "invalid_handshake", cancellationToken);
                return;
            }

            if (hello.Type != "Hello" || hello.ProtocolVersion != ProtocolConstants.CurrentVersion)
            {
                await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, hello.Sequence, "protocol_version_or_handshake_invalid"), cancellationToken);
                await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "protocol_mismatch", cancellationToken);
                return;
            }

            await SendAsync(socket, new ServerEnvelope("HelloAck", ProtocolConstants.CurrentVersion, hello.Sequence, "ready"), cancellationToken);
            var lastSequence = hello.Sequence;
            var worldJoined = false;
            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveEnvelopeAsync(socket, cancellationToken);
                if (message is null)
                    break;

                if (message.Sequence <= lastSequence)
                {
                    await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "sequence_must_increase"), cancellationToken);
                    await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "invalid_sequence", cancellationToken);
                    break;
                }
                lastSequence = message.Sequence;

                if (message.ProtocolVersion != ProtocolConstants.CurrentVersion)
                {
                    await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "protocol_version_mismatch"), cancellationToken);
                    await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "protocol_mismatch", cancellationToken);
                    break;
                }

                if (message.Type == "Ping")
                {
                    await SendAsync(socket, new ServerEnvelope("Pong", ProtocolConstants.CurrentVersion, message.Sequence), cancellationToken);
                    continue;
                }
                if (message.Type == "Bye")
                {
                    await CloseIfOpenAsync(socket, WebSocketCloseStatus.NormalClosure, "client_logout", cancellationToken);
                    break;
                }
                if (message.Type == "WorldJoin")
                {
                    if (worldJoined || accountId is null || token is null ||
                        !ProtocolCodec.TryDecodePayload<WorldJoinPayload>(message.Payload, out var join) ||
                        join is null || join.WorldSessionId == Guid.Empty)
                    {
                        await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "world_join_not_authorized"), cancellationToken);
                        await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "world_join_not_authorized", cancellationToken);
                        break;
                    }

                    var snapshot = await sessions.AttachAsync(accountId.Value, join.WorldSessionId, cancellationToken);
                    if (snapshot is null)
                    {
                        await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "world_session_unavailable"), cancellationToken);
                        await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "world_session_unavailable", cancellationToken);
                        break;
                    }
                    if (!registry.TryAttach(join.WorldSessionId, connectionId))
                    {
                        await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "world_session_already_connected"), cancellationToken);
                        await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "world_session_already_connected", cancellationToken);
                        break;
                    }
                    attachedSessionId = join.WorldSessionId;
                    worldJoined = true;
                    await SendAsync(socket, new ServerEnvelope("WorldJoined", ProtocolConstants.CurrentVersion,
                        message.Sequence, "ready", ProtocolCodec.ToPayload(snapshot)), cancellationToken);
                    continue;
                }
                if (message.Type == "MoveIntent" && worldJoined && attachedSessionId is { } currentSessionId &&
                    accountId is { } ownerAccountId && token is not null)
                {
                    var currentAccount = await auth.GetAuthenticatedAccountAsync(token, cancellationToken);
                    if (currentAccount != ownerAccountId)
                    {
                        await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "authentication_expired"), cancellationToken);
                        await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "authentication_expired", cancellationToken);
                        break;
                    }
                    if (!ProtocolCodec.TryDecodePayload<MoveIntentPayload>(message.Payload, out var intent) || intent is null)
                    {
                        await SendAsync(socket, new ServerEnvelope("InputRejected", ProtocolConstants.CurrentVersion, message.Sequence, "invalid_move_intent"), cancellationToken);
                        continue;
                    }
                    var result = await sessions.MoveAsync(ownerAccountId, currentSessionId, intent, cancellationToken);
                    if (!result.Accepted)
                    {
                        await SendAsync(socket, new ServerEnvelope("InputRejected", ProtocolConstants.CurrentVersion,
                            message.Sequence, result.RejectionCode ?? "move_rejected"), cancellationToken);
                        if (result.RejectionCode is "world_session_expired" or "world_unavailable")
                        {
                            await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "world_session_unavailable", cancellationToken);
                            break;
                        }
                        continue;
                    }
                    await SendAsync(socket, new ServerEnvelope("WorldState", ProtocolConstants.CurrentVersion,
                        message.Sequence, null, ProtocolCodec.ToPayload(result.Snapshot!)), cancellationToken);
                    continue;
                }

                await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence,
                    worldJoined ? "unsupported_message_type" : "world_join_required"), cancellationToken);
                await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "unsupported_message", cancellationToken);
                break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Client disconnect and server shutdown are expected transport outcomes.
        }
        catch (WebSocketException exception)
        {
            logger.LogDebug("WebSocket transport ended before a close frame ({SocketError})", exception.WebSocketErrorCode);
        }
        finally
        {
            if (attachedSessionId is { } sessionId)
                registry.Detach(sessionId, connectionId);
        }
        logger.LogDebug("WebSocket session closed with status {SocketState}", socket.State);
    }

    private static async Task<ClientEnvelope?> ReceiveEnvelopeAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        using var message = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(buffer.AsMemory(), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                return null;
            if (result.MessageType != WebSocketMessageType.Text)
                return null;
            if (message.Length + result.Count > ProtocolConstants.MaximumFrameBytes)
                return null;
            message.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
                break;
        }
        return ProtocolCodec.TryDecodeClient(message.ToArray(), out var envelope) ? envelope : null;
    }

    private static async Task SendAsync(WebSocket socket, ServerEnvelope envelope, CancellationToken cancellationToken)
    {
        var payload = ProtocolCodec.Encode(envelope);
        await socket.SendAsync(payload.AsMemory(), WebSocketMessageType.Text, true, cancellationToken);
    }

    private static async Task CloseIfOpenAsync(WebSocket socket, WebSocketCloseStatus status, string description, CancellationToken cancellationToken)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            await socket.CloseOutputAsync(status, description, cancellationToken);
    }
}
