using System.Net;
using System.Text.Json;
using Godot;
using ProjetoRpg.Shared.World;

namespace ProjetoRpg.Client.Scripts;

/// <summary>
/// Owns the authenticated account session and routes the user into the character lobby.
/// The bearer token remains in memory and is never rendered, logged, or persisted.
/// </summary>
public partial class AccountGateway : Control
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private AccountApiClient _api = null!;
    private Control _authenticationPanel = null!;
    private LineEdit _username = null!;
    private LineEdit _password = null!;
    private Button _loginButton = null!;
    private Button _registerButton = null!;
    private Label _status = null!;
    private CharacterLobby? _lobby;
    private WorldGameplay? _world;
    private Guid? _activeWorldSessionId;
    private string? _accessToken;
    private string? _preferredCharacterId;
    private bool _busy;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        _api = new AccountApiClient();
        AddChild(_api);
        BuildAuthenticationInterface();

        if (_api.ConfigurationError is not null)
            SetStatus(_api.ConfigurationError, true);
        else
            SetStatus($"Servidor configurado: {_api.BaseUrl}. Nenhuma credencial é salva neste dispositivo.");
    }

    private void BuildAuthenticationInterface()
    {
        var backdrop = new ColorRect
        {
            Color = new Color(0.025f, 0.045f, 0.071f, 1.0f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        backdrop.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        _authenticationPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(520, 380),
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -260,
            OffsetTop = -190,
            OffsetRight = 260,
            OffsetBottom = 190
        };
        AddChild(_authenticationPanel);
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", 12);
        _authenticationPanel.AddChild(layout);
        AddLabel(layout, "PROJETORPG", 28);
        AddLabel(layout, "Acesse sua conta para abrir o salão de personagens.", 15);
        _username = AddLineEdit(layout, "Usuário (3–32 caracteres)");
        _password = AddLineEdit(layout, "Senha (12–128 caracteres)");
        _password.Secret = true;
        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 10);
        layout.AddChild(buttons);
        _registerButton = AddButton(buttons, "Criar conta", OnRegisterPressed);
        _loginButton = AddButton(buttons, "Entrar", OnLoginPressed);
        _status = AddLabel(layout, string.Empty, 14);
        _status.CustomMinimumSize = new Vector2(0, 64);
    }

    private async void OnRegisterPressed()
    {
        await RunBusyAsync(async () =>
        {
            var body = JsonSerializer.Serialize(new { username = _username.Text.Trim(), password = _password.Text }, _jsonOptions);
            var response = await _api.SendAsync(Godot.HttpClient.Method.Post, "/api/v1/accounts/register", body);
            if (!IsNetworkSuccess(response))
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
            if (!IsNetworkSuccess(response))
            {
                SetStatus("Não foi possível alcançar o servidor. Confirme o serviço e tente novamente.", true);
                return;
            }

            if (response.StatusCode != (long)HttpStatusCode.Created)
            {
                SetStatus(DescribeApiError(response), true);
                return;
            }
            if (!TryReadSession(response.Body, out var token))
            {
                SetStatus("O servidor retornou uma sessão inválida. Nenhuma credencial foi armazenada.", true);
                return;
            }

            _accessToken = token;
            _password.Clear();
            try
            {
                OpenLobby();
            }
            catch (Exception exception) when (exception is InvalidOperationException or InvalidCastException)
            {
                ClearLocalSession();
                SetStatus("Não foi possível carregar a cena do salão. A sessão local foi removida; tente novamente após corrigir o projeto.", true);
                return;
            }

            _authenticationPanel.Visible = false;
            await RefreshCharactersAsync();
        });
    }

    private void OpenLobby()
    {
        var packedScene = ResourceLoader.Load<PackedScene>("res://Scenes/CharacterLobby.tscn")
            ?? throw new InvalidOperationException("A cena CharacterLobby não está disponível.");
        var lobby = packedScene.Instantiate<CharacterLobby>()
            ?? throw new InvalidOperationException("A raiz da cena carregada não é CharacterLobby.");
        _lobby = lobby;
        lobby.SetAnchorsPreset(LayoutPreset.FullRect);
        lobby.RefreshRequested += OnRefreshRequested;
        lobby.CreateRequested += OnCreateRequested;
        lobby.LogoutRequested += OnLogoutRequested;
        lobby.EnterWorldRequested += OnEnterWorldRequested;
        AddChild(lobby);
        lobby.SetBusy(_busy);
    }

    private async void OnRefreshRequested() => await RunBusyAsync(RefreshCharactersAsync);

    private async void OnEnterWorldRequested(string characterId)
    {
        if (!Guid.TryParse(characterId, out var selectedCharacterId)) return;
        await RunBusyAsync(() => EnterWorldAsync(selectedCharacterId));
    }

    private async Task EnterWorldAsync(Guid characterId)
    {
        if (_accessToken is null || _lobby is null) return;
        var body = JsonSerializer.Serialize(new EnterWorldRequest(characterId), _jsonOptions);
        ApiResponse response;
        try
        {
            response = await _api.SendAsync(Godot.HttpClient.Method.Post,
                "/api/v1/world/sessions", body, _accessToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or TaskCanceledException or TimeoutException)
        {
            _lobby.SetStatus("Não foi possível solicitar entrada no mundo. Nenhuma entrada local foi iniciada.", true);
            return;
        }
        if (!IsNetworkSuccess(response))
        {
            _lobby.SetStatus("Falha de conexão ao solicitar entrada. O personagem continua no salão.", true);
            return;
        }
        if (response.StatusCode == (long)HttpStatusCode.Unauthorized)
        {
            ExpireSession();
            return;
        }
        if (response.StatusCode != (long)HttpStatusCode.Created && response.StatusCode != (long)HttpStatusCode.OK)
        {
            _lobby.SetStatus(DescribeApiError(response), true);
            return;
        }
        if (!TryReadWorldSnapshot(response.Body, characterId, out var snapshot))
        {
            _lobby.SetStatus("O servidor não retornou um estado inicial autorizado válido. A entrada foi bloqueada.", true);
            return;
        }

        try
        {
            var packedScene = ResourceLoader.Load<PackedScene>("res://Scenes/WorldGameplay.tscn")
                ?? throw new InvalidOperationException("A cena de gameplay própria não está disponível.");
            var world = packedScene.Instantiate<WorldGameplay>()
                ?? throw new InvalidOperationException("A raiz da cena de gameplay não é WorldGameplay.");
            world.Initialize(_api.BaseUrl, _accessToken, snapshot);
            world.ReturnRequested += OnWorldReturnRequested;
            _world = world;
            _activeWorldSessionId = snapshot.WorldSessionId;
            world.SetAnchorsPreset(LayoutPreset.FullRect);
            _lobby.Visible = false;
            AddChild(world);
            _lobby.SetStatus($"Entrada autorizada em {snapshot.MapId}; sessão recuperada={snapshot.Resumed}.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or InvalidCastException)
        {
            await ReleaseWorldSessionAsync(snapshot.WorldSessionId);
            if (_world is not null && GodotObject.IsInstanceValid(_world))
                _world.QueueFree();
            _world = null;
            _activeWorldSessionId = null;
            _lobby.Visible = true;
            _lobby.SetStatus($"Entrada autorizada, mas a cena Coast não pôde ser carregada: {exception.Message}. Retorno ao salão.", true);
        }
    }

    private async void OnWorldReturnRequested() => await RunBusyAsync(ReturnToLobbyAsync);

    private async Task ReturnToLobbyAsync()
    {
        var remoteClosed = false;
        if (_accessToken is not null && _activeWorldSessionId is { } worldSessionId)
        {
            try
            {
                var response = await _api.SendAsync(Godot.HttpClient.Method.Delete,
                    $"/api/v1/world/sessions/{worldSessionId}", bearerToken: _accessToken);
                remoteClosed = IsNetworkSuccess(response) && response.StatusCode == (long)HttpStatusCode.NoContent;
            }
            catch (Exception exception) when (exception is InvalidOperationException or TaskCanceledException or TimeoutException)
            {
                // The server expires inactive sessions after ten minutes; local return is never blocked.
            }
        }

        if (_world is not null && GodotObject.IsInstanceValid(_world))
        {
            _world.ReturnRequested -= OnWorldReturnRequested;
            _world.QueueFree();
        }
        _world = null;
        _activeWorldSessionId = null;
        if (_lobby is not null && GodotObject.IsInstanceValid(_lobby))
        {
            _lobby.Visible = true;
            _lobby.SetStatus(remoteClosed
                ? "Sessão de mundo encerrada no servidor. Você voltou ao salão."
                : "Você voltou ao salão. O encerramento remoto não foi confirmado; a sessão expirará em até dez minutos.", !remoteClosed);
        }
    }

    private async Task ReleaseWorldSessionAsync(Guid worldSessionId)
    {
        if (_accessToken is null) return;
        try
        {
            await _api.SendAsync(Godot.HttpClient.Method.Delete,
                $"/api/v1/world/sessions/{worldSessionId}", bearerToken: _accessToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or TaskCanceledException or TimeoutException)
        {
            // The server-side grace period is the recovery path if explicit cleanup cannot be confirmed.
        }
    }

    private async void OnCreateRequested(string name)
    {
        if (_accessToken is null || _lobby is null) return;

        await RunBusyAsync(async () =>
        {
            var body = JsonSerializer.Serialize(new { name, classId = (string?)null }, _jsonOptions);
            var response = await _api.SendAsync(Godot.HttpClient.Method.Post, "/api/v1/characters", body, _accessToken);
            if (!IsNetworkSuccess(response))
            {
                _lobby.SetStatus("Não foi possível alcançar o servidor. O personagem não foi confirmado; tente novamente.", true);
                return;
            }
            if (response.StatusCode == (long)HttpStatusCode.Unauthorized)
            {
                ExpireSession();
                return;
            }
            if (response.StatusCode != (long)HttpStatusCode.Created)
            {
                _lobby.SetStatus(DescribeApiError(response), true);
                return;
            }
            if (!TryReadCharacter(response.Body, out var createdCharacter))
            {
                _lobby.SetStatus("O servidor confirmou a criação, mas retornou dados inválidos. Atualize a lista antes de tentar novamente.", true);
                return;
            }

            _preferredCharacterId = createdCharacter.CharacterId;
            _lobby.ClearNewCharacterName();
            _lobby.SetStatus("Criação confirmada pelo servidor. Atualizando a lista da conta…");
            await RefreshCharactersAsync();
        });
    }

    private async void OnLogoutRequested() => await LogoutAsync();

    private async Task LogoutAsync()
    {
        var token = _accessToken;
        await RunBusyAsync(async () =>
        {
            var serverRevoked = false;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var response = await _api.SendAsync(Godot.HttpClient.Method.Delete, "/api/v1/sessions/current", bearerToken: token);
                    serverRevoked = IsNetworkSuccess(response) &&
                        (response.StatusCode == (long)HttpStatusCode.NoContent || response.StatusCode == (long)HttpStatusCode.Unauthorized);
                }
                catch (Exception exception) when (exception is InvalidOperationException or TaskCanceledException or TimeoutException)
                {
                    // Local credentials are always cleared, even when the revocation request cannot reach the server.
                }
            }

            ClearLocalSession();
            SetStatus(serverRevoked
                ? "Sessão encerrada no servidor e neste cliente."
                : "Credencial removida deste cliente; a revogação remota não foi confirmada e a sessão expirará ou poderá ser revogada depois.");
        });
    }

    private async Task RefreshCharactersAsync()
    {
        if (_accessToken is null || _lobby is null) return;

        _lobby.SetLoading(true);
        ApiResponse response;
        try
        {
            response = await _api.SendAsync(Godot.HttpClient.Method.Get, "/api/v1/characters", bearerToken: _accessToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or TaskCanceledException or TimeoutException)
        {
            _lobby.SetLoading(false);
            _lobby.SetLoadFailure("Não foi possível carregar os personagens. Verifique o servidor e tente atualizar.");
            _lobby.SetStatus("Falha ao consultar a lista. A sessão continua ativa; tente novamente.", true);
            return;
        }
        if (!IsNetworkSuccess(response))
        {
            _lobby.SetLoading(false);
            _lobby.SetLoadFailure("Não foi possível carregar os personagens. Verifique o servidor e tente atualizar.");
            _lobby.SetStatus("Falha de conexão ao carregar personagens. A lista atual foi preservada; tente novamente.", true);
            _preferredCharacterId = null;
            return;
        }
        if (response.StatusCode == (long)HttpStatusCode.Unauthorized)
        {
            ExpireSession();
            return;
        }
        if (response.StatusCode != (long)HttpStatusCode.OK)
        {
            _lobby.SetLoading(false);
            _lobby.SetLoadFailure("A resposta do servidor não permitiu carregar a lista. Tente atualizar.");
            _lobby.SetStatus(DescribeApiError(response), true);
            _preferredCharacterId = null;
            return;
        }

        try
        {
            var characters = JsonSerializer.Deserialize<List<CharacterSummary>>(response.Body, _jsonOptions)
                ?? throw new JsonException("Lista nula.");
            var seenCharacterIds = new HashSet<Guid>();
            if (characters.Any(character =>
                    !Guid.TryParse(character.CharacterId, out var id) ||
                    !seenCharacterIds.Add(id) ||
                    !IsValidCharacterName(character.Name) ||
                    !IsValidClassId(character.ClassId)))
                throw new JsonException("A lista contém identificadores duplicados ou campos fora do contrato.");

            _lobby.SetCharacters(characters, _preferredCharacterId);
            _preferredCharacterId = null;
            _lobby.SetLoading(false);
            _lobby.SetStatus(characters.Count == 0
                ? "Lista carregada do servidor: nenhum personagem nesta conta."
                : $"Lista carregada do servidor: {characters.Count} personagem(ns). Selecione um cartão para solicitar entrada em Coast.");
        }
        catch (JsonException)
        {
            _lobby.SetLoading(false);
            _lobby.SetLoadFailure("O servidor retornou uma lista inválida. A sessão continua ativa; tente atualizar.");
            _lobby.SetStatus("Resposta inválida ao carregar personagens. A sessão continua em memória; tente atualizar.", true);
            _preferredCharacterId = null;
        }
    }

    private void ExpireSession()
    {
        ClearLocalSession();
        SetStatus("A sessão expirou ou foi revogada pelo servidor. Entre novamente.", true);
    }

    private void ClearLocalSession()
    {
        _accessToken = null;
        _preferredCharacterId = null;
        if (_lobby is not null && GodotObject.IsInstanceValid(_lobby))
        {
            _lobby.Visible = false;
            _lobby.RefreshRequested -= OnRefreshRequested;
            _lobby.CreateRequested -= OnCreateRequested;
            _lobby.LogoutRequested -= OnLogoutRequested;
            _lobby.EnterWorldRequested -= OnEnterWorldRequested;
            _lobby.QueueFree();
        }
        _lobby = null;
        _username.Clear();
        _password.Clear();
        _authenticationPanel.Visible = true;
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        SetBusy(true);
        try
        {
            await action();
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or TaskCanceledException or TimeoutException or JsonException)
        {
            if (_lobby is not null)
                _lobby.SetStatus("Solicitação interrompida. Verifique o servidor e atualize para confirmar o estado salvo.", true);
            else
                SetStatus("Solicitação interrompida. Verifique se o servidor está acessível e tente novamente.", true);
        }
        finally
        {
            _busy = false;
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _loginButton.Disabled = busy;
        _registerButton.Disabled = busy;
        _username.Editable = !busy;
        _password.Editable = !busy;
        if (_lobby is not null && GodotObject.IsInstanceValid(_lobby))
            _lobby.SetBusy(busy);
    }

    private static bool IsNetworkSuccess(ApiResponse response) =>
        response.Result == (long)HttpRequest.Result.Success;

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

    private bool TryReadWorldSnapshot(string body, Guid expectedCharacterId, out WorldSessionSnapshot snapshot)
    {
        snapshot = null!;
        try
        {
            var parsed = JsonSerializer.Deserialize<WorldSessionSnapshot>(body, _jsonOptions);
            if (parsed is null || parsed.WorldSessionId == Guid.Empty || parsed.CharacterId != expectedCharacterId ||
                !IsValidCharacterName(parsed.CharacterName) ||
                !IsValidClassId(parsed.ClassId) || parsed.MapId != "world.coast" || parsed.SpawnId != "coast.initial" ||
                !IsValidWorldPosition(parsed.Position) || !IsValidWorldPosition(parsed.Velocity) ||
                parsed.MovementSpeed is <= 0 or > 10 || parsed.ServerTick < 0 || parsed.LastInputSequence < 0 ||
                parsed.ExpiresAt <= DateTimeOffset.UnixEpoch)
                return false;
            snapshot = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsValidWorldPosition(WorldPosition? position) =>
        position is not null && double.IsFinite(position.X) && double.IsFinite(position.Y) && double.IsFinite(position.Z);

    private bool TryReadCharacter(string body, out CharacterSummary character)
    {
        character = new CharacterSummary(string.Empty, string.Empty, null);
        try
        {
            var parsed = JsonSerializer.Deserialize<CharacterSummary>(body, _jsonOptions);
            if (parsed is null || !Guid.TryParse(parsed.CharacterId, out _) ||
                !IsValidCharacterName(parsed.Name) || !IsValidClassId(parsed.ClassId))
                return false;
            character = parsed;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsValidCharacterName(string? name)
    {
        if (name is not { Length: >= 3 and <= 20 } || !IsAsciiLetter(name[0])) return false;
        return name.Skip(1).All(character => IsAsciiLetter(character) ||
            character is >= '0' and <= '9' or '_');
    }

    private static bool IsValidClassId(string? classId)
    {
        if (classId is null) return true;
        if (classId.Length is < 1 or > 64) return false;
        var previousWasSeparator = true;
        foreach (var character in classId)
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                previousWasSeparator = false;
                continue;
            }
            if ((character is '.' or '_') && !previousWasSeparator)
            {
                previousWasSeparator = true;
                continue;
            }
            return false;
        }
        return !previousWasSeparator;
    }

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static string DescribeApiError(ApiResponse response)
    {
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
            // Status HTTP remains safe to report without echoing a response body.
        }

        return code switch
        {
            "invalid_registration" => "Cadastro inválido: usuário deve ter 3–32 caracteres permitidos e senha 12–128 caracteres.",
            "invalid_credentials" => "Usuário ou senha inválidos.",
            "authentication_required" => "Sessão inválida. Entre novamente.",
            "invalid_character" => "Nome de personagem inválido. Use letra inicial e 3–20 letras, números ou sublinhados.",
            "name_unavailable" => "Esse nome de personagem já está em uso.",
            "class_unavailable" => "A classe solicitada ainda não está disponível.",
            "character_not_found" => "O personagem não existe ou não pertence a esta conta.",
            "world_unavailable" => "O mapa Coast ou o spawn autorizado não está disponível no servidor.",
            _ when response.StatusCode == (long)HttpStatusCode.TooManyRequests => "Muitas tentativas. Aguarde um pouco antes de tentar novamente.",
            _ when response.StatusCode >= 500 => "O servidor está temporariamente indisponível. Tente novamente mais tarde.",
            _ => $"A solicitação não foi concluída (HTTP {response.StatusCode})."
        };
    }

    private void SetStatus(string message, bool isError = false)
    {
        if (!GodotObject.IsInstanceValid(_status)) return;
        _status.Text = message;
        _status.Modulate = isError ? new Color(1.0f, 0.48f, 0.39f) : new Color(0.76f, 0.88f, 0.93f);
    }

    private static Label AddLabel(Container parent, string text, int fontSize)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        parent.AddChild(label);
        return label;
    }

    private static LineEdit AddLineEdit(Container parent, string placeholder)
    {
        var lineEdit = new LineEdit { PlaceholderText = placeholder, CustomMinimumSize = new Vector2(0, 40) };
        parent.AddChild(lineEdit);
        return lineEdit;
    }

    private static Button AddButton(Container parent, string text, Action pressed)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 42) };
        button.Pressed += pressed;
        parent.AddChild(button);
        return button;
    }
}
