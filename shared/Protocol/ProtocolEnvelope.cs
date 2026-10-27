using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetoRpg.Shared.Protocol;

public static class ProtocolConstants
{
    public const int CurrentVersion = 1;
    public const int MaximumFrameBytes = 4096;
    public const int MaximumMessageTypeLength = 32;
}

public sealed record ClientEnvelope(string Type, int ProtocolVersion, long Sequence);

public sealed record ServerEnvelope(string Type, int ProtocolVersion, long Sequence, string? Detail = null);

public static class ProtocolCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static byte[] Encode<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

    public static bool TryDecodeClient(ReadOnlySpan<byte> utf8Json, out ClientEnvelope? envelope)
    {
        envelope = null;
        if (utf8Json.Length is 0 or > ProtocolConstants.MaximumFrameBytes)
            return false;

        try
        {
            var parsed = JsonSerializer.Deserialize<ClientEnvelope>(utf8Json, JsonOptions);
            if (parsed is null || string.IsNullOrWhiteSpace(parsed.Type) ||
                parsed.Type.Length > ProtocolConstants.MaximumMessageTypeLength ||
                parsed.ProtocolVersion < 0 || parsed.Sequence < 0)
                return false;

            envelope = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
