using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>Account-backed character selection with one isolated animated preview per server record.</summary>
public partial class CharacterLobby : Control
{
    public event Action? RefreshRequested;
    public event Action<string>? CreateRequested;
    public event Action? LogoutRequested;

    private GridContainer _characterCards = null!;
    private Label _emptyState = null!;
    private Label _selectedDetails = null!;
    private Label _status = null!;
    private LineEdit _newCharacterName = null!;
    private Button _createButton = null!;
    private Button _refreshButton = null!;
    private Button _logoutButton = null!;
    private Button _enterWorldButton = null!;
    private readonly Dictionary<PanelContainer, string> _cardCharacterIds = new();
    private readonly Dictionary<Button, string> _selectionButtons = new();
    private IReadOnlyList<CharacterSummary> _characters = Array.Empty<CharacterSummary>();
    private string? _selectedCharacterId;
    private bool _busy;
    private bool _hasLoadedCharacters;

    public string? SelectedCharacterId => _selectedCharacterId;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        BuildInterface();
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
        ClearCharacterCards();
        _emptyState.Visible = characters.Count == 0;

        if (characters.Count == 0)
        {
            _selectedCharacterId = null;
            _selectedDetails.Text = "Nenhum personagem selecionado.";
            _emptyState.Text = "Esta conta ainda não tem personagens. Crie um usando o formulário abaixo.";
            return;
        }

        foreach (var character in characters)
            AddCharacterCard(character);

