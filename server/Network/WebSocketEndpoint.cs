using System.Net.WebSockets;
using ProjetoRpg.Shared.Protocol;

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
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var cancellationToken = context.RequestAborted;

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

            await SendAsync(socket, new ServerEnvelope("Error", ProtocolConstants.CurrentVersion, message.Sequence, "unsupported_message_type"), cancellationToken);
            await CloseIfOpenAsync(socket, WebSocketCloseStatus.PolicyViolation, "unsupported_message", cancellationToken);
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
