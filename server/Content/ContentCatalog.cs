using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjetoRpg.Server.Content;

public sealed record ContentEntry(string Id, string Kind, string RelativePath);

public sealed class ContentCatalog
{
    private static readonly Regex StableId = new("^[a-z0-9]+(?:[._][a-z0-9]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private ContentCatalog(string version, IReadOnlyList<ContentEntry> entries, string rootDirectory)
    {
        Version = version;
        Entries = entries;
        RootDirectory = rootDirectory;
    }

    public string Version { get; }
    public IReadOnlyList<ContentEntry> Entries { get; }
    public string RootDirectory { get; }

    public static ContentCatalog Load(string directory)
    {
        var root = Path.GetFullPath(directory);
        var manifestPath = Path.Combine(root, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new InvalidDataException($"Content manifest not found: {manifestPath}");

        using var manifest = JsonDocument.Parse(File.ReadAllBytes(manifestPath), new JsonDocumentOptions { MaxDepth = 32 });
        var document = manifest.RootElement;
        if (document.ValueKind != JsonValueKind.Object ||
            !document.TryGetProperty("schemaVersion", out var schemaVersion) ||
            !schemaVersion.TryGetInt32(out var schema) || schema != 1)
            throw new InvalidDataException("Content manifest schemaVersion must be 1.");

        if (!document.TryGetProperty("contentVersion", out var versionElement) ||
            versionElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(versionElement.GetString()))
            throw new InvalidDataException("Content manifest requires a non-empty contentVersion.");

        if (!document.TryGetProperty("entries", out var entriesElement) || entriesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("Content manifest entries must be an array.");

        var entries = new List<ContentEntry>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in entriesElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object ||
                !TryString(item, "id", out var id) ||
                !TryString(item, "kind", out var kind) ||
                !TryString(item, "path", out var relativePath))
                throw new InvalidDataException("Each content entry requires string id, kind, and path fields.");

            if (!StableId.IsMatch(id))
                throw new InvalidDataException($"Invalid stable content id: {id}");
            if (!ids.Add(id))
                throw new InvalidDataException($"Duplicate content id: {id}");
            if (Path.IsPathRooted(relativePath))
                throw new InvalidDataException($"Content paths must be relative: {relativePath}");

            var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
            var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootPrefix, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new InvalidDataException($"Content path escapes the catalog directory: {relativePath}");
            if (!File.Exists(fullPath))
                throw new InvalidDataException($"Content file does not exist for {id}: {relativePath}");

            using var contentDocument = JsonDocument.Parse(File.ReadAllBytes(fullPath), new JsonDocumentOptions { MaxDepth = 64 });
            if (contentDocument.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException($"Content file must contain a JSON object: {relativePath}");

            entries.Add(new ContentEntry(id, kind, relativePath));
        }

        return new ContentCatalog(versionElement.GetString()!, entries, root);
    }

    private static bool TryString(JsonElement element, string property, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(property, out var field) || field.ValueKind != JsonValueKind.String)
            return false;
        value = field.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }
}
