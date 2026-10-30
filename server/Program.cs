using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;
using ProjetoRpg.Server.Accounts;
using ProjetoRpg.Server.Characters;
using ProjetoRpg.Server.Configuration;
using ProjetoRpg.Server.Content;
using ProjetoRpg.Server.Network;
using ProjetoRpg.Server.Persistence;
using ProjetoRpg.Server.Services;
using ProjetoRpg.Server.World;

var databaseCommand = DatabaseCommandRunner.IsDatabaseCommand(args);
var builder = WebApplication.CreateBuilder(databaseCommand ? [] : args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);

ServerConfiguration configuration;
try
{
    configuration = ServerConfiguration.Load(builder.Configuration, builder.Environment);
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine($"CONFIGURATION_INVALID: {exception.Message}");
    Environment.ExitCode = 78;
    return;
}

if (databaseCommand)
{
    if (builder.Environment.IsProduction() && configuration.MigrationConnectionString is null)
    {
        Console.Error.WriteLine("CONFIGURATION_INVALID: Production migrations require ConnectionStrings:MigrationDatabase to keep DDL privileges separate from the runtime account.");
        Environment.ExitCode = 78;
        return;
    }
    await using var commandDataSource = NpgsqlDataSource.Create(
        configuration.MigrationConnectionString ?? configuration.DatabaseConnectionString);
    using var commandLoggerFactory = LoggerFactory.Create(logging => logging.AddJsonConsole());
    Environment.ExitCode = await DatabaseCommandRunner.RunAsync(args, commandDataSource,
        commandLoggerFactory.CreateLogger("ProjetoRpg.DatabaseMigrations"));
    return;
}

var contentDirectory = Environment.GetEnvironmentVariable("GAME_CONTENT_DIRECTORY")
    ?? Path.Combine(AppContext.BaseDirectory, "content");
ContentCatalog contentCatalog;
WorldMapCatalog worldMapCatalog;
try
{
    contentCatalog = ContentCatalog.Load(contentDirectory);
    worldMapCatalog = WorldMapCatalog.Load(contentCatalog);
}
catch (Exception exception) when (exception is InvalidDataException or JsonException or IOException)
{
    Console.Error.WriteLine($"CONTENT_CONFIGURATION_INVALID: {exception.Message}");
    Environment.ExitCode = 78;
    return;
}

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(configuration.DatabaseConnectionString));
builder.Services.AddSingleton(contentCatalog);
builder.Services.AddSingleton(worldMapCatalog);
builder.Services.AddSingleton<WorldSessionService>();
builder.Services.AddSingleton<WorldConnectionRegistry>();
builder.Services.AddSingleton<IPasswordHasher<AccountCredential>, PasswordHasher<AccountCredential>>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<CharacterService>();
builder.Services.AddSingleton<WorldTickState>();
builder.Services.AddHostedService<WorldTickService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = configuration.AuthRequestsPerMinute,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ProjetoRpg.RequestErrors");
    context.Response.Clear();
    context.Response.ContentType = "application/json";
    if (exception is NpgsqlException databaseException)
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        var sqlState = databaseException is PostgresException postgresException ? postgresException.SqlState : "unknown";
        logger.LogError("Database request failed; SQL state {SqlState}; trace {TraceIdentifier}",
            sqlState, context.TraceIdentifier);
        await context.Response.WriteAsJsonAsync(new { error = "service_unavailable", traceId = context.TraceIdentifier });
        return;
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    logger.LogError("Unhandled request failure {ExceptionType}; trace {TraceIdentifier}",
        exception?.GetType().Name ?? "unknown", context.TraceIdentifier);
    await context.Response.WriteAsJsonAsync(new { error = "internal_error", traceId = context.TraceIdentifier });
}));
app.UseRateLimiter();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(20) });

app.MapGet("/", () => Results.Ok(new
{
    service = "ProjetoRPG dedicated server",
    stage = "coast-gameplay-in-progress",
    protocolVersion = ProjetoRpg.Shared.Protocol.ProtocolConstants.CurrentVersion
}));
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/database", async (NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
{
    try
    {
        var available = await MigrationRunner.CanConnectAsync(dataSource, cancellationToken);
        return available
            ? (IResult)Results.Ok(new { status = "available" })
            : Results.Json(new { status = "unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (NpgsqlException)
    {
        return Results.Json(new { status = "unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});
app.MapGet("/health/ready", async (NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
{
    try
    {
        var readiness = await MigrationRunner.CheckReadinessAsync(dataSource, cancellationToken);
        if (readiness.SchemaCurrent)
            return (IResult)Results.Ok(new { status = "ready", database = "available", schemaVersion = readiness.SchemaVersion });
        return Results.Json(new { status = "not_ready", database = "available", schema = "migration_pending", schemaVersion = readiness.SchemaVersion },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (NpgsqlException)
    {
        return Results.Json(new { status = "not_ready", database = "unavailable", schema = "unknown" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (InvalidOperationException)
    {
        return Results.Json(new { status = "not_ready", database = "available", schema = "invalid" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});
app.MapGet("/health/tick", (WorldTickState ticks) => Results.Ok(new
{
    tickCount = ticks.TickCount,
    lastTickUnixMilliseconds = ticks.LastTickUnixMilliseconds,
    configuredRateHz = WorldTickService.TickRateHz,
    databaseConnectionConfigured = true
}));
app.MapGet("/health/content", (ContentCatalog catalog) => Results.Ok(new
{
    contentVersion = catalog.Version,
    entries = catalog.Entries.Count
}));
app.MapGet("/health/world", (WorldMapCatalog maps) => Results.Ok(new
{
    mapId = WorldSessionService.CoastMapId,
    spawnId = WorldSessionService.CoastSpawnId,
    available = maps.TryGetAvailableMap(WorldSessionService.CoastMapId, out var map) && map is not null
}));
app.MapGet("/ws", WebSocketEndpoint.HandleAsync);
app.MapAccountEndpoints();
app.MapWorldEndpoints();

app.Lifetime.ApplicationStarted.Register(() =>
    app.Logger.LogInformation("ProjetoRPG server started; content {ContentVersion} ({EntryCount} entries)", contentCatalog.Version, contentCatalog.Entries.Count));

await app.RunAsync();
