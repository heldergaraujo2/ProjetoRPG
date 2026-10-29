using System.Net;
using System.Text.Json;
using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>
/// Account and persistent-character flow backed by the dedicated server API.
/// The bearer token stays in memory and is never rendered or logged.
/// </summary>
public partial class AccountGateway : Control
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private AccountApiClient _api = null!;
    private LineEdit _username = null!;
    private LineEdit _password = null!;
    private LineEdit _characterName = null!;
    private VBoxContainer _characterRows = null!;
    private VBoxContainer _authenticationPanel = null!;
    private VBoxContainer _charactersPanel = null!;
    private Label _status = null!;
    private Label _selectedCharacter = null!;
    private Button _loginButton = null!;
    private Button _registerButton = null!;
    private Button _createCharacterButton = null!;
    private Button _refreshCharactersButton = null!;
    private Button _logoutButton = null!;
    private string? _accessToken;
    private bool _busy;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        _api = new AccountApiClient();
        AddChild(_api);
        BuildInterface();

        if (_api.ConfigurationError is not null)
        {
            SetStatus(_api.ConfigurationError, true);
        }
        else
        {
            SetStatus($"Servidor configurado: {_api.BaseUrl}. Nenhuma credencial é salva neste dispositivo.");
        }
    }

    private void BuildInterface()
    {
        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(560, 540),
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -280,
            OffsetTop = -270,
            OffsetRight = 280,
            OffsetBottom = 270
        };
        AddChild(panel);

        var layout = new VBoxContainer { CustomMinimumSize = new Vector2(520, 500) };
        panel.AddChild(layout);
        AddLabel(layout, "ProjetoRPG", 28);
        AddLabel(layout, "Conta e personagens persistidos no servidor", 16);

        _authenticationPanel = new VBoxContainer();
        layout.AddChild(_authenticationPanel);
        _username = AddLineEdit(_authenticationPanel, "Usuário (3–32 caracteres)");
        _password = AddLineEdit(_authenticationPanel, "Senha (12–128 caracteres)");
        _password.Secret = true;
        var authButtons = new HBoxContainer();
        _authenticationPanel.AddChild(authButtons);
        _registerButton = AddButton(authButtons, "Criar conta", OnRegisterPressed);
        _loginButton = AddButton(authButtons, "Entrar", OnLoginPressed);

        _charactersPanel = new VBoxContainer { Visible = false };
        layout.AddChild(_charactersPanel);
        AddLabel(_charactersPanel, "Personagens desta conta", 18);
        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(0, 130), HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        _charactersPanel.AddChild(scroll);
        _characterRows = new VBoxContainer();
        scroll.AddChild(_characterRows);

        _selectedCharacter = AddLabel(_charactersPanel, "Selecione um personagem para ver o estado persistido.");
        _characterName = AddLineEdit(_charactersPanel, "Novo nome (letra inicial, 3–20 caracteres)");
        _createCharacterButton = AddButton(_charactersPanel, "Criar personagem", OnCreateCharacterPressed);
        _refreshCharactersButton = AddButton(_charactersPanel, "Atualizar lista", OnRefreshCharactersPressed);
        AddLabel(_charactersPanel,
            "O servidor ainda não oferece entrada em mundo ou movimento. A demo 3D permanece local e isolada; este fluxo não simula multiplayer.");
        _logoutButton = AddButton(_charactersPanel, "Encerrar sessão", OnLogoutPressed);

        _status = AddLabel(layout, string.Empty);
        _status.CustomMinimumSize = new Vector2(0, 50);
        _status.AutowrapMode = TextServer.AutowrapMode.WordSmart;
    }

    private async void OnRegisterPressed()
    {
        await RunBusyAsync(async () =>
        {
            var body = JsonSerializer.Serialize(new { username = _username.Text.Trim(), password = _password.Text }, _jsonOptions);
            var response = await _api.SendAsync(Godot.HttpClient.Method.Post, "/api/v1/accounts/register", body);
            if (response.Result != (long)HttpRequest.Result.Success)
            {
                SetStatus("Não foi possível alcançar o servidor. Confirme o serviço e tente novamente.", true);
                return;
            }

            if (response.StatusCode == (long)HttpStatusCode.Accepted)
            {
                SetStatus("Solicitação de cadastro aceita. Por segurança, a API não informa se o usuário já existia; tente entrar.");
                _password.Clear();
                return;
            }

            SetStatus(DescribeApiError(response), true);
        });
    }

    private async void OnLoginPressed()
    {
        await RunBusyAsync(async () =>
        {
            var body = JsonSerializer.Serialize(new { username = _username.Text.Trim(), password = _password.Text }, _jsonOptions);
            var response = await _api.SendAsync(Godot.HttpClient.Method.Post, "/api/v1/sessions", body);
            if (response.Result != (long)HttpRequest.Result.Success)
            {
                SetStatus("Não foi possível alcançar o servidor. Confirme o serviço e tente novamente.", true);
                return;
            }

            if (response.StatusCode != (long)HttpStatusCode.Created || !TryReadSession(response.Body, out var token))
            {
                SetStatus(DescribeApiError(response), true);
                return;
            }

            _accessToken = token;
            _password.Clear();
            _authenticationPanel.Visible = false;
            _charactersPanel.Visible = true;
            await RefreshCharactersAsync();
        });
    }

    private async void OnRefreshCharactersPressed()
    {
        await RunBusyAsync(RefreshCharactersAsync);
    }

    private async void OnCreateCharacterPressed()
    {
        if (_accessToken is null)
        {
            SetStatus("Entre novamente para criar um personagem.", true);
            return;
        }

        await RunBusyAsync(async () =>
        {
            var body = JsonSerializer.Serialize(new { name = _characterName.Text.Trim(), classId = (string?)null }, _jsonOptions);
            var response = await _api.SendAsync(Godot.HttpClient.Method.Post, "/api/v1/characters", body, _accessToken);
            if (response.Result != (long)HttpRequest.Result.Success)
            {
                SetStatus("Não foi possível alcançar o servidor. Confirme o serviço e tente novamente.", true);
                return;
            }

            if (response.StatusCode != (long)HttpStatusCode.Created)
            {
                SetStatus(DescribeApiError(response), true);
                return;
            }

            _characterName.Clear();
            SetStatus("Personagem criado e persistido pelo servidor.");
            await RefreshCharactersAsync();
        });
    }

    private async void OnLogoutPressed()
    {
        var token = _accessToken;
        await RunBusyAsync(async () =>
        {
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    await _api.SendAsync(Godot.HttpClient.Method.Delete, "/api/v1/sessions/current", bearerToken: token);
                }
                catch (InvalidOperationException)
                {
                    // The local session is still cleared when the server is unreachable.
                }
            }

            _accessToken = null;
            _authenticationPanel.Visible = true;
            _charactersPanel.Visible = false;
            _characterRows.GetChildren().ToList().ForEach(child => child.QueueFree());
            _selectedCharacter.Text = "Selecione um personagem para ver o estado persistido.";
            _password.Clear();
            SetStatus("Sessão removida deste cliente. Se o servidor estava indisponível, a sessão remota expirará ou poderá ser revogada depois.");
        });
    }

    private async Task RefreshCharactersAsync()
    {
        if (_accessToken is null) return;

        var response = await _api.SendAsync(Godot.HttpClient.Method.Get, "/api/v1/characters", bearerToken: _accessToken);
        if (response.Result != (long)HttpRequest.Result.Success)
        {
            SetStatus("Falha de conexão ao carregar personagens. Tente novamente após confirmar o servidor.", true);
            return;
        }

        if (response.StatusCode == (long)HttpStatusCode.Unauthorized)
        {
            _accessToken = null;
            _authenticationPanel.Visible = true;
            _charactersPanel.Visible = false;
            SetStatus("A sessão expirou ou foi revogada. Entre novamente.", true);
            return;
        }

        if (response.StatusCode != (long)HttpStatusCode.OK)
        {
            SetStatus(DescribeApiError(response), true);
            return;
        }

        List<CharacterSummary> characters;
        try
        {
            characters = JsonSerializer.Deserialize<List<CharacterSummary>>(response.Body, _jsonOptions)
                ?? throw new JsonException("A lista de personagens veio vazia ou inválida.");
        }
        catch (JsonException)
        {
            SetStatus("Resposta inválida ao carregar personagens. A sessão continua em memória.", true);
            return;
        }

        _characterRows.GetChildren().ToList().ForEach(child => child.QueueFree());
        _selectedCharacter.Text = characters.Count == 0
            ? "Ainda não há personagens nesta conta. Crie um abaixo."
            : "Selecione um personagem para ver o estado persistido.";

        foreach (var character in characters)
        {
            var summary = string.IsNullOrWhiteSpace(character.ClassId) ? "sem classe" : character.ClassId;
            var button = new Button { Text = $"{character.Name} — {summary}", CustomMinimumSize = new Vector2(0, 36) };
            button.Pressed += () => SelectCharacter(character);
            _characterRows.AddChild(button);
        }

        SetStatus($"Lista carregada do servidor: {characters.Count} personagem(ns). A seleção não entra em um mundo ainda.");
    }

    private void SelectCharacter(CharacterSummary character)
    {
        if (!Guid.TryParse(character.CharacterId, out var characterId))
        {
            SetStatus("O servidor retornou um ID de personagem inválido.", true);
            return;
        }

        var className = string.IsNullOrWhiteSpace(character.ClassId) ? "sem classe atribuída" : $"classe {character.ClassId}";
        _selectedCharacter.Text = $"Selecionado: {character.Name} ({className}) · ID {characterId}";
        SetStatus("Seleção atual corresponde a um personagem persistido; nenhum estado de mundo foi criado.");
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        SetButtonsDisabled(true);
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is InvalidOperationException or TaskCanceledException or TimeoutException)
        {
            SetStatus("Solicitação interrompida. Verifique se o servidor está acessível e tente novamente.", true);
        }
        finally
        {
            _busy = false;
            SetButtonsDisabled(false);
        }
    }

    private void SetButtonsDisabled(bool disabled)
    {
        if (GodotObject.IsInstanceValid(_loginButton)) _loginButton.Disabled = disabled;
        if (GodotObject.IsInstanceValid(_registerButton)) _registerButton.Disabled = disabled;
        if (GodotObject.IsInstanceValid(_createCharacterButton)) _createCharacterButton.Disabled = disabled;
        if (GodotObject.IsInstanceValid(_refreshCharactersButton)) _refreshCharactersButton.Disabled = disabled;
        if (GodotObject.IsInstanceValid(_logoutButton)) _logoutButton.Disabled = disabled;
    }

    private bool TryReadSession(string body, out string token)
    {
        token = string.Empty;
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("accessToken", out var accessToken) || accessToken.ValueKind != JsonValueKind.String ||
                !root.TryGetProperty("tokenType", out var tokenType) || tokenType.ValueKind != JsonValueKind.String ||
                !root.TryGetProperty("expiresAt", out var expiresAt) || expiresAt.ValueKind != JsonValueKind.String ||
                !string.Equals(tokenType.GetString(), "Bearer", StringComparison.Ordinal) ||
                !DateTimeOffset.TryParse(expiresAt.GetString(), out var expiry) || expiry <= DateTimeOffset.UtcNow)
                return false;

            var candidate = accessToken.GetString();
            if (candidate is null || candidate.Length != 43 || candidate.Any(character =>
                    !(character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_')))
                return false;

            token = candidate;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string DescribeApiError(ApiResponse response)
    {
        if (response.Result != (long)HttpRequest.Result.Success)
            return "Falha de rede ao falar com o servidor.";

        string? code = null;
        try
        {
            using var document = JsonDocument.Parse(response.Body);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                code = error.GetString();
        }
        catch (JsonException)
        {
            // Status HTTP remains safe to report without echoing the response body.
        }

        return code switch
        {
            "invalid_registration" => "Cadastro inválido: usuário deve ter 3–32 caracteres permitidos e senha 12–128 caracteres.",
            "invalid_credentials" => "Usuário ou senha inválidos.",
            "authentication_required" => "Sessão inválida. Entre novamente.",
            "invalid_character" => "Nome de personagem inválido. Use letra inicial e 3–20 letras, números ou sublinhados.",
            "name_unavailable" => "Esse nome de personagem já está em uso.",
            "class_unavailable" => "A classe solicitada ainda não está disponível.",
            _ when response.StatusCode == (long)HttpStatusCode.TooManyRequests => "Muitas tentativas. Aguarde um pouco antes de tentar novamente.",
            _ when response.StatusCode >= 500 => "O servidor está temporariamente indisponível. Tente novamente mais tarde.",
            _ => $"A solicitação não foi concluída (HTTP {response.StatusCode})."
        };
    }

    private void SetStatus(string message, bool isError = false)
    {
        if (!GodotObject.IsInstanceValid(_status)) return;
        _status.Text = message;
        _status.Modulate = isError ? new Color(1.0f, 0.55f, 0.45f) : new Color(0.85f, 0.9f, 1.0f);
    }

    private static Label AddLabel(Container parent, string text, int fontSize = 14)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        if (fontSize != 14) label.AddThemeFontSizeOverride("font_size", fontSize);
        parent.AddChild(label);
        return label;
    }

    private static LineEdit AddLineEdit(Container parent, string placeholder)
    {
        var lineEdit = new LineEdit { PlaceholderText = placeholder, CustomMinimumSize = new Vector2(0, 38) };
        parent.AddChild(lineEdit);
        return lineEdit;
    }

    private static Button AddButton(Container parent, string text, Action pressed)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 38) };
        button.Pressed += pressed;
        parent.AddChild(button);
        return button;
    }

    private sealed record CharacterSummary(string CharacterId, string Name, string? ClassId);
}
