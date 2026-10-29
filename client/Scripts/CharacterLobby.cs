using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>Account-backed character selection with an isolated, non-interactive 3D preview.</summary>
public partial class CharacterLobby : Control
{
    public event Action? RefreshRequested;
    public event Action<string>? CreateRequested;
    public event Action? LogoutRequested;

    private VBoxContainer _characterRows = null!;
    private Label _emptyState = null!;
    private Label _selectedDetails = null!;
    private Label _status = null!;
    private LineEdit _newCharacterName = null!;
    private Button _createButton = null!;
    private Button _refreshButton = null!;
    private Button _logoutButton = null!;
    private Button _enterWorldButton = null!;
    private Node3D _previewRoot = null!;
    private Node3D? _figure;
    private IReadOnlyList<CharacterSummary> _characters = Array.Empty<CharacterSummary>();
    private readonly Dictionary<Button, string> _rowCharacterIds = new();
    private string? _selectedCharacterId;
    private bool _busy;
    private bool _hasLoadedCharacters;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        BuildInterface();
        BuildPreviewWorld();
        SetStatus("Carregue a lista da conta para selecionar um personagem.");
    }

    public void SetLoading(bool loading)
    {
        if (_characters.Count != 0) return;
        _emptyState.Visible = true;
        if (loading)
            _emptyState.Text = "Carregando personagens da conta…";
        else if (_hasLoadedCharacters)
            _emptyState.Text = "Esta conta ainda não tem personagens. Crie um usando o formulário abaixo.";
    }

    public void SetLoadFailure(string message)
    {
        if (_characters.Count == 0)
        {
            _emptyState.Visible = true;
            _emptyState.Text = message;
        }
    }

    public void SetCharacters(IReadOnlyList<CharacterSummary> characters, string? preferredCharacterId = null)
    {
        _hasLoadedCharacters = true;
        _characters = characters;
        _rowCharacterIds.Clear();
        _characterRows.GetChildren().ToList().ForEach(child => child.QueueFree());
        _emptyState.Visible = characters.Count == 0;

        foreach (var character in characters)
        {
            var classText = string.IsNullOrWhiteSpace(character.ClassId) ? "Sem classe" : character.ClassId;
            var row = new Button
            {
                Text = $"{character.Name}\n{classText}",
                Alignment = HorizontalAlignment.Left,
                CustomMinimumSize = new Vector2(0, 58),
                ToggleMode = true,
                ButtonPressed = string.Equals(character.CharacterId, preferredCharacterId ?? _selectedCharacterId, StringComparison.Ordinal)
            };
            row.Pressed += () => SelectCharacter(character);
            _rowCharacterIds[row] = character.CharacterId;
            _characterRows.AddChild(row);
        }

        if (characters.Count == 0)
        {
            _selectedCharacterId = null;
            _selectedDetails.Text = "Nenhum personagem selecionado.";
            ClearPreview();
            return;
        }

        var selected = characters.FirstOrDefault(character =>
            string.Equals(character.CharacterId, preferredCharacterId ?? _selectedCharacterId, StringComparison.Ordinal))
            ?? characters[0];
        SelectCharacter(selected);
    }

    public void SetBusy(bool busy)
    {
        _busy = busy;
        _createButton.Disabled = busy;
        _refreshButton.Disabled = busy;
        _logoutButton.Disabled = busy;
        _newCharacterName.Editable = !busy;
    }

    public void SetStatus(string message, bool isError = false)
    {
        _status.Text = message;
        _status.Modulate = isError ? new Color(1.0f, 0.48f, 0.39f) : new Color(0.76f, 0.88f, 0.93f);
    }

    public void ClearNewCharacterName() => _newCharacterName.Clear();

    private void BuildInterface()
    {
        var backdrop = new ColorRect
        {
            Color = new Color(0.025f, 0.045f, 0.071f, 1.0f),
            MouseFilter = MouseFilterEnum.Ignore
        };
        backdrop.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(backdrop);

        // Keep the lobby content centered instead of stretching panels to the screen edges.
        var center = new CenterContainer
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(center);

        var margin = new MarginContainer
        {
            CustomMinimumSize = new Vector2(1280, 0),
            OffsetLeft = 20,
            OffsetTop = 12,
            OffsetRight = -20,
            OffsetBottom = -12
        };
        center.AddChild(margin);
        var page = new VBoxContainer();
        page.AddThemeConstantOverride("separation", 14);
        margin.AddChild(page);

        var header = new HBoxContainer { CustomMinimumSize = new Vector2(0, 68) };
        header.AddThemeConstantOverride("separation", 12);
        page.AddChild(header);
        var title = AddLabel(header, "PROJETORPG  /  SALÃO DE PERSONAGENS", 22);
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddLabel(header, "Prévia visual genérica · sem dados de gameplay", 13);

        var body = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 18);
        page.AddChild(body);

        var stagePanel = CreatePanel();
        stagePanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        stagePanel.SizeFlagsVertical = SizeFlags.ExpandFill;
        body.AddChild(stagePanel);
        var viewport = new SubViewportContainer
        {
            Stretch = true,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore
        };
        stagePanel.AddChild(viewport);
        var subViewport = new SubViewport
        {
            Size = new Vector2I(1000, 720),
            RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible,
            HandleInputLocally = false
        };
        viewport.AddChild(subViewport);
        _previewRoot = new Node3D { Name = "PreviewWorld" };
        subViewport.AddChild(_previewRoot);

        var sidePanel = CreatePanel();
        sidePanel.CustomMinimumSize = new Vector2(330, 0);
        sidePanel.SizeFlagsVertical = SizeFlags.ExpandFill;
        body.AddChild(sidePanel);
        var side = new VBoxContainer();
        side.AddThemeConstantOverride("separation", 10);
        sidePanel.AddChild(side);
        AddLabel(side, "CONTA", 12).Modulate = new Color(0.52f, 0.77f, 0.83f);
        AddLabel(side, "Seus personagens", 21);
        AddLabel(side, "Somente os dados fornecidos pelo servidor são exibidos. Classe, nível e atributos não são inferidos.", 13);
        var scroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(0, 190),
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        side.AddChild(scroll);
        _characterRows = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _characterRows.AddThemeConstantOverride("separation", 6);
        scroll.AddChild(_characterRows);
        _emptyState = AddLabel(side, "Carregando personagens da conta…", 14);
        _emptyState.CustomMinimumSize = new Vector2(0, 58);
        _selectedDetails = AddLabel(side, "Nenhum personagem selecionado.", 14);
        _selectedDetails.CustomMinimumSize = new Vector2(0, 56);

        AddLabel(side, "Criar personagem", 18);
        _newCharacterName = new LineEdit
        {
            PlaceholderText = "Nome (3–20 caracteres)",
            CustomMinimumSize = new Vector2(0, 40),
            MaxLength = 20
        };
        side.AddChild(_newCharacterName);
        _createButton = AddButton(side, "Criar e salvar", () =>
        {
            if (!_busy) CreateRequested?.Invoke(_newCharacterName.Text.Trim());
        });
        _refreshButton = AddButton(side, "Atualizar lista", () =>
        {
            if (!_busy) RefreshRequested?.Invoke();
        });

        var footer = new HBoxContainer { CustomMinimumSize = new Vector2(0, 72) };
        footer.AddThemeConstantOverride("separation", 12);
        page.AddChild(footer);
        _status = AddLabel(footer, string.Empty, 13);
        _status.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _status.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        _enterWorldButton = AddButton(footer, "Entrar no mundo — indisponível", () => { });
        _enterWorldButton.Disabled = true;
        _enterWorldButton.TooltipText = "O servidor ainda não oferece uma operação autoritativa de entrada no mundo.";
        _logoutButton = AddButton(footer, "Trocar conta / sair", () =>
        {
            if (!_busy) LogoutRequested?.Invoke();
        });
    }

    private void BuildPreviewWorld()
    {
        var environment = ResourceLoader.Load<Godot.Environment>("res://Scenes/CharacterLobbyEnvironment.tres")
            ?? throw new InvalidOperationException("O ambiente 3D do lobby não está disponível.");
        var worldEnvironment = new WorldEnvironment { Environment = environment };
        _previewRoot.AddChild(worldEnvironment);

        // Compact display plinth: much narrower than the character silhouette.
        AddStageMesh(new CylinderMesh { TopRadius = 0.52f, BottomRadius = 0.58f, Height = 0.16f, RadialSegments = 48 },
            new Vector3(0, -0.13f, 0), new Color(0.13f, 0.21f, 0.25f), metallic: 0.48f);
        AddStageMesh(new CylinderMesh { TopRadius = 0.43f, BottomRadius = 0.43f, Height = 0.025f, RadialSegments = 48 },
            new Vector3(0, -0.035f, 0), new Color(0.26f, 0.43f, 0.42f), metallic: 0.25f);
        AddStageMesh(new CylinderMesh { TopRadius = 0.62f, BottomRadius = 0.62f, Height = 0.045f, RadialSegments = 48 },
            new Vector3(0, -0.235f, 0), new Color(0.18f, 0.31f, 0.34f), metallic: 0.6f);

        for (var index = 0; index < 8; index++)
        {
            var angle = Mathf.Tau * index / 8.0f;
            var pillar = new MeshInstance3D
            {
                Mesh = new CylinderMesh { TopRadius = 0.16f, BottomRadius = 0.23f, Height = 2.4f, RadialSegments = 16 },
                Position = new Vector3(Mathf.Cos(angle) * 4.4f, 0.8f, Mathf.Sin(angle) * 4.4f),
                MaterialOverride = MakeMaterial(new Color(0.11f, 0.19f, 0.24f), 0.42f)
            };
            _previewRoot.AddChild(pillar);
            var crown = new MeshInstance3D
            {
                Mesh = new SphereMesh { Radius = 0.24f, Height = 0.48f, RadialSegments = 16, Rings = 8 },
                Position = pillar.Position + new Vector3(0, 1.25f, 0),
                MaterialOverride = MakeMaterial(new Color(0.24f, 0.68f, 0.65f), 0.24f)
            };
            _previewRoot.AddChild(crown);
        }

        var keyLight = new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-43, -28, 0),
            LightColor = new Color(0.78f, 0.88f, 1.0f),
            LightEnergy = 1.25f,
            ShadowEnabled = true
        };
        _previewRoot.AddChild(keyLight);
        var fillLight = new OmniLight3D
        {
            Position = new Vector3(-3.0f, 3.1f, 1.2f),
            LightColor = new Color(0.24f, 0.78f, 0.72f),
            LightEnergy = 0.8f,
            OmniRange = 9.0f
        };
        _previewRoot.AddChild(fillLight);
        var camera = new Camera3D
        {
            // Frontal, elevated and distant framing inspired by classic MMORPG character halls.
            Position = new Vector3(0, 3.45f, 12.8f),
            Current = true,
            Fov = 32
        };
        // LookAt requires the node to be inside the scene tree.
        _previewRoot.AddChild(camera);
        camera.LookAt(new Vector3(0, 1.15f, 0), Vector3.Up);
    }

    private void SelectCharacter(CharacterSummary character)
    {
        _selectedCharacterId = character.CharacterId;
        foreach (var node in _characterRows.GetChildren())
        {
            if (node is Button button && _rowCharacterIds.TryGetValue(button, out var rowId))
                button.ButtonPressed = string.Equals(rowId, character.CharacterId, StringComparison.Ordinal);
        }

        var classText = string.IsNullOrWhiteSpace(character.ClassId) ? "sem classe atribuída" : $"classe {character.ClassId}";
        _selectedDetails.Text = $"Selecionado: {character.Name}\n{classText}\nPrévia 3D ilustrativa.";
        ReplacePreviewFigure(character.ClassId);
        SetStatus("Personagem selecionado. Nenhuma sessão de mundo foi criada.");
    }

    private void ReplacePreviewFigure(string? classId)
    {
        ClearPreview();

        // Use the real imported GLB assets. ClassId is only a display hint;
        // the server contract remains the source of character data.
        var normalizedClass = classId?.Trim().ToLowerInvariant() ?? string.Empty;
        var modelPath = normalizedClass.Contains("mage") || normalizedClass.Contains("wizard") ||
                        normalizedClass.Contains("mago") || normalizedClass.Contains("feitice")
            ? "res://Assets/Characters/KayKitAdventurers/Mage.glb"
            : "res://Assets/Characters/KayKitAdventurers/Barbarian.glb";

        var packedModel = ResourceLoader.Load<PackedScene>(modelPath);
        if (packedModel is null)
        {
            GD.PushWarning($"Não foi possível carregar a prévia 3D: {modelPath}");
            return;
        }

        var model = packedModel.Instantiate<Node3D>();
        if (model is null)
        {
            GD.PushWarning($"O modelo não possui raiz Node3D: {modelPath}");
            return;
        }

        model.Name = "CharacterModelPreview";
        // Scale only the preview instance; the imported asset remains untouched.
        model.Scale = new Vector3(1.35f, 1.35f, 1.35f);
        model.Position = Vector3.Zero;
        _previewRoot.AddChild(model);
        _figure = model;
    }

    private void ClearPreview()
    {
        if (_figure is null || !GodotObject.IsInstanceValid(_figure)) return;
        _previewRoot.RemoveChild(_figure);
        _figure.Free();
        _figure = null;
    }

    private void AddFigurePart(Node3D parent, Mesh mesh, Vector3 position, Material material, Vector3? rotation = null)
    {
        var part = new MeshInstance3D { Mesh = mesh, Position = position, MaterialOverride = material };
        if (rotation is { } angle) part.Rotation = angle;
        parent.AddChild(part);
    }

    private void AddStageMesh(Mesh mesh, Vector3 position, Color color, float metallic)
    {
        _previewRoot.AddChild(new MeshInstance3D
        {
            Mesh = mesh,
            Position = position,
            MaterialOverride = MakeMaterial(color, metallic)
        });
    }

    private static StandardMaterial3D MakeMaterial(Color color, float metallic) => new()
    {
        AlbedoColor = color,
        Metallic = metallic,
        Roughness = 0.38f
    };

    private static PanelContainer CreatePanel()
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.045f, 0.082f, 0.105f, 0.96f),
            BorderColor = new Color(0.19f, 0.39f, 0.42f, 1.0f),
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            ContentMarginLeft = 14,
            ContentMarginRight = 14,
            ContentMarginTop = 12,
            ContentMarginBottom = 12
        };
        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

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
