using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using NpgsqlTypes;
using ProjetoRpg.Server.Configuration;

namespace ProjetoRpg.Server.Accounts;

public sealed record AccountCredential(Guid AccountId);
public sealed record CreatedSession(string AccessToken, DateTimeOffset ExpiresAt);
public sealed record RegistrationResult(bool Created, Guid? AccountId);

public static partial class AccountInput
{
    [GeneratedRegex("^[A-Za-z0-9_.-]{3,32}$", RegexOptions.CultureInvariant)]
    private static partial Regex UsernamePattern();

    public static bool TryNormalizeUsername(string? input, out string username, out string normalized)
    {
        username = string.Empty;
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input) || input.Length > 64) return false;
        try { username = input.Normalize(NormalizationForm.FormKC); }
        catch (ArgumentException) { return false; }
        if (!UsernamePattern().IsMatch(username)) return false;
        normalized = username.ToUpperInvariant();
        return true;
    }

    public static bool IsValidPassword(string? password) =>
        password is { Length: >= 12 and <= 128 } && Encoding.UTF8.GetByteCount(password) <= 512;
}

public sealed class AuthService
{
    private static readonly AccountCredential DummyCredential = new(Guid.Empty);
    private readonly NpgsqlDataSource _dataSource;
    private readonly IPasswordHasher<AccountCredential> _passwordHasher;
    private readonly TimeSpan _sessionLifetime;
    private readonly ILogger<AuthService> _logger;
    private readonly string _dummyPasswordHash;