        var selected = characters.FirstOrDefault(character =>
            string.Equals(character.CharacterId, preferredCharacterId ?? _selectedCharacterId, StringComparison.Ordinal))
            ?? characters[0];
        SelectCharacter(selected, updateStatus: false);
        SetStatus($"{characters.Count} personagem(ns) carregado(s) da conta. Cada cartão é um registro real retornado pela API.");
    }

    public void SetBusy(bool busy)
    {
        _busy = busy;
        _createButton.Disabled = busy;
        _refreshButton.Disabled = busy;
        _logoutButton.Disabled = busy;
        _newCharacterName.Editable = !busy;
        // World entry remains gated until authenticated entry, server spawn, and authoritative movement are implemented.
        _enterWorldButton.Disabled = true;
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
        var subtitle = AddLabel(header, "Prévia 3D ilustrativa · dados da conta", 13);
        subtitle.CustomMinimumSize = new Vector2(340, 0);
        subtitle.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
        subtitle.HorizontalAlignment = HorizontalAlignment.Right;
        subtitle.VerticalAlignment = VerticalAlignment.Center;

        var body = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 18);
        page.AddChild(body);

        var stagePanel = CreatePanel();
        stagePanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        stagePanel.SizeFlagsVertical = SizeFlags.ExpandFill;
        body.AddChild(stagePanel);
        var stage = new VBoxContainer();
        stage.AddThemeConstantOverride("separation", 9);
        stagePanel.AddChild(stage);
        var stageTitle = AddLabel(stage, "PERSONAGENS DA CONTA  ·  selecione um cartão", 13);
        stageTitle.Modulate = new Color(0.62f, 0.83f, 0.86f);
        var scroll = new ScrollContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        stage.AddChild(scroll);
        _characterCards = new GridContainer
        {
            Columns = 3,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _characterCards.AddThemeConstantOverride("h_separation", 10);
        _characterCards.AddThemeConstantOverride("v_separation", 10);
        scroll.AddChild(_characterCards);

        var sidePanel = CreatePanel();
        sidePanel.CustomMinimumSize = new Vector2(330, 0);
        sidePanel.SizeFlagsVertical = SizeFlags.ExpandFill;
        body.AddChild(sidePanel);
        var side = new VBoxContainer();
        side.AddThemeConstantOverride("separation", 10);
        sidePanel.AddChild(side);
        AddLabel(side, "CONTA", 12).Modulate = new Color(0.52f, 0.77f, 0.83f);
        AddLabel(side, "Seus personagens", 21);
        AddLabel(side, "Os cartões usam somente registros reais da API. Classe, nível, atributos e aparência não são inferidos.", 13);
        _emptyState = AddLabel(side, "Carregando personagens da conta…", 14);
        _emptyState.CustomMinimumSize = new Vector2(0, 58);
        _selectedDetails = AddLabel(side, "Nenhum personagem selecionado.", 14);
        _selectedDetails.CustomMinimumSize = new Vector2(0, 72);

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
        _enterWorldButton = AddButton(footer, "Entrar no mundo — em implementação", () => { });
        _enterWorldButton.CustomMinimumSize = new Vector2(260, 40);
        _enterWorldButton.Disabled = true;
        _enterWorldButton.TooltipText = "A entrada permanece bloqueada até haver validação server-authoritative de sessão, ownership, mapa, spawn e movimento.";
        _logoutButton = AddButton(footer, "Trocar conta / sair", () =>
        {
            if (!_busy) LogoutRequested?.Invoke();
        });
        _logoutButton.CustomMinimumSize = new Vector2(170, 40);
    }

    private void AddCharacterCard(CharacterSummary character)
    {
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(224, 360),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _cardCharacterIds[card] = character.CharacterId;
        _characterCards.AddChild(card);

        var contents = new VBoxContainer();
        contents.AddThemeConstantOverride("separation", 6);
        card.AddChild(contents);
        contents.AddChild(CreateCharacterViewport(character));

        var name = AddLabel(contents, character.Name, 16);
        name.HorizontalAlignment = HorizontalAlignment.Center;
        var classText = string.IsNullOrWhiteSpace(character.ClassId)
            ? "Sem classe atribuída · prévia genérica"
            : $"Classe: {character.ClassId}";
        var facts = AddLabel(contents, classText, 11);
        facts.HorizontalAlignment = HorizontalAlignment.Center;
        facts.CustomMinimumSize = new Vector2(0, 32);

        var button = AddButton(contents, "Selecionar", () => SelectCharacter(character));
        button.ToggleMode = true;
        _selectionButtons[button] = character.CharacterId;
        ApplyCardStyle(card, selected: false);
    }

    private static Control CreateCharacterViewport(CharacterSummary character)
    {
        var viewportContainer = new SubViewportContainer
        {
            CustomMinimumSize = new Vector2(0, 245),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Stretch = true,
            MouseFilter = MouseFilterEnum.Ignore
        };
        var viewport = new SubViewport
        {
            Size = new Vector2I(320, 300),
            RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible,
            HandleInputLocally = false
        };
        viewportContainer.AddChild(viewport);

        var world = new Node3D { Name = $"Preview_{character.CharacterId:N}" };
        viewport.AddChild(world);
        var environment = ResourceLoader.Load<Godot.Environment>("res://Scenes/CharacterLobbyEnvironment.tres")
            ?? throw new InvalidOperationException("O ambiente 3D do lobby não está disponível.");
        world.AddChild(new WorldEnvironment { Environment = environment });
        world.AddChild(new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-43, -28, 0),
            LightColor = new Color(0.78f, 0.88f, 1.0f),
            LightEnergy = 1.25f,
            ShadowEnabled = true
        });
        world.AddChild(new OmniLight3D
        {
            Position = new Vector3(-2.5f, 2.8f, 1.0f),
            LightColor = new Color(0.24f, 0.78f, 0.72f),
            LightEnergy = 0.8f,
            OmniRange = 8.0f
        });

        var (modelPath, yawDegrees) = ResolvePreviewModel(character.ClassId);
        var model = ResourceLoader.Load<PackedScene>(modelPath);
        var actorScene = ResourceLoader.Load<PackedScene>("res://Scenes/CharacterActor.tscn");
        if (model is null || actorScene is null)
        {
            GD.PushWarning($"A prévia ilustrativa de {character.Name} não pôde carregar os recursos de personagem.");
            return viewportContainer;
        }

        var actor = actorScene.Instantiate<CharacterActor>();
        if (actor is null)
        {
            GD.PushWarning($"A cena de ator não pôde ser instanciada para {character.Name}.");
            return viewportContainer;
        }
        actor.Name = $"AccountCharacter_{character.CharacterId:N}";
        actor.ModelScene = model;
        actor.PlayerControlled = false;
        actor.VisualScale = 1.15f;
        actor.VisualYawCorrectionDegrees = yawDegrees;
        actor.TargetMarkerPath = new NodePath("");
        world.AddChild(actor);

        var plinth = new MeshInstance3D
        {
            Position = new Vector3(0, -0.16f, 0),
            Mesh = new CylinderMesh { TopRadius = 0.53f, BottomRadius = 0.57f, Height = 0.14f, RadialSegments = 48 },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.13f, 0.21f, 0.25f),
                Metallic = 0.45f,
                Roughness = 0.4f
            }
        };
        world.AddChild(plinth);

        var camera = new Camera3D
        {
            Position = new Vector3(0, 2.35f, 6.7f),
            Current = true,
            Fov = 34
        };
        world.AddChild(camera);
        camera.LookAt(new Vector3(0, 0.9f, 0), Vector3.Up);
        return viewportContainer;
    }

    private static (string Path, float YawDegrees) ResolvePreviewModel(string? classId)
    {
        // Class IDs are display hints only. Unknown/null classes use an explicitly
        // generic visual rather than inventing a class or additional character.
        var normalized = classId?.Trim().ToLowerInvariant();
        if (normalized is "mage" or "adventurer.mage")
            return ("res://Assets/Characters/KayKitAdventurers/Mage.glb", 0.0f);
        if (normalized is "barbarian" or "adventurer.barbarian")
            return ("res://Assets/Characters/KayKitAdventurers/Barbarian.glb", 180.0f);
        return ("res://Assets/Characters/KayKitAdventurers/Barbarian.glb", 180.0f);
    }

    private void SelectCharacter(CharacterSummary character, bool updateStatus = true)
    {
        _selectedCharacterId = character.CharacterId;
        _selectedDetails.Text = string.IsNullOrWhiteSpace(character.ClassId)
            ? $"Selecionado: {character.Name}\nClasse não atribuída. A figura é uma prévia genérica, não aparência persistida."
            : $"Selecionado: {character.Name}\nClasse: {character.ClassId}\nA figura é uma prévia ilustrativa, não estado de gameplay.";
        RefreshCardSelection();
        if (updateStatus)
            SetStatus($"{character.Name} selecionado. A seleção não inicia sessão de mundo.");
    }

    private void RefreshCardSelection()
    {
        foreach (var (card, characterId) in _cardCharacterIds)
            ApplyCardStyle(card, string.Equals(characterId, _selectedCharacterId, StringComparison.Ordinal));
        foreach (var (button, characterId) in _selectionButtons)
        {
            var selected = string.Equals(characterId, _selectedCharacterId, StringComparison.Ordinal);
            button.ButtonPressed = selected;
            button.Text = selected ? "Selecionado" : "Selecionar";
        }
    }

    private static void ApplyCardStyle(PanelContainer card, bool selected)
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.045f, 0.082f, 0.105f, 0.98f),
            BorderColor = selected ? new Color(0.31f, 0.92f, 0.75f, 1.0f) : new Color(0.19f, 0.39f, 0.42f, 1.0f),
            BorderWidthBottom = selected ? 3 : 1,
            BorderWidthLeft = selected ? 3 : 1,
            BorderWidthRight = selected ? 3 : 1,
            BorderWidthTop = selected ? 3 : 1,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            ContentMarginLeft = 8,
            ContentMarginRight = 8,
            ContentMarginTop = 8,
            ContentMarginBottom = 8
        };
        card.AddThemeStyleboxOverride("panel", style);
    }

    private void ClearCharacterCards()
    {
        foreach (var child in _characterCards.GetChildren())
        {
            _characterCards.RemoveChild(child);
            child.QueueFree();
        }
        _cardCharacterIds.Clear();
        _selectionButtons.Clear();
    }

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
