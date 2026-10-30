using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Godot;
using ProjetoRpg.Shared.Protocol;
using ProjetoRpg.Shared.World;

namespace ProjetoRpg.Client.Scripts;

/// <summary>Authenticated WebSocket transport; only directional intentions are sent to the server.</summary>
public sealed class WorldRealtimeClient : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentQueue<WorldSessionSnapshot> _states = new();
    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private ClientWebSocket? _socket;
    private CancellationTokenSource? _lifetime;
    private Task? _receiveLoop;
    private long _envelopeSequence;
    private long _inputSequence;
    private int _disposed;
    private string? _lastError;

    public bool IsConnected => _socket?.State == WebSocketState.Open && Volatile.Read(ref _disposed) == 0;
    public string? LastError => Volatile.Read(ref _lastError);

    public async Task<WorldSessionSnapshot> ConnectAsync(
        string baseUrl, string accessToken, Guid worldSessionId, CancellationToken cancellationToken)
    {
        var httpUri = new Uri(baseUrl, UriKind.Absolute);
        var builder = new UriBuilder(httpUri)
        {
            Scheme = httpUri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws",
            Path = "/ws",
            Query = string.Empty,
            Fragment = string.Empty
        };
        var socket = new ClientWebSocket();
        _socket = socket;
        socket.Options.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _lifetime = lifetime;
        await socket.ConnectAsync(builder.Uri, lifetime.Token);

        await SendEnvelopeAsync("Hello", null, lifetime.Token);
        var hello = await ReceiveHandshakeAsync(lifetime.Token);
        if (hello.Type != "HelloAck")
            throw new InvalidOperationException("O servidor recusou o protocolo de sessão de mundo.");

        await SendEnvelopeAsync("WorldJoin", ProtocolCodec.ToPayload(new WorldJoinPayload(worldSessionId)), lifetime.Token);
        var joined = await ReceiveHandshakeAsync(lifetime.Token);
        if (joined.Type != "WorldJoined" || !ProtocolCodec.TryDecodePayload<WorldSessionSnapshot>(joined.Payload, out var snapshot) || snapshot is null)
            throw new InvalidOperationException(TranslateServerError(joined.Detail));

        _inputSequence = snapshot.LastInputSequence;
        _receiveLoop = ReceiveLoopAsync(lifetime.Token);
        return snapshot;
    }

    public async Task SendMoveAsync(Vector2 direction, CancellationToken cancellationToken = default)
    {
        var lifetime = _lifetime;
        if (!IsConnected || lifetime is null) return;
        var sequence = Interlocked.Increment(ref _inputSequence);
        var payload = ProtocolCodec.ToPayload(new MoveIntentPayload(direction.X, direction.Y, sequence));
        await SendEnvelopeAsync("MoveIntent", payload, cancellationToken == default ? lifetime.Token : cancellationToken);
    }

    public bool TryDequeueState(out WorldSessionSnapshot? snapshot) => _states.TryDequeue(out snapshot);

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        var socket = _socket;
        var lifetime = _lifetime;
        if (socket is not null && socket.State == WebSocketState.Open && lifetime is not null)
        {
            try
            {
                await SendEnvelopeAsync("Bye", null, CancellationToken.None);
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "client_logout", CancellationToken.None);
            }
            catch (Exception exception) when (exception is WebSocketException or ObjectDisposedException or InvalidOperationException)
            {
                // Disposal is best-effort; the server retains the resumable session until its grace period expires.
            }
        }
        lifetime?.Cancel();
        if (_receiveLoop is not null)
        {
            try { await _receiveLoop; }
            catch (OperationCanceledException) { }
        }
        socket?.Dispose();
        lifetime?.Dispose();
    }

    private async Task SendEnvelopeAsync(string type, JsonElement? payload, CancellationToken cancellationToken)
    {
        var socket = _socket;
        if (socket is null || socket.State != WebSocketState.Open)
            throw new InvalidOperationException("A conexão de mundo não está aberta.");
        var envelope = new ClientEnvelope(type, ProtocolConstants.CurrentVersion,
            Interlocked.Increment(ref _envelopeSequence), payload);
        var bytes = ProtocolCodec.Encode(envelope);
        await _sendGate.WaitAsync(cancellationToken);
        try
        {
            await socket.SendAsync(bytes.AsMemory(), WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            _sendGate.Release();
        }
    }

    private async Task<ServerEnvelope> ReceiveHandshakeAsync(CancellationToken cancellationToken)
    {
        var envelope = await ReceiveEnvelopeAsync(cancellationToken)
            ?? throw new InvalidOperationException("A conexão foi encerrada antes de autorizar a sessão de mundo.");
        if (envelope.Type == "Error")
            throw new InvalidOperationException(TranslateServerError(envelope.Detail));
        return envelope;
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var envelope = await ReceiveEnvelopeAsync(cancellationToken);
                if (envelope is null)
                {
                    Volatile.Write(ref _lastError, "connection_closed");
                    return;
                }
                if (envelope.Type == "WorldState" &&
                    ProtocolCodec.TryDecodePayload<WorldSessionSnapshot>(envelope.Payload, out var snapshot) && snapshot is not null)
                    _states.Enqueue(snapshot);
                else if (envelope.Type == "Error" || envelope.Type == "InputRejected" && envelope.Detail != "movement_rate_limited")
                    Volatile.Write(ref _lastError, TranslateServerError(envelope.Detail));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when the player returns to the lobby or reconnects.
        }
        catch (Exception exception) when (exception is WebSocketException or InvalidOperationException or JsonException)
        {
            Volatile.Write(ref _lastError, "connection_lost");
        }
    }

    private async Task<ServerEnvelope?> ReceiveEnvelopeAsync(CancellationToken cancellationToken)
    {
        var socket = _socket;
        if (socket is null) return null;
        var buffer = new byte[1024];
        using var message = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(buffer.AsMemory(), cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close) return null;
            if (result.MessageType != WebSocketMessageType.Text || message.Length + result.Count > ProtocolConstants.MaximumFrameBytes)
                throw new InvalidOperationException("O servidor enviou uma mensagem de protocolo inválida.");
            message.Write(buffer, 0, result.Count);
            if (result.EndOfMessage) break;
        }
        return JsonSerializer.Deserialize<ServerEnvelope>(message.ToArray(), JsonOptions);
    }

    private static string TranslateServerError(string? code) => code switch
    {
        "world_session_unavailable" or "world_session_expired" => "A sessão de mundo expirou. Retorne ao salão e entre novamente.",
        "world_session_already_connected" => "Este personagem já está conectado em outra sessão.",
        "authentication_expired" => "A sessão da conta expirou. Retorne ao salão e entre novamente.",
        "world_unavailable" => "O mapa ou a navegação não está disponível no servidor.",
        "protocol_version_or_handshake_invalid" or "protocol_version_mismatch" => "A versão do protocolo do cliente não corresponde à do servidor.",
        "connection_lost" or "connection_closed" => "A conexão com o servidor foi interrompida.",
        "movement_rate_limited" => "O servidor está limitando a frequência de movimento.",
        "invalid_move_intent" or "input_sequence_must_increase" => "O servidor recusou uma intenção de movimento inválida.",
        _ => "Não foi possível estabelecer a sessão de mundo. Verifique a conexão e tente novamente."
    };
}
