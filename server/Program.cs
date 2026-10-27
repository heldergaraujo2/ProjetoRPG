using ProjetoRpg.Server.Content;
using ProjetoRpg.Server.Network;
using ProjetoRpg.Server.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 64 * 1024);

var contentDirectory = Environment.GetEnvironmentVariable("GAME_CONTENT_DIRECTORY")
    ?? Path.Combine(AppContext.BaseDirectory, "content");
var contentCatalog = ContentCatalog.Load(contentDirectory);
builder.Services.AddSingleton(contentCatalog);
builder.Services.AddSingleton<WorldTickState>();
builder.Services.AddHostedService<WorldTickService>();

var app = builder.Build();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(20) });

app.MapGet("/", () => Results.Ok(new
{
    service = "ProjetoRPG dedicated server",
    stage = "foundation-in-progress",
    protocolVersion = ProjetoRpg.Shared.Protocol.ProtocolConstants.CurrentVersion
}));
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/tick", (WorldTickState ticks) => Results.Ok(new
{
    tickCount = ticks.TickCount,
    lastTickUnixMilliseconds = ticks.LastTickUnixMilliseconds,
    configuredRateHz = WorldTickService.TickRateHz,
    persistenceConfigured = false
}));
app.MapGet("/health/content", (ContentCatalog catalog) => Results.Ok(new
{
    contentVersion = catalog.Version,
    entries = catalog.Entries.Count
}));
app.MapGet("/ws", WebSocketEndpoint.HandleAsync);

app.Lifetime.ApplicationStarted.Register(() =>
    app.Logger.LogInformation("ProjetoRPG server foundation started; content {ContentVersion} ({EntryCount} entries)", contentCatalog.Version, contentCatalog.Entries.Count));

await app.RunAsync();
