using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProjetoRpg.Server.Services;

public sealed class WorldTickService(WorldTickState state, ILogger<WorldTickService> logger) : BackgroundService
{
    public const int TickRateHz = 20;
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(1000d / TickRateHz);
    private static readonly Meter Meter = new("ProjetoRpg.Server", "0.1.0");
    private static readonly Counter<long> TickCounter = Meter.CreateCounter<long>("projetorpg.world.ticks");
    private static readonly Histogram<double> TickDuration = Meter.CreateHistogram<double>("projetorpg.world.tick.duration", "ms");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var startedAt = Stopwatch.GetTimestamp();
                var tick = state.MarkTick();
                TickCounter.Add(1);
                TickDuration.Record(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

                if (tick % TickRateHz == 0)
                    logger.LogInformation("World heartbeat at {TickCount} ticks; configured rate {TickRateHz} Hz", tick, TickRateHz);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("World tick loop stopped cleanly at tick {TickCount}", state.TickCount);
        }
    }
}
