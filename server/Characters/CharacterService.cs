using System.Text;
using System.Text.RegularExpressions;
using Npgsql;
using NpgsqlTypes;
using ProjetoRpg.Server.Content;

namespace ProjetoRpg.Server.Characters;

public sealed record CharacterRecord(Guid CharacterId, string Name, string? ClassId, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record CharacterCreationResult(CharacterWriteOutcome Outcome, CharacterRecord? Character);
public enum CharacterWriteOutcome { Created, Updated, NameUnavailable, ClassUnavailable, NotFound }

public static partial class CharacterInput
{
    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{2,19}$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    public static bool TryNormalizeName(string? input, out string name, out string normalized)
    {
        name = string.Empty;
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input) || input.Length > 40) return false;
        try { name = input.Normalize(NormalizationForm.FormKC); }
        catch (ArgumentException) { return false; }
        if (!NamePattern().IsMatch(name)) return false;
        normalized = name.ToUpperInvariant();
        return true;
    }
}

public sealed class CharacterService(NpgsqlDataSource dataSource, ContentCatalog catalog)
{
    public async Task<CharacterCreationResult> CreateAsync(
        Guid accountId, string name, string normalizedName, string? classId, CancellationToken cancellationToken)
    {
        if (classId is not null && !catalog.ContainsClass(classId)) return new(CharacterWriteOutcome.ClassUnavailable, null);
        var characterId = Guid.NewGuid();
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            CharacterRecord character;
            await using (var insert = new NpgsqlCommand(
                "INSERT INTO characters (character_id, account_id, name, name_normalized, class_id) VALUES (@id, @accountId, @name, @normalized, @classId) RETURNING created_at, updated_at",
                connection, transaction))
            {
                insert.Parameters.AddWithValue("id", characterId);
                insert.Parameters.AddWithValue("accountId", accountId);
                insert.Parameters.AddWithValue("name", name);
                insert.Parameters.AddWithValue("normalized", normalizedName);
                insert.Parameters.Add(new NpgsqlParameter("classId", NpgsqlDbType.Varchar) { Value = (object?)classId ?? DBNull.Value });
                await using var reader = await insert.ExecuteReaderAsync(cancellationToken);
                await reader.ReadAsync(cancellationToken);
                character = new CharacterRecord(characterId, name, classId,
                    ReadTimestamp(reader, 0), ReadTimestamp(reader, 1));
            }
            await WriteAuditAsync(connection, transaction, "character_created", accountId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CharacterCreationResult(CharacterWriteOutcome.Created, character);
        }
        catch (PostgresException exception) when (
            exception.SqlState == PostgresErrorCodes.UniqueViolation &&
            exception.ConstraintName == "uq_characters_name_normalized")
        {
            return new CharacterCreationResult(CharacterWriteOutcome.NameUnavailable, null);
        }
    }

    public async Task<IReadOnlyList<CharacterRecord>> ListForAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            "SELECT character_id, name, class_id, created_at, updated_at FROM characters WHERE account_id = @accountId ORDER BY created_at, character_id",
            connection);
        command.Parameters.AddWithValue("accountId", accountId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var characters = new List<CharacterRecord>();
        while (await reader.ReadAsync(cancellationToken)) characters.Add(ReadCharacter(reader));
        return characters;
    }

    public async Task<CharacterRecord?> GetForAccountAsync(Guid accountId, Guid characterId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            "SELECT character_id, name, class_id, created_at, updated_at FROM characters WHERE account_id = @accountId AND character_id = @characterId",
            connection);
        command.Parameters.AddWithValue("accountId", accountId);
        command.Parameters.AddWithValue("characterId", characterId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadCharacter(reader) : null;
    }

    public async Task<CharacterWriteOutcome> RenameForAccountAsync(
        Guid accountId, Guid characterId, string name, string normalizedName, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using (var update = new NpgsqlCommand(
                "UPDATE characters SET name = @name, name_normalized = @normalized, updated_at = now() WHERE account_id = @accountId AND character_id = @characterId",
                connection, transaction))
            {
                update.Parameters.AddWithValue("name", name);
                update.Parameters.AddWithValue("normalized", normalizedName);
                update.Parameters.AddWithValue("accountId", accountId);
                update.Parameters.AddWithValue("characterId", characterId);
                if (await update.ExecuteNonQueryAsync(cancellationToken) == 0)
                    return CharacterWriteOutcome.NotFound;
            }
            await WriteAuditAsync(connection, transaction, "character_renamed", accountId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return CharacterWriteOutcome.Updated;
        }
        catch (PostgresException exception) when (
            exception.SqlState == PostgresErrorCodes.UniqueViolation &&
            exception.ConstraintName == "uq_characters_name_normalized")
        {
            return CharacterWriteOutcome.NameUnavailable;
        }
    }

    private static CharacterRecord ReadCharacter(NpgsqlDataReader reader) => new(
        reader.GetGuid(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2),
        ReadTimestamp(reader, 3), ReadTimestamp(reader, 4));

    private static DateTimeOffset ReadTimestamp(NpgsqlDataReader reader, int ordinal) =>
        new(DateTime.SpecifyKind(reader.GetDateTime(ordinal), DateTimeKind.Utc));

    private static async Task WriteAuditAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, string eventType, Guid accountId, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "INSERT INTO security_audit_events (event_type, actor_account_id) VALUES (@eventType, @accountId)",
            connection, transaction);
        command.Parameters.AddWithValue("eventType", eventType);
        command.Parameters.AddWithValue("accountId", accountId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
