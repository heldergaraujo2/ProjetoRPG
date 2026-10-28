using Npgsql;

namespace ProjetoRpg.Server.Configuration;

public sealed record ServerConfiguration(
    string DatabaseConnectionString,
    string? MigrationConnectionString,
    TimeSpan SessionLifetime,
    int AuthRequestsPerMinute)
{
    public static ServerConfiguration Load(IConfiguration configuration, IHostEnvironment environment)
    {
        var rawConnectionString = configuration.GetConnectionString("GameDatabase");
        if (string.IsNullOrWhiteSpace(rawConnectionString))
            throw new InvalidOperationException("Required configuration ConnectionStrings:GameDatabase is missing.");
        ValidateConnectionString(rawConnectionString, "ConnectionStrings:GameDatabase", environment);

        var migrationConnectionString = configuration.GetConnectionString("MigrationDatabase");
        if (!string.IsNullOrWhiteSpace(migrationConnectionString))
            ValidateConnectionString(migrationConnectionString, "ConnectionStrings:MigrationDatabase", environment);

        var lifetimeMinutes = ReadInteger(configuration, "Auth:SessionLifetimeMinutes", 720, 5, 10080);
        var authLimit = ReadInteger(configuration, "Auth:RequestsPerMinute", 12, 1, 600);
        return new ServerConfiguration(rawConnectionString, migrationConnectionString, TimeSpan.FromMinutes(lifetimeMinutes), authLimit);
    }

    private static void ValidateConnectionString(string rawConnectionString, string key, IHostEnvironment environment)
    {
        NpgsqlConnectionStringBuilder connection;
        try { connection = new NpgsqlConnectionStringBuilder(rawConnectionString); }
        catch (ArgumentException) { throw new InvalidOperationException($"Configuration {key} is invalid."); }

        if (string.IsNullOrWhiteSpace(connection.Host) ||
            string.IsNullOrWhiteSpace(connection.Database) ||
            string.IsNullOrWhiteSpace(connection.Username) ||
            string.IsNullOrWhiteSpace(connection.Password))
            throw new InvalidOperationException($"Configuration {key} must provide host, database, username and password.");

        var sslMode = connection.SslMode.ToString();
        if (environment.IsProduction() && sslMode is not ("VerifyFull" or "VerifyCA"))
            throw new InvalidOperationException($"Production {key} must use TLS certificate verification (SslMode=VerifyFull or VerifyCA).");
    }

    private static int ReadInteger(IConfiguration configuration, string key, int fallback, int minimum, int maximum)
    {
        var raw = configuration[key];
        if (raw is null) return fallback;
        if (!int.TryParse(raw, out var value) || value < minimum || value > maximum)
            throw new InvalidOperationException($"Configuration {key} must be an integer between {minimum} and {maximum}.");
        return value;
    }
}
