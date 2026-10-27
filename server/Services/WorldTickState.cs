namespace ProjetoRpg.Server.Services;

public sealed class WorldTickState
{
    private long _tickCount;
    private long _lastTickUnixMilliseconds;

    public long TickCount => Interlocked.Read(ref _tickCount);
    public long LastTickUnixMilliseconds => Interlocked.Read(ref _lastTickUnixMilliseconds);
    public bool HasTicked => TickCount > 0;

    public long MarkTick()
    {
        Interlocked.Exchange(ref _lastTickUnixMilliseconds, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        return Interlocked.Increment(ref _tickCount);
    }
}
