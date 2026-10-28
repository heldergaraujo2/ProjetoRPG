using Microsoft.AspNetCore.RateLimiting;
using ProjetoRpg.Server.Characters;

namespace ProjetoRpg.Server.Accounts;

public sealed record RegisterAccountRequest(string? Username, string? Password);
public sealed record LoginRequest(string? Username, string? Password);
public sealed record CreateCharacterRequest(string? Name, string? ClassId);
public sealed record RenameCharacterRequest(string? Name);
public sealed record ApiError(string Error);
public sealed record RegistrationAccepted(string Status);

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/accounts/register", async (
            RegisterAccountRequest request, AuthService auth, CancellationToken cancellationToken) =>
        {
            if (!AccountInput.TryNormalizeUsername(request.Username, out var username, out var normalized) ||
                !AccountInput.IsValidPassword(request.Password))
                return (IResult)Results.Json(new ApiError("invalid_registration"), statusCode: StatusCodes.Status400BadRequest);

            // Same response for new and already-registered names avoids account enumeration.
            await auth.RegisterAsync(username, normalized, request.Password!, cancellationToken);
            return (IResult)Results.Json(new RegistrationAccepted("accepted"), statusCode: StatusCodes.Status202Accepted);
        }).RequireRateLimiting("auth");

        app.MapPost("/api/v1/sessions", async (
            LoginRequest request, AuthService auth, CancellationToken cancellationToken) =>
        {
            var session = await auth.LoginAsync(request.Username, request.Password, cancellationToken);
            if (session is null)
                return (IResult)Results.Json(new ApiError("invalid_credentials"), statusCode: StatusCodes.Status401Unauthorized);
            return (IResult)Results.Json(new
            {
                accessToken = session.AccessToken,
                tokenType = "Bearer",
                expiresAt = session.ExpiresAt
            }, statusCode: StatusCodes.Status201Created);
        }).RequireRateLimiting("auth");

        app.MapDelete("/api/v1/sessions/current", async (HttpContext context, AuthService auth, CancellationToken cancellationToken) =>
        {
            await auth.RevokeCurrentSessionAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            return Results.NoContent();
        });

        app.MapGet("/api/v1/characters", async (
            HttpContext context, AuthService auth, CharacterService characters, CancellationToken cancellationToken) =>
        {
            var accountId = await auth.GetAuthenticatedAccountAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            if (accountId is null) return Unauthorized();
            return (IResult)Results.Ok(await characters.ListForAccountAsync(accountId.Value, cancellationToken));
        });

        app.MapPost("/api/v1/characters", async (
            CreateCharacterRequest request, HttpContext context, AuthService auth, CharacterService characters,
            CancellationToken cancellationToken) =>
        {
            var accountId = await auth.GetAuthenticatedAccountAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            if (accountId is null) return Unauthorized();
            if (!CharacterInput.TryNormalizeName(request.Name, out var name, out var normalized) ||
                request.ClassId is not null && (string.IsNullOrWhiteSpace(request.ClassId) || request.ClassId.Length > 64))
                return Results.Json(new ApiError("invalid_character"), statusCode: StatusCodes.Status400BadRequest);

            var result = await characters.CreateAsync(accountId.Value, name, normalized, request.ClassId, cancellationToken);
            IResult response = result.Outcome switch
            {
                CharacterWriteOutcome.Created => Results.Created($"/api/v1/characters/{result.Character!.CharacterId}", result.Character),
                CharacterWriteOutcome.NameUnavailable => Results.Json(new ApiError("name_unavailable"), statusCode: StatusCodes.Status409Conflict),
                CharacterWriteOutcome.ClassUnavailable => Results.Json(new ApiError("class_unavailable"), statusCode: StatusCodes.Status422UnprocessableEntity),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
            return response;
        });

        app.MapGet("/api/v1/characters/{characterId:guid}", async (
            Guid characterId, HttpContext context, AuthService auth, CharacterService characters, CancellationToken cancellationToken) =>
        {
            var accountId = await auth.GetAuthenticatedAccountAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            if (accountId is null) return Unauthorized();
            var character = await characters.GetForAccountAsync(accountId.Value, characterId, cancellationToken);
            return character is null
                ? (IResult)Results.NotFound(new ApiError("character_not_found"))
                : Results.Ok(character);
        });

        app.MapPatch("/api/v1/characters/{characterId:guid}", async (
            Guid characterId, RenameCharacterRequest request, HttpContext context, AuthService auth,
            CharacterService characters, CancellationToken cancellationToken) =>
        {
            var accountId = await auth.GetAuthenticatedAccountAsync(AuthService.GetBearerToken(context.Request), cancellationToken);
            if (accountId is null) return Unauthorized();
            if (!CharacterInput.TryNormalizeName(request.Name, out var name, out var normalized))
                return Results.Json(new ApiError("invalid_character_name"), statusCode: StatusCodes.Status400BadRequest);

            var outcome = await characters.RenameForAccountAsync(accountId.Value, characterId, name, normalized, cancellationToken);
            IResult response = outcome switch
            {
                CharacterWriteOutcome.Updated => Results.NoContent(),
                CharacterWriteOutcome.NameUnavailable => Results.Json(new ApiError("name_unavailable"), statusCode: StatusCodes.Status409Conflict),
                CharacterWriteOutcome.NotFound => Results.NotFound(new ApiError("character_not_found")),
                _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
            };
            return response;
        });
    }

    private static IResult Unauthorized() =>
        Results.Json(new ApiError("authentication_required"), statusCode: StatusCodes.Status401Unauthorized);
}
