using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetoRpg.Shared.Protocol;

public static class ProtocolConstants
{
    public const int CurrentVersion = 2;
    public const int MaximumFrameBytes = 4096;
    public const int MaximumMessageTypeLength = 32;
}

public sealed record ClientEnvelope(
    string Type,
    int ProtocolVersion,
    long Sequence,
    JsonElement? Payload = null);

public sealed record ServerEnvelope(
    string Type,
    int ProtocolVersion,
    long Sequence,
    string? Detail = null,
    JsonElement? Payload = null);

public static class ProtocolCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static byte[] Encode<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);

    public static JsonElement ToPayload<T>(T value) => JsonSerializer.SerializeToElement(value, JsonOptions);

    public static bool TryDecodePayload<T>(JsonElement? payload, out T? value)
    {
        value = default;
        if (!payload.HasValue || payload.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return false;
        try
        {
            value = payload.Value.Deserialize<T>(JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

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
                parsed.ProtocolVersion < 0 || parsed.Sequence < 0 ||
                parsed.Payload is { ValueKind: JsonValueKind.Undefined })
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
