using System.Text.Json;
using Npgsql;

namespace ProjetoRpg.Server.Persistence;

public static class DatabaseCommandRunner
{
    public static bool IsDatabaseCommand(string[] args) => args.Length > 0 && (args[0] is "--db-migrate" or "--db-status" or "--db-rollback");

    public static async Task<int> RunAsync(string[] args, NpgsqlDataSource dataSource, ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (args[0])
            {
                case "--db-migrate":
                    await MigrationRunner.ApplyAsync(dataSource, logger, cancellationToken);
                    Console.WriteLine("DB_MIGRATIONS_APPLIED");
                    return 0;
                case "--db-status":
                    var applied = await MigrationRunner.GetStatusAsync(dataSource, cancellationToken);
                    Console.WriteLine(JsonSerializer.Serialize(new
                    {
                        applied,
                        pending = new[] { 1 }.Except(applied.Select(item => item.Version)).ToArray()
                    }));
                    return 0;
                case "--db-rollback":
                    var confirmed = args.Skip(1).Contains("--confirm-data-loss", StringComparer.Ordinal);
                    await MigrationRunner.RollbackLatestAsync(dataSource, logger, confirmed, cancellationToken);
                    Console.WriteLine("DB_MIGRATION_ROLLED_BACK");
                    return 0;
                default:
                    return 64;
            }
        }
        catch (NpgsqlException exception)
        {
            var sqlState = exception is PostgresException postgresException ? postgresException.SqlState : "unknown";
            logger.LogError("Database command failed; SQL state {SqlState}", sqlState);
            Console.Error.WriteLine("DATABASE_COMMAND_FAILED: check PostgreSQL availability, schema permissions and connection configuration.");
            return 1;
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError("Database command refused: {Reason}", exception.Message);
            Console.Error.WriteLine($"DATABASE_COMMAND_REFUSED: {exception.Message}");
            return 1;
        }
    }
}
