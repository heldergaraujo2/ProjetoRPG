using System.Net.WebSockets;
using System.Text.Json;
using Godot;
using ProjetoRpg.Shared.World;

namespace ProjetoRpg.Client.Scripts;

/// <summary>Coast gameplay scene: the server owns session, movement and walkable position.</summary>
public partial class WorldGameplay : Control
{
    private string _baseUrl = string.Empty;
    private string _accessToken = string.Empty;
    private WorldSessionSnapshot _entry = null!;
    private SubViewport _worldViewport = null!;
    private Node3D _worldRoot = null!;
    private CharacterActor _player = null!;
    private Camera3D _camera = null!;
    private Label _connectionStatus = null!;
    private Button _retryButton = null!;
    private bool _connecting;
    private bool _wasConnected;
    private double _sendAccumulator;
    private int _sendPending;
    private WorldRealtimeClient? _realtime;

    public event Action? ReturnRequested;

    public void Initialize(string baseUrl, string accessToken, WorldSessionSnapshot entry)
    {
        _baseUrl = baseUrl;
        _accessToken = accessToken;
        _entry = entry;
    }

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        try
        {
            BuildCoastWorld();
            BuildOverlay();
            _ = ConnectWorldAsync();
        }
        catch (Exception exception) when (exception is InvalidOperationException or InvalidCastException)
        {
            BuildOverlay();
            SetConnectionStatus($"Não foi possível carregar Coast: {exception.Message}. Retorno ao salão em instantes.", true);
            _retryButton.Visible = false;
            _ = ReturnAfterLoadFailureAsync();
        }
    }

    public override void _Process(double delta)
    {
if (_player is not null && _camera is not null)
        {
            _camera.GlobalPosition = _player.GlobalPosition + new Vector3(0, 8.5f, 12.0f);
            _camera.LookAt(_player.GlobalPosition + new Vector3(0, 1.0f, 0), Vector3.Up);
        }

        var connected = _realtime?.IsConnected == true;
        if (connected)
        {
            _wasConnected = true;
            _sendAccumulator += delta;
            if (_sendAccumulator >= 0.05 && Interlocked.CompareExchange(ref _sendPending, 1, 0) == 0)
            {
                _sendAccumulator = 0;
                _ = SendCurrentIntentAsync(_realtime!, ReadMoveDirection());
            }
            while (_realtime!.TryDequeueState(out var state) && state is not null)
                ApplySnapshot(state);
        }
        else if (_wasConnected)
        {
            _wasConnected = false;
            _player?.ApplyAuthoritativeState(_player.GlobalPosition, Vector3.Zero);
            SetConnectionStatus(_realtime?.LastError ?? "Conexão interrompida. A posição confirmada permanece salva para reconexão.", true);
            _retryButton.Visible = true;
        }
    }

    public override void _ExitTree()
    {
        _accessToken = string.Empty;
        if (_realtime is not null)
            _ = _realtime.DisposeAsync();
    }

    private void BuildCoastWorld()
    {
        if (_entry is null || _entry.WorldSessionId == Guid.Empty || _entry.CharacterId == Guid.Empty ||
            _entry.MapId != "world.coast" || _entry.SpawnId != "coast.initial" || !IsFinite(_entry.Position))
            throw new InvalidOperationException("O estado de entrada autorizado pelo servidor está incompleto.");

        var viewportContainer = new SubViewportContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            Stretch = true
        };
        viewportContainer.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(viewportContainer);
        _worldViewport = new SubViewport
        {
            Size = new Vector2I(1280, 720),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
            HandleInputLocally = false
        };
        viewportContainer.AddChild(_worldViewport);
        _worldRoot = new Node3D { Name = "AuthorizedCoastGameplay" };
        _worldViewport.AddChild(_worldRoot);

        var coastScene = ResourceLoader.Load<PackedScene>("res://Assets/Worlds/JDungeon3D-Coast/CoastWorld.tscn")
            ?? throw new InvalidOperationException("A cena importada CoastWorld.tscn não está disponível.");
        var coast = coastScene.Instantiate<Node3D>()
            ?? throw new InvalidOperationException("A cena CoastWorld não possui raiz Node3D.");
        coast.Name = "CoastWorld";
        _worldRoot.AddChild(coast);

        var actorScene = ResourceLoader.Load<PackedScene>("res://Scenes/CharacterActor.tscn")
            ?? throw new InvalidOperationException("A cena própria CharacterActor.tscn não está disponível.");
        _player = actorScene.Instantiate<CharacterActor>()
            ?? throw new InvalidOperationException("O ator próprio não pôde ser criado.");
        _player.Name = $"AuthorizedCharacter_{_entry.CharacterId:N}";
        _player.PlayerControlled = false;
        _player.TargetMarkerPath = new NodePath("");
        var (modelPath, yawDegrees) = CharacterLobby.ResolvePreviewModel(_entry.ClassId);
        _player.ModelScene = ResourceLoader.Load<PackedScene>(modelPath)
            ?? throw new InvalidOperationException("O modelo ilustrativo do personagem não está disponível.");
        _player.VisualScale = 1.0f;
        _player.VisualYawCorrectionDegrees = yawDegrees;
        _worldRoot.AddChild(_player);
        _player.GlobalPosition = ToGodot(_entry.Position);
        _camera = new Camera3D
        {
            Name = "GameplayCamera",
            Current = true,
            Fov = 60
        };
        _worldRoot.AddChild(_camera);
        _camera.GlobalPosition = _player.GlobalPosition + new Vector3(0, 8.5f, 12.0f);
        _camera.LookAt(_player.GlobalPosition + new Vector3(0, 1.0f, 0), Vector3.Up);
        ApplySnapshot(_entry);
    }

    private void BuildOverlay()
    {
        var panel = new PanelContainer
        {
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 0,
            AnchorBottom = 0,
            OffsetLeft = 18,
            OffsetTop = 18,
            OffsetRight = 440,
            OffsetBottom = 232,
            MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(panel);
        var layout = new VBoxContainer();
        layout.AddThemeConstantOverride("separation", 8);
        panel.AddChild(layout);
        AddLabel(layout, $"PROJETORPG  /  COAST  /  {_entry?.CharacterName ?? "personagem"}", 17);
        _connectionStatus = AddLabel(layout, "Validando sessão autorizada…", 13);
        AddLabel(layout, "Mover: W A S D ou setas · servidor valida velocidade e navegação", 12);
        AddLabel(layout, "Câmera acompanha o personagem · colisões pelo navmesh autorizado", 12);
        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 8);
        layout.AddChild(actions);
        _retryButton = AddButton(actions, "Tentar reconectar", OnRetryPressed);
        _retryButton.Visible = false;
        AddButton(actions, "Voltar ao salão", () => ReturnRequested?.Invoke());
    }

    private async Task ReturnAfterLoadFailureAsync()
    {
        await ToSignal(GetTree().CreateTimer(3.0), SceneTreeTimer.SignalName.Timeout);
        if (GodotObject.IsInstanceValid(this))
            ReturnRequested?.Invoke();
    }

    private async void OnRetryPressed() => await ConnectWorldAsync();

    private async Task ConnectWorldAsync()
    {
        if (_connecting || string.IsNullOrWhiteSpace(_accessToken)) return;
        _connecting = true;
        _retryButton.Disabled = true;
        SetConnectionStatus("Conectando ao servidor e validando a sessão de mundo…");
        var candidate = new WorldRealtimeClient();
        try
        {
            var snapshot = await candidate.ConnectAsync(_baseUrl, _accessToken, _entry.WorldSessionId, CancellationToken.None);
            if (!GodotObject.IsInstanceValid(this))
            {
                await candidate.DisposeAsync();
                return;
            }
            if (_realtime is not null)
                await _realtime.DisposeAsync();
            _realtime = candidate;
            _wasConnected = true;
            _retryButton.Visible = false;
            ApplySnapshot(snapshot);
            SetConnectionStatus($"Conectado · posição e intenção validadas pelo servidor (tick {snapshot.ServerTick}).");
        }
        catch (Exception exception) when (exception is InvalidOperationException or WebSocketException or OperationCanceledException or JsonException)
        {
            await candidate.DisposeAsync();
            if (GodotObject.IsInstanceValid(this))
            {
                SetConnectionStatus(exception is InvalidOperationException
                    ? exception.Message
                    : "Não foi possível conectar ao servidor. A sessão permanece recuperável por até dez minutos.", true);
                _retryButton.Visible = true;
            }
        }
        finally
        {
            _connecting = false;
            if (GodotObject.IsInstanceValid(_retryButton))
                _retryButton.Disabled = false;
        }
    }

    private async Task SendCurrentIntentAsync(WorldRealtimeClient client, Vector2 direction)
    {
        try
        {
            await client.SendMoveAsync(direction);
        }
        catch (Exception exception) when (exception is InvalidOperationException or WebSocketException or OperationCanceledException or ObjectDisposedException)
        {
            // _Process observes the transport state and presents a reconnect action.
        }
        finally
        {
            Interlocked.Exchange(ref _sendPending, 0);
        }
    }

    private void ApplySnapshot(WorldSessionSnapshot snapshot)
    {
        if (snapshot.WorldSessionId != _entry.WorldSessionId || snapshot.CharacterId != _entry.CharacterId ||
            snapshot.MapId != "world.coast" || !IsFinite(snapshot.Position) || !IsFinite(snapshot.Velocity))
        {
            SetConnectionStatus("O servidor retornou estado de mundo incompatível; o movimento foi bloqueado.", true);
            return;
        }
        _player.ApplyAuthoritativeState(ToGodot(snapshot.Position), ToGodot(snapshot.Velocity));
    }

    private static Vector2 ReadMoveDirection()
    {
        var x = (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right) ? 1.0f : 0.0f) -
                (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left) ? 1.0f : 0.0f);
        var z = (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down) ? 1.0f : 0.0f) -
                (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up) ? 1.0f : 0.0f);
        var direction = new Vector2(x, z);
        return direction.LengthSquared() > 1.0f ? direction.Normalized() : direction;
    }

    private void SetConnectionStatus(string text, bool isError = false)
    {
        if (!GodotObject.IsInstanceValid(_connectionStatus)) return;
        _connectionStatus.Text = text;
        _connectionStatus.Modulate = isError ? new Color(1.0f, 0.48f, 0.39f) : new Color(0.55f, 0.93f, 0.78f);
    }

    private static bool IsFinite(WorldPosition? value) =>
        value is not null && double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);

    private static Vector3 ToGodot(WorldPosition value) => new((float)value.X, (float)value.Y, (float)value.Z);

    private static Label AddLabel(Container parent, string text, int fontSize)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        parent.AddChild(label);
        return label;
    }

    private static Button AddButton(Container parent, string text, Action pressed)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(0, 40) };
        button.Pressed += pressed;
        parent.AddChild(button);
        return button;
    }
}
