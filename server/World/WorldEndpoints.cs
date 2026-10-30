using System.Collections.Concurrent;
using ProjetoRpg.Server.Accounts;
using ProjetoRpg.Shared.World;

namespace ProjetoRpg.Server.World;

public sealed record WorldApiError(string Error);

public sealed class WorldConnectionRegistry
{
    private readonly ConcurrentDictionary<Guid, Guid> _connections = new();

    public bool TryAttach(Guid worldSessionId, Guid connectionId) => _connections.TryAdd(worldSessionId, connectionId);

    public void Detach(Guid worldSessionId, Guid connectionId)
    {
        ((ICollection<KeyValuePair<Guid, Guid>>)_connections).Remove(new(worldSessionId, connectionId));
    }
}

public static class WorldEndpoints
{
    public static void MapWorldEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/world/sessions", async (
            EnterWorldRequest request,
            HttpContext context,
            AuthService auth,
            WorldSessionService sessions,
            CancellationToken cancellationToken) =>
        {
            var accountId = await auth.GetAuthenticatedAccountAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            if (accountId is null) return (IResult)Results.Json(new WorldApiError("authentication_required"), statusCode: StatusCodes.Status401Unauthorized);
            if (request.CharacterId == Guid.Empty)
                return Results.Json(new WorldApiError("invalid_character"), statusCode: StatusCodes.Status400BadRequest);

            EnterWorldResult? result;
            try
            {
                result = await sessions.EnterAsync(accountId.Value, request.CharacterId, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Results.Json(new WorldApiError("world_unavailable"), statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            if (result is null)
                return Results.NotFound(new WorldApiError("character_not_found"));
            return result.Resumed
                ? Results.Ok(result.Snapshot)
                : Results.Json(result.Snapshot, statusCode: StatusCodes.Status201Created);
        }).RequireRateLimiting("auth");

        app.MapDelete("/api/v1/world/sessions/{worldSessionId:guid}", async (
            Guid worldSessionId,
            HttpContext context,
            AuthService auth,
            WorldSessionService sessions,
            CancellationToken cancellationToken) =>
        {
            var accountId = await auth.GetAuthenticatedAccountAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            if (accountId is null) return (IResult)Results.Json(new WorldApiError("authentication_required"), statusCode: StatusCodes.Status401Unauthorized);
            await sessions.LeaveAsync(accountId.Value, worldSessionId, cancellationToken);
            return Results.NoContent();
        });
    }
}
