using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>
/// Isolated, dependency-free port of the visual character-selector stage.
/// Uses the existing KayKit GLBs and intentionally does not own account or network state.
/// </summary>
public partial class TemplateCharacterSelectorStage : Node3D
{
    private static readonly string[] ModelPaths =
    {
        "res://Assets/Characters/KayKitAdventurers/Barbarian.glb",
        "res://Assets/Characters/KayKitAdventurers/Mage.glb"
    };

    private Node3D _characterRoot = null!;
    private Camera3D _camera = null!;
    private Label3D _nameLabel = null!;
    private int _selectedIndex;

    public override void _Ready()
    {
        _characterRoot = GetNode<Node3D>("CharacterRoot");
        _camera = GetNode<Camera3D>("Camera3D");
        _nameLabel = GetNode<Label3D>("NameLabel");
        _camera.LookAt(new Vector3(0, 1.15f, 0), Vector3.Up);
        ShowCharacter(0);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_left"))
            ShowCharacter((_selectedIndex + ModelPaths.Length - 1) % ModelPaths.Length);
        else if (Input.IsActionJustPressed("ui_right"))
            ShowCharacter((_selectedIndex + 1) % ModelPaths.Length);
    }

    public void SelectNext() => ShowCharacter((_selectedIndex + 1) % ModelPaths.Length);
    public void SelectPrevious() => ShowCharacter((_selectedIndex + ModelPaths.Length - 1) % ModelPaths.Length);

    private void ShowCharacter(int index)
    {
        _selectedIndex = index;
        foreach (var child in _characterRoot.GetChildren())
        {
            _characterRoot.RemoveChild(child);
            child.QueueFree();
        }

        var packed = ResourceLoader.Load<PackedScene>(ModelPaths[_selectedIndex]);
        if (packed is null)
        {
            GD.PushError($"Character selector model not found: {ModelPaths[_selectedIndex]}");
            _nameLabel.Text = "Modelo indisponível";
            return;
        }

        var model = packed.Instantiate<Node3D>();
        model.Name = _selectedIndex == 0 ? "BarbarianPreview" : "MagePreview";
        model.Scale = Vector3.One * 1.5f;
        _characterRoot.AddChild(model);
        _nameLabel.Text = _selectedIndex == 0 ? "BARBARIAN" : "MAGE";
    }
}
