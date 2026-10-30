using Npgsql;
using ProjetoRpg.Server.Services;
using ProjetoRpg.Shared.World;

namespace ProjetoRpg.Server.World;

public sealed class WorldSessionService(
    NpgsqlDataSource dataSource,
    WorldMapCatalog maps,
    WorldTickState ticks)
{
    public const string CoastMapId = "world.coast";
    public const string CoastSpawnId = "coast.initial";
    private static readonly TimeSpan SessionGracePeriod = TimeSpan.FromMinutes(10);

    public async Task<EnterWorldResult?> EnterAsync(Guid accountId, Guid characterId, CancellationToken cancellationToken)
    {
        if (!maps.TryGetAvailableMap(CoastMapId, out var map) || map is null ||
            !map.Spawns.Any(spawn => spawn.Id == CoastSpawnId))
            throw new InvalidOperationException("The authorized Coast map and spawn are unavailable.");

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        string characterName;
        string? classId;
        await using (var character = new NpgsqlCommand(
            "SELECT name, class_id FROM characters WHERE account_id = @accountId AND character_id = @characterId FOR UPDATE",
            connection, transaction))
        {
            character.Parameters.AddWithValue("accountId", accountId);
            character.Parameters.AddWithValue("characterId", characterId);
            await using var reader = await character.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;
            characterName = reader.GetString(0);
            classId = reader.IsDBNull(1) ? null : reader.GetString(1);
        }

        await using (var expire = new NpgsqlCommand(
            "UPDATE world_sessions SET closed_at = now(), velocity_x = 0, velocity_y = 0, velocity_z = 0 WHERE character_id = @characterId AND closed_at IS NULL AND expires_at <= now()",
            connection, transaction))
        {
            expire.Parameters.AddWithValue("characterId", characterId);
            await expire.ExecuteNonQueryAsync(cancellationToken);
        }

        WorldSessionSnapshot snapshot;
        bool resumed;
        await using (var existing = new NpgsqlCommand(
            "SELECT world_session_id, map_id, spawn_id, position_x, position_y, position_z, last_input_sequence, expires_at FROM world_sessions WHERE character_id = @characterId AND account_id = @accountId AND closed_at IS NULL AND expires_at > now() FOR UPDATE",
            connection, transaction))
        {
            existing.Parameters.AddWithValue("characterId", characterId);
            existing.Parameters.AddWithValue("accountId", accountId);
            await using var reader = await existing.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var sessionMapId = reader.GetString(1);
                if (!maps.TryGetAvailableMap(sessionMapId, out var activeMap) || activeMap is null || reader.GetString(2) != CoastSpawnId)
                    throw new InvalidOperationException("The persisted world session references an unavailable map or spawn.");
                snapshot = new WorldSessionSnapshot(
                    reader.GetGuid(0), characterId, characterName, classId, sessionMapId, reader.GetString(2),
                    new WorldPosition(reader.GetDouble(3), reader.GetDouble(4), reader.GetDouble(5)),
                    new WorldPosition(0, 0, 0), activeMap.WalkSpeed, ticks.TickCount, reader.GetInt64(6),
                    ReadTimestamp(reader, 7), true);
                resumed = true;
            }
            else
            {
                snapshot = null!;
                resumed = false;
            }
        }

        if (resumed)
        {
            DateTimeOffset refreshedExpiry;
            await using (var refresh = new NpgsqlCommand(
                "UPDATE world_sessions SET velocity_x = 0, velocity_y = 0, velocity_z = 0, last_input_tick = @tick, last_active_at = clock_timestamp(), expires_at = clock_timestamp() + @grace WHERE world_session_id = @sessionId RETURNING expires_at",
                connection, transaction))
            {
                refresh.Parameters.AddWithValue("tick", ticks.TickCount);
                refresh.Parameters.AddWithValue("grace", SessionGracePeriod);
                refresh.Parameters.AddWithValue("sessionId", snapshot.WorldSessionId);
                refreshedExpiry = ReadTimestamp(await refresh.ExecuteScalarAsync(cancellationToken));
            }
            snapshot = snapshot with
            {
                ServerTick = ticks.TickCount,
                Velocity = new WorldPosition(0, 0, 0),
                ExpiresAt = refreshedExpiry
            };
        }

        if (!resumed)
        {
            var spawn = map.GetSpawn(CoastSpawnId);
            var sessionId = Guid.NewGuid();
            DateTimeOffset expiresAt;
            await using (var insert = new NpgsqlCommand(
                "INSERT INTO world_sessions (world_session_id, account_id, character_id, map_id, spawn_id, position_x, position_y, position_z, last_input_tick, expires_at) VALUES (@sessionId, @accountId, @characterId, @mapId, @spawnId, @x, @y, @z, @tick, clock_timestamp() + @grace) RETURNING expires_at",
                connection, transaction))
            {
                insert.Parameters.AddWithValue("sessionId", sessionId);
                insert.Parameters.AddWithValue("accountId", accountId);
                insert.Parameters.AddWithValue("characterId", characterId);
                insert.Parameters.AddWithValue("mapId", map.Id);
                insert.Parameters.AddWithValue("spawnId", spawn.Id);
                insert.Parameters.AddWithValue("x", spawn.Position.X);
                insert.Parameters.AddWithValue("y", spawn.Position.Y);
                insert.Parameters.AddWithValue("z", spawn.Position.Z);
                insert.Parameters.AddWithValue("tick", ticks.TickCount);
                insert.Parameters.AddWithValue("grace", SessionGracePeriod);
                expiresAt = ReadTimestamp(await insert.ExecuteScalarAsync(cancellationToken));
            }
            snapshot = new WorldSessionSnapshot(sessionId, characterId, characterName, classId,
                map.Id, spawn.Id, spawn.Position, new WorldPosition(0, 0, 0), map.WalkSpeed,
                ticks.TickCount, 0, expiresAt, false);
        }

        await transaction.CommitAsync(cancellationToken);
        return new EnterWorldResult(snapshot, resumed);
    }

    /// <summary>Rebases the tick clock and inactivity lease after reconnect, including after a server restart.</summary>
    public async Task<WorldSessionSnapshot?> AttachAsync(Guid accountId, Guid worldSessionId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            UPDATE world_sessions ws
            SET velocity_x = 0, velocity_y = 0, velocity_z = 0,
                last_input_tick = @tick, last_active_at = clock_timestamp(), expires_at = clock_timestamp() + @grace
            FROM characters c
            WHERE c.character_id = ws.character_id AND c.account_id = ws.account_id
              AND ws.world_session_id = @sessionId AND ws.account_id = @accountId
              AND ws.closed_at IS NULL AND ws.expires_at > now()
            RETURNING ws.character_id, c.name, c.class_id, ws.map_id, ws.spawn_id,
                      ws.position_x, ws.position_y, ws.position_z,
                      ws.last_input_sequence, ws.expires_at
            """, connection);
        command.Parameters.AddWithValue("sessionId", worldSessionId);
        command.Parameters.AddWithValue("accountId", accountId);
        command.Parameters.AddWithValue("tick", ticks.TickCount);
        command.Parameters.AddWithValue("grace", SessionGracePeriod);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        var mapId = reader.GetString(3);
        if (!maps.TryGetAvailableMap(mapId, out var map) || map is null ||
            !string.Equals(reader.GetString(4), CoastSpawnId, StringComparison.Ordinal))
            return null;
        return new WorldSessionSnapshot(worldSessionId, reader.GetGuid(0), reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2), mapId, reader.GetString(4),
            new WorldPosition(reader.GetDouble(5), reader.GetDouble(6), reader.GetDouble(7)),
            new WorldPosition(0, 0, 0), map.WalkSpeed, ticks.TickCount,
            reader.GetInt64(8), ReadTimestamp(reader, 9), true);
    }

    public async Task<WorldMoveResult> MoveAsync(Guid accountId, Guid worldSessionId,
        MoveIntentPayload intent, CancellationToken cancellationToken)
    {
        if (!float.IsFinite(intent.DirectionX) || !float.IsFinite(intent.DirectionZ) ||
            Math.Abs(intent.DirectionX) > 1.001f || Math.Abs(intent.DirectionZ) > 1.001f ||
            intent.InputSequence <= 0)
            return new WorldMoveResult(false, "invalid_move_intent", null);

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var read = new NpgsqlCommand("""
            SELECT ws.character_id, c.name, c.class_id, ws.map_id, ws.spawn_id,
                   ws.position_x, ws.position_y, ws.position_z,
                   ws.last_input_sequence, ws.last_input_tick
            FROM world_sessions ws
            JOIN characters c ON c.character_id = ws.character_id AND c.account_id = ws.account_id
            WHERE ws.world_session_id = @sessionId AND ws.account_id = @accountId
              AND ws.closed_at IS NULL AND ws.expires_at > now()
            FOR UPDATE OF ws
            """, connection, transaction);
        read.Parameters.AddWithValue("sessionId", worldSessionId);
        read.Parameters.AddWithValue("accountId", accountId);
        Guid characterId;
        string characterName;
        string? classId;
        string mapId;
        string spawnId;
        WorldPosition current;
        long lastSequence;
        long lastTick;
        await using (var reader = await read.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
                return new WorldMoveResult(false, "world_session_expired", null);
            characterId = reader.GetGuid(0);
            characterName = reader.GetString(1);
            classId = reader.IsDBNull(2) ? null : reader.GetString(2);
            mapId = reader.GetString(3);
            spawnId = reader.GetString(4);
            current = new WorldPosition(reader.GetDouble(5), reader.GetDouble(6), reader.GetDouble(7));
            lastSequence = reader.GetInt64(8);
            lastTick = reader.GetInt64(9);
        }

        if (intent.InputSequence <= lastSequence)
            return new WorldMoveResult(false, "input_sequence_must_increase", null);
        var currentTick = ticks.TickCount;
        if (currentTick <= lastTick)
            return new WorldMoveResult(false, "movement_rate_limited", null);
        if (!maps.TryGetAvailableMap(mapId, out var map) || map is null || spawnId != CoastSpawnId)
            return new WorldMoveResult(false, "world_unavailable", null);

        var elapsedTicks = (int)Math.Clamp(currentTick - lastTick, 1, 2);
        map.TryMove(current, intent.DirectionX, intent.DirectionZ, elapsedTicks, out var position, out var velocity);
        DateTimeOffset expiresAt;
        await using (var update = new NpgsqlCommand("""
            UPDATE world_sessions
            SET position_x = @x, position_y = @y, position_z = @z,
                velocity_x = @vx, velocity_y = @vy, velocity_z = @vz,
                last_input_sequence = @sequence, last_input_tick = @tick,
                last_active_at = clock_timestamp(), expires_at = clock_timestamp() + @grace
            WHERE world_session_id = @sessionId
            RETURNING expires_at
            """, connection, transaction))
        {
            update.Parameters.AddWithValue("x", position.X);
            update.Parameters.AddWithValue("y", position.Y);
            update.Parameters.AddWithValue("z", position.Z);
            update.Parameters.AddWithValue("vx", velocity.X);
            update.Parameters.AddWithValue("vy", velocity.Y);
            update.Parameters.AddWithValue("vz", velocity.Z);
            update.Parameters.AddWithValue("sequence", intent.InputSequence);
            update.Parameters.AddWithValue("tick", currentTick);
            update.Parameters.AddWithValue("sessionId", worldSessionId);
            update.Parameters.AddWithValue("grace", SessionGracePeriod);
            expiresAt = ReadTimestamp(await update.ExecuteScalarAsync(cancellationToken));
        }
        await transaction.CommitAsync(cancellationToken);

        var snapshot = new WorldSessionSnapshot(worldSessionId, characterId, characterName, classId,
            mapId, spawnId, position, velocity, map.WalkSpeed, currentTick,
            intent.InputSequence, expiresAt, true);
        return new WorldMoveResult(true, null, snapshot);
    }

    public async Task LeaveAsync(Guid accountId, Guid worldSessionId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand("""
            UPDATE world_sessions
            SET closed_at = now(), velocity_x = 0, velocity_y = 0, velocity_z = 0
            WHERE world_session_id = @sessionId AND account_id = @accountId AND closed_at IS NULL
            """, connection);
        command.Parameters.AddWithValue("sessionId", worldSessionId);
        command.Parameters.AddWithValue("accountId", accountId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DateTimeOffset ReadTimestamp(NpgsqlDataReader reader, int ordinal) =>
        new(DateTime.SpecifyKind(reader.GetDateTime(ordinal), DateTimeKind.Utc));

    private static DateTimeOffset ReadTimestamp(object? value)
    {
        if (value is not DateTime timestamp)
            throw new InvalidOperationException("Database did not return a session expiry timestamp.");
        return new DateTimeOffset(DateTime.SpecifyKind(timestamp, DateTimeKind.Utc));
    }
}
