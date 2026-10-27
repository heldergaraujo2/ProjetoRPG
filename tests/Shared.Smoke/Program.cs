using System.Text;
using ProjetoRpg.Shared.Protocol;

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
Require(!ProtocolCodec.TryDecodeClient(ProtocolCodec.Encode(new ClientEnvelope("", 1, 1)), out _), "empty message types must be rejected");
Require(!ProtocolCodec.TryDecodeClient(ProtocolCodec.Encode(new ClientEnvelope("Ping", 1, -1)), out _), "negative sequences must be rejected");

Console.WriteLine("SHARED_PROTOCOL_SMOKE_OK");