    public AuthService(
        NpgsqlDataSource dataSource,
        IPasswordHasher<AccountCredential> passwordHasher,
        ServerConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _dataSource = dataSource;
        _passwordHasher = passwordHasher;
        _sessionLifetime = configuration.SessionLifetime;
        _logger = logger;
        _dummyPasswordHash = _passwordHasher.HashPassword(DummyCredential, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    }

    public async Task<RegistrationResult> RegisterAsync(string username, string normalizedUsername, string password, CancellationToken cancellationToken)
    {
        var accountId = Guid.NewGuid();
        var passwordHash = _passwordHasher.HashPassword(new AccountCredential(accountId), password);
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using (var insert = new NpgsqlCommand(
                "INSERT INTO accounts (account_id, username, username_normalized, password_hash) VALUES (@id, @username, @normalized, @hash)",
                connection, transaction))
            {
                insert.Parameters.AddWithValue("id", accountId);
                insert.Parameters.AddWithValue("username", username);
                insert.Parameters.AddWithValue("normalized", normalizedUsername);
                insert.Parameters.AddWithValue("hash", passwordHash);
                await insert.ExecuteNonQueryAsync(cancellationToken);
            }
            await WriteAuditAsync(connection, transaction, "account_registered", accountId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Account registered {AccountId}", accountId);
            return new RegistrationResult(true, accountId);
        }
        catch (PostgresException exception) when (
            exception.SqlState == PostgresErrorCodes.UniqueViolation &&
            exception.ConstraintName == "uq_accounts_username_normalized")
        {
            return new RegistrationResult(false, null);
        }
    }

    public async Task<CreatedSession?> LoginAsync(string? username, string? password, CancellationToken cancellationToken)
    {
        var validUsername = AccountInput.TryNormalizeUsername(username, out _, out var normalizedUsername);
        var validPassword = AccountInput.IsValidPassword(password);
        var candidatePassword = validPassword ? password! : "invalid-password-input";
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        Guid? accountId = null;
        string? storedHash = null;
        if (validUsername)
        {
            await using var find = new NpgsqlCommand(
                "SELECT account_id, password_hash FROM accounts WHERE username_normalized = @normalized", connection);
            find.Parameters.AddWithValue("normalized", normalizedUsername);
            await using var reader = await find.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                accountId = reader.GetGuid(0);
                storedHash = reader.GetString(1);
            }
        }

        var credential = accountId is { } id ? new AccountCredential(id) : DummyCredential;
        var hash = storedHash ?? _dummyPasswordHash;
        PasswordVerificationResult verification;
        try { verification = _passwordHasher.VerifyHashedPassword(credential, hash, candidatePassword); }
        catch (FormatException) { verification = PasswordVerificationResult.Failed; }

        if (!validUsername || !validPassword || accountId is null || verification == PasswordVerificationResult.Failed)
        {
            await WriteAuditAsync(connection, null, "login_failed", accountId, cancellationToken);
            _logger.LogWarning("Authentication rejected");
            return null;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var tokenHash = HashToken(token);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var refreshedHash = _passwordHasher.HashPassword(new AccountCredential(accountId.Value), candidatePassword);
            await using var updateHash = new NpgsqlCommand(
                "UPDATE accounts SET password_hash = @hash WHERE account_id = @accountId", connection, transaction);
            updateHash.Parameters.AddWithValue("hash", refreshedHash);
            updateHash.Parameters.AddWithValue("accountId", accountId.Value);
            await updateHash.ExecuteNonQueryAsync(cancellationToken);
        }
        DateTimeOffset expiresAt;
        await using (var createSession = new NpgsqlCommand(
            "INSERT INTO account_sessions (token_hash, account_id, expires_at) VALUES (@tokenHash, @accountId, now() + @lifetime) RETURNING expires_at",
            connection, transaction))
        {
            createSession.Parameters.AddWithValue("tokenHash", tokenHash);
            createSession.Parameters.AddWithValue("accountId", accountId.Value);
            createSession.Parameters.AddWithValue("lifetime", _sessionLifetime);
            var expiryValue = await createSession.ExecuteScalarAsync(cancellationToken)
                ?? throw new InvalidOperationException("Database did not return the new session expiry.");
            var expiryUtc = DateTime.SpecifyKind((DateTime)expiryValue, DateTimeKind.Utc);
            expiresAt = new DateTimeOffset(expiryUtc);
        }
        await WriteAuditAsync(connection, transaction, "login_succeeded", accountId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Account authenticated {AccountId}", accountId.Value);
        return new CreatedSession(token, expiresAt);
    }

    public async Task<Guid?> GetAuthenticatedAccountAsync(string? token, CancellationToken cancellationToken)
    {
        if (!TryHashToken(token, out var tokenHash)) return null;
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(
            "SELECT account_id FROM account_sessions WHERE token_hash = @tokenHash AND revoked_at IS NULL AND expires_at > now()",
            connection);
        command.Parameters.AddWithValue("tokenHash", tokenHash);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is Guid accountId ? accountId : null;
    }

    public async Task RevokeCurrentSessionAsync(string? token, CancellationToken cancellationToken)
    {
        if (!TryHashToken(token, out var tokenHash)) return;
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        Guid? accountId;
        await using (var revoke = new NpgsqlCommand(
            "UPDATE account_sessions SET revoked_at = now() WHERE token_hash = @tokenHash AND revoked_at IS NULL AND expires_at > now() RETURNING account_id",
            connection, transaction))
        {
            revoke.Parameters.AddWithValue("tokenHash", tokenHash);
            var result = await revoke.ExecuteScalarAsync(cancellationToken);
            accountId = result is Guid id ? id : null;
        }
        if (accountId is { } owner)
            await WriteAuditAsync(connection, transaction, "session_revoked", owner, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public static string? GetBearerToken(HttpRequest request)
    {
        var header = request.Headers["Authorization"].ToString();
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        return header[prefix.Length..].Trim();
    }

    private static bool TryHashToken(string? token, out byte[] hash)
    {
        hash = [];
        if (token is null || token.Length != 43 || token.Any(character =>
                !(character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_')))
            return false;
        hash = SHA256.HashData(Encoding.ASCII.GetBytes(token));
        return true;
    }

    private static byte[] HashToken(string token) => SHA256.HashData(Encoding.ASCII.GetBytes(token));

    private static async Task WriteAuditAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        string eventType,
        Guid? accountId,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(
            "INSERT INTO security_audit_events (event_type, actor_account_id) VALUES (@eventType, @accountId)",
            connection, transaction);
        command.Parameters.AddWithValue("eventType", eventType);
        command.Parameters.Add(new NpgsqlParameter("accountId", NpgsqlDbType.Uuid) { Value = (object?)accountId ?? DBNull.Value });
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
