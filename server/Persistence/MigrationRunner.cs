using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace ProjetoRpg.Server.Persistence;

public static class MigrationRunner
{
    private const long AdvisoryLockKey = 731924108;
    private const int LatestVersion = 2;
    private static readonly MigrationDefinition[] Migrations = [new(1, "0001_initial"), new(2, "0002_world_sessions")];

    public static async Task ApplyAsync(NpgsqlDataSource dataSource, ILogger logger, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await AcquireLockAsync(connection, cancellationToken);
        try
        {
            await EnsureHistoryTableAsync(connection, cancellationToken);
            var applied = await ReadAppliedAsync(connection, cancellationToken);
            ValidateHistory(applied);

            foreach (var migration in Migrations)
            {
                if (applied.TryGetValue(migration.Version, out var current))
                {
                    ValidateChecksum(migration, current);
                    continue;
                }

                var up = ReadSql(migration, "up");
                var down = ReadSql(migration, "down");
                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
                await ExecuteAsync(connection, transaction, up, cancellationToken);
                await using (var record = new NpgsqlCommand(
                    "INSERT INTO schema_migrations (version, name, up_checksum, down_checksum) VALUES (@version, @name, @up, @down)",
                    connection, transaction))
                {
                    record.Parameters.AddWithValue("version", migration.Version);
                    record.Parameters.AddWithValue("name", migration.Name);
                    record.Parameters.AddWithValue("up", Checksum(up));
                    record.Parameters.AddWithValue("down", Checksum(down));
                    await record.ExecuteNonQueryAsync(cancellationToken);
                }
                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Database migration applied: {MigrationVersion} {MigrationName}", migration.Version, migration.Name);
            }
        }
        finally
        {
            await ReleaseLockAsync(connection, cancellationToken);
        }
    }

    public static async Task RollbackLatestAsync(
        NpgsqlDataSource dataSource,
        ILogger logger,
        bool confirmDataLoss,
        CancellationToken cancellationToken = default)
    {
        if (!confirmDataLoss)
            throw new InvalidOperationException("Rollback can delete player data. Re-run with --confirm-data-loss after taking a verified backup.");

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await AcquireLockAsync(connection, cancellationToken);
        try
        {
            if (!await HistoryTableExistsAsync(connection, cancellationToken))
                throw new InvalidOperationException("No migration history exists; there is nothing to roll back.");

            var applied = await ReadAppliedAsync(connection, cancellationToken);
            ValidateHistory(applied);
            if (applied.Count == 0)
                throw new InvalidOperationException("No applied migration exists; there is nothing to roll back.");

            var latest = applied.Values.OrderByDescending(item => item.Version).First();
            var definition = Migrations.Single(item => item.Version == latest.Version);
            ValidateChecksum(definition, latest);
            var down = ReadSql(definition, "down");
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await ExecuteAsync(connection, transaction, down, cancellationToken);
            await using (var remove = new NpgsqlCommand("DELETE FROM schema_migrations WHERE version = @version", connection, transaction))
            {
                remove.Parameters.AddWithValue("version", latest.Version);
                await remove.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
            logger.LogWarning("Database migration rolled back: {MigrationVersion} {MigrationName}", latest.Version, latest.Name);
        }
        finally
        {
            await ReleaseLockAsync(connection, cancellationToken);
        }
    }

    public static async Task<IReadOnlyList<AppliedMigration>> GetStatusAsync(
        NpgsqlDataSource dataSource,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        if (!await HistoryTableExistsAsync(connection, cancellationToken)) return [];
        var applied = await ReadAppliedAsync(connection, cancellationToken);
        ValidateHistory(applied);
        return applied.Values.OrderBy(item => item.Version).ToArray();
    }

    public static async Task<DatabaseReadiness> CheckReadinessAsync(
        NpgsqlDataSource dataSource,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using (var ping = new NpgsqlCommand("SELECT 1", connection))
            await ping.ExecuteScalarAsync(cancellationToken);

        if (!await HistoryTableExistsAsync(connection, cancellationToken))
            return new DatabaseReadiness(true, false, null);

        var applied = await ReadAppliedAsync(connection, cancellationToken);
        ValidateHistory(applied);
        var version = applied.Count == 0 ? 0 : applied.Keys.Max();
        return new DatabaseReadiness(true, version >= LatestVersion, version);
    }

    public static async Task<bool> CanConnectAsync(NpgsqlDataSource dataSource, CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var ping = new NpgsqlCommand("SELECT 1", connection);
        return Convert.ToInt32(await ping.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    private static async Task EnsureHistoryTableAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version integer PRIMARY KEY CHECK (version > 0),
                name text NOT NULL UNIQUE,
                up_checksum text NOT NULL CHECK (length(up_checksum) = 64),
                down_checksum text NOT NULL CHECK (length(down_checksum) = 64),
                applied_at timestamptz NOT NULL DEFAULT now()
            )
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> HistoryTableExistsAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("SELECT to_regclass('public.schema_migrations') IS NOT NULL", connection);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    private static async Task<Dictionary<int, AppliedMigration>> ReadAppliedAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, AppliedMigration>();
        await using var command = new NpgsqlCommand(
            "SELECT version, name, up_checksum, down_checksum FROM schema_migrations ORDER BY version", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var item = new AppliedMigration(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
            if (!result.TryAdd(item.Version, item))
                throw new InvalidOperationException("Migration history contains a duplicate version.");
        }
        return result;
    }

    private static void ValidateHistory(IReadOnlyDictionary<int, AppliedMigration> applied)
    {
        foreach (var item in applied.Values)
        {
            if (Migrations.All(expected => expected.Version != item.Version))
                throw new InvalidOperationException($"Database contains unknown migration version {item.Version}; refusing to modify schema.");
            var expected = Migrations.Single(candidate => candidate.Version == item.Version);
            ValidateChecksum(expected, item);
        }
    }

    private static void ValidateChecksum(MigrationDefinition migration, AppliedMigration applied)
    {
        if (!string.Equals(migration.Name, applied.Name, StringComparison.Ordinal) ||
            !string.Equals(Checksum(ReadSql(migration, "up")), applied.UpChecksum, StringComparison.Ordinal) ||
            !string.Equals(Checksum(ReadSql(migration, "down")), applied.DownChecksum, StringComparison.Ordinal))
            throw new InvalidOperationException($"Migration {migration.Version} differs from the version already applied; refusing an unsafe schema change.");
    }

    private static string ReadSql(MigrationDefinition migration, string direction)
    {
        var resource = $"{typeof(MigrationRunner).Namespace}.Migrations.{migration.Name}.{direction}.sql";
        using var stream = typeof(MigrationRunner).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Embedded migration resource is missing: {resource}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string Checksum(string sql) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql))).ToLowerInvariant();

    private static async Task ExecuteAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task AcquireLockAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("SELECT pg_advisory_lock(@key)", connection);
        command.Parameters.AddWithValue("key", AdvisoryLockKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task ReleaseLockAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@key)", connection);
        command.Parameters.AddWithValue("key", AdvisoryLockKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record MigrationDefinition(int Version, string Name);
}

public sealed record AppliedMigration(int Version, string Name, string UpChecksum, string DownChecksum);
public sealed record DatabaseReadiness(bool DatabaseAvailable, bool SchemaCurrent, int? SchemaVersion);
