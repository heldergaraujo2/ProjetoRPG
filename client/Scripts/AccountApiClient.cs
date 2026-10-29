using System.Text;
using Godot;

namespace ProjetoRpg.Client.Scripts;

public sealed record ApiResponse(long Result, long StatusCode, string Body);

/// <summary>Small HTTP transport for the real account/session/character API.</summary>
public partial class AccountApiClient : Node
{
    private readonly HTTPRequest _request = new();
    private TaskCompletionSource<ApiResponse>? _pendingRequest;

    public string BaseUrl { get; }
    public string? ConfigurationError { get; }

    public AccountApiClient()
    {
        var configuredUrl = Environment.GetEnvironmentVariable("PROJETORPG_SERVER_URL");
        var candidate = string.IsNullOrWhiteSpace(configuredUrl)
            ? "http://127.0.0.1:5050"
            : configuredUrl.Trim();

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback) &&
            string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment))
        {
            BaseUrl = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }
        else
        {
            BaseUrl = string.Empty;
            ConfigurationError = "Use HTTPS para servidor remoto; HTTP só é permitido em loopback local. Não inclua usuário ou senha na URL.";
        }
    }

    public override void _Ready()
    {
        _request.Timeout = 15.0f;
        _request.MaxRedirects = 0;
        AddChild(_request);
        _request.RequestCompleted += OnRequestCompleted;
    }

    public async Task<ApiResponse> SendAsync(
        HTTPClient.Method method,
        string path,
        string? jsonBody = null,
        string? bearerToken = null)
    {
        if (ConfigurationError is not null)
            throw new InvalidOperationException(ConfigurationError);
        if (_pendingRequest is not null)
            throw new InvalidOperationException("Uma solicitação HTTP já está em andamento.");

        var headers = new List<string> { "Accept: application/json" };
        if (jsonBody is not null)
            headers.Add("Content-Type: application/json; charset=utf-8");
        if (!string.IsNullOrEmpty(bearerToken))
            headers.Add($"Authorization: Bearer {bearerToken}");

        var completion = new TaskCompletionSource<ApiResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequest = completion;
        var requestUrl = $"{BaseUrl}{path}";
        var error = _request.Request(requestUrl, headers.ToArray(), method, jsonBody ?? string.Empty);
        if (error != Error.Ok)
        {
            _pendingRequest = null;
            throw new InvalidOperationException($"Não foi possível iniciar a solicitação HTTP ({error}).");
        }

        return await completion.Task;
    }

    public override void _ExitTree()
    {
        _request.RequestCompleted -= OnRequestCompleted;
        _request.CancelRequest();
        _pendingRequest?.TrySetCanceled();
        _pendingRequest = null;
    }

    private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        var completion = _pendingRequest;
        _pendingRequest = null;
        completion?.TrySetResult(new ApiResponse(result, responseCode, Encoding.UTF8.GetString(body)));
    }
}
