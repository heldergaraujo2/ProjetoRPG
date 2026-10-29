using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>Soft follow camera used only by the isolated 3D character demo.</summary>
public partial class DemoCamera : Camera3D
{
    [Export] public NodePath TargetPath { get; set; } = new("../Player");
    [Export] public Vector3 FollowOffset { get; set; } = new(0.0f, 8.5f, 11.0f);
    [Export] public float FollowSharpness { get; set; } = 5.0f;

    private Node3D? _target;

    public override void _Ready()
    {
        _target = GetNodeOrNull<Node3D>(TargetPath);
        if (_target is null)
        {
            GD.PushWarning("Demo camera could not find its target; it will remain at its scene position.");
            return;
        }

        Current = true;
        GlobalPosition = _target.GlobalPosition + FollowOffset;
        LookAt(_target.GlobalPosition + Vector3.Up * 0.8f, Vector3.Up);
    }

    public override void _Process(double delta)
    {
        if (_target is null)
        {
            return;
        }

        float blend = 1.0f - Mathf.Exp(-FollowSharpness * (float)delta);
        Vector3 desiredPosition = _target.GlobalPosition + FollowOffset;
        GlobalPosition = GlobalPosition.Lerp(desiredPosition, blend);
        LookAt(_target.GlobalPosition + Vector3.Up * 0.8f, Vector3.Up);
    }
}
