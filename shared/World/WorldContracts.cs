namespace ProjetoRpg.Shared.World;

public sealed record WorldPosition(double X, double Y, double Z);

public sealed record EnterWorldRequest(Guid CharacterId);

public sealed record WorldJoinPayload(Guid WorldSessionId);

/// <summary>Client sends only a bounded directional intention, never a target position or speed.</summary>
public sealed record MoveIntentPayload(float DirectionX, float DirectionZ, long InputSequence);

public sealed record WorldSessionSnapshot(
    Guid WorldSessionId,
    Guid CharacterId,
    string CharacterName,
    string? ClassId,
    string MapId,
    string SpawnId,
    WorldPosition Position,
    WorldPosition Velocity,
    float MovementSpeed,
    long ServerTick,
    long LastInputSequence,
    DateTimeOffset ExpiresAt,
    bool Resumed);

public sealed record EnterWorldResult(WorldSessionSnapshot Snapshot, bool Resumed);

public enum WorldEntryOutcome
{
    Created,
    Resumed,
    CharacterNotFound,
    WorldUnavailable
}

public sealed record WorldMoveResult(bool Accepted, string? RejectionCode, WorldSessionSnapshot? Snapshot);
