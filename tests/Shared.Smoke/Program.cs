using System.Text;
using System.Text.Json;
using ProjetoRpg.Shared.Protocol;
using ProjetoRpg.Shared.World;

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

var original = new ClientEnvelope("Ping", ProtocolConstants.CurrentVersion, 7);
var encoded = ProtocolCodec.Encode(original);
Require(ProtocolCodec.TryDecodeClient(encoded, out var decoded), "valid protocol frame must decode");
Require(decoded == original, "JSON round trip must preserve the protocol envelope");
Require(!ProtocolCodec.TryDecodeClient(Encoding.UTF8.GetBytes("{"), out _), "malformed JSON must be rejected");
Require(!ProtocolCodec.TryDecodeClient(new byte[ProtocolConstants.MaximumFrameBytes + 1], out _), "oversized frames must be rejected");
Require(!ProtocolCodec.TryDecodeClient(ProtocolCodec.Encode(new ClientEnvelope("", ProtocolConstants.CurrentVersion, 1)), out _), "empty message types must be rejected");
Require(!ProtocolCodec.TryDecodeClient(ProtocolCodec.Encode(new ClientEnvelope("Ping", ProtocolConstants.CurrentVersion, -1)), out _), "negative sequences must be rejected");

var direction = new MoveIntentPayload(0.5f, -0.5f, 14);
var move = new ClientEnvelope("MoveIntent", ProtocolConstants.CurrentVersion, 8, ProtocolCodec.ToPayload(direction));
Require(ProtocolCodec.TryDecodeClient(ProtocolCodec.Encode(move), out var decodedMove), "movement envelope must decode");
Require(ProtocolCodec.TryDecodePayload<MoveIntentPayload>(decodedMove!.Payload, out var decodedDirection),
    "typed movement intention must decode");
Require(decodedDirection == direction, "movement payload round trip must preserve its bounded intent fields");

using var arbitraryPosition = JsonDocument.Parse("{\"directionX\":0,\"directionZ\":1,\"inputSequence\":15,\"targetPosition\":{\"x\":999,\"y\":999,\"z\":999}}");
Require(!ProtocolCodec.TryDecodePayload<MoveIntentPayload>(arbitraryPosition.RootElement, out _),
    "strict movement payload must reject client-provided final-position fields");

Console.WriteLine("SHARED_PROTOCOL_SMOKE_OK protocol=2 world-intent=strict");
