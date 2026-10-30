using Godot;

namespace ProjetoRpg.Client.Scripts;

/// <summary>
/// Reusable, local-only actor for the isolated 3D demo. It has no network or
/// persistence integration; only the player instance accepts mouse destinations.
/// </summary>
public partial class CharacterActor : CharacterBody3D
{
    [Export] public PackedScene? ModelScene { get; set; }
    [Export] public bool PlayerControlled { get; set; } = true;
    [Export] public float WalkSpeed { get; set; } = 2.8f;
    [Export] public float RunSpeed { get; set; } = 4.2f;
    [Export] public float RunDistanceThreshold { get; set; } = 5.0f;
    [Export] public float VisualScale { get; set; } = 1.0f;
    [Export] public float VisualYawCorrectionDegrees { get; set; } = 0.0f;
    [Export] public NodePath TargetMarkerPath { get; set; } = new("../TargetMarker");

    private AnimationPlayer? _animationPlayer;
    private Node3D? _targetMarker;
    private Vector3 _destination;
    private bool _hasDestination;
    private StringName _currentAnimation = new(" ");

    public override void _Ready()
    {
        if (ModelScene is null)
        {
            GD.PushError($"{Name}: required visual model is missing. Assign ModelScene to a valid GLB/PackedScene and verify its res:// file path exists; CharacterDemo uses Barbarian.glb for Player and Mage.glb for NPC.");
            return;
        }

        Node instantiatedModel = ModelScene.Instantiate();
        if (instantiatedModel is not Node3D model)
        {
            string modelPath = string.IsNullOrWhiteSpace(ModelScene.ResourcePath) ? "<unknown resource path>" : ModelScene.ResourcePath;
            GD.PushError($"{Name}: required visual model '{modelPath}' did not instantiate with a Node3D root. Check that the GLB imports correctly.");
            instantiatedModel.Free();
            return;
        }

        model.Name = "VisualModel";
        model.Scale = Vector3.One * VisualScale;
        // Keep any yaw correction on the visual import only. The CharacterBody3D
        // root continues to own movement, LookAt, and the collision shape.
        model.RotateY(Mathf.DegToRad(VisualYawCorrectionDegrees));
        AddChild(model);
        _animationPlayer = FindAnimationPlayer(model);
        _targetMarker = GetNodeOrNull<Node3D>(TargetMarkerPath);

        if (_animationPlayer is null)
        {
            GD.PushWarning($"{Name}: imported model has no AnimationPlayer; movement remains available without animation.");
        }
        else
        {
            PlayAnimation("Idle");
        }

        if (!PlayerControlled && _targetMarker is not null)
        {
            // The marker belongs to the player demo flow; the NPC never owns it.
            _targetMarker = null;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!PlayerControlled || @event is not InputEventMouseButton mouseButton)
        {
            return;
        }

        if (mouseButton.ButtonIndex != MouseButton.Left || !mouseButton.Pressed)
        {
            return;
        }

        Camera3D? camera = GetViewport().GetCamera3D();
        if (camera is null)
        {
            return;
        }

        Vector3 rayOrigin = camera.ProjectRayOrigin(mouseButton.Position);
        Vector3 rayDirection = camera.ProjectRayNormal(mouseButton.Position);
        if (Mathf.Abs(rayDirection.Y) < 0.0001f)
        {
            return;
        }

        // The demo arena is a flat y=0 plane; no network or physics query is
        // needed to produce a local click-to-move destination.
        float distance = -rayOrigin.Y / rayDirection.Y;
        if (distance < 0.0f)
        {
            return;
        }

        Vector3 hit = rayOrigin + rayDirection * distance;
        if (Mathf.Abs(hit.X) > 11.5f || Mathf.Abs(hit.Z) > 11.5f)
        {
            return;
        }

        _destination = new Vector3(hit.X, 0.0f, hit.Z);
        _hasDestination = true;
        if (_targetMarker is not null)
        {
            _targetMarker.GlobalPosition = _destination + Vector3.Up * 0.13f;
            _targetMarker.Visible = true;
        }
    }

    public void ApplyAuthoritativeState(Vector3 position, Vector3 velocity)
    {
        GlobalPosition = position;
        Velocity = velocity;
        var planarVelocity = new Vector3(velocity.X, 0.0f, velocity.Z);
        if (planarVelocity.LengthSquared() > 0.0025f)
        {
            LookAt(GlobalPosition + planarVelocity.Normalized(), Vector3.Up);
            PlayAnimation("Walking_A");
        }
        else
        {
            PlayAnimation("Idle");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!PlayerControlled)
        {
            Velocity = Vector3.Zero;
            return;
        }
        if (!_hasDestination)
        {
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        Vector3 offset = _destination - GlobalPosition;
        offset.Y = 0.0f;
        float distance = offset.Length();
        if (distance <= 0.08f)
        {
            _hasDestination = false;
            Velocity = Vector3.Zero;
            MoveAndSlide();
            if (_targetMarker is not null)
            {
                _targetMarker.Visible = false;
            }

            PlayAnimation("Idle");
            return;
        }

        Vector3 direction = offset / distance;
        float speed = distance >= RunDistanceThreshold ? RunSpeed : WalkSpeed;
        Velocity = new Vector3(direction.X * speed, 0.0f, direction.Z * speed);
        LookAt(GlobalPosition + direction, Vector3.Up);
        MoveAndSlide();
        PlayAnimation(distance >= RunDistanceThreshold ? "Running_A" : "Walking_A");
    }

    private void PlayAnimation(StringName animationName)
    {
        if (_animationPlayer is null || _currentAnimation == animationName || !_animationPlayer.HasAnimation(animationName))
        {
            return;
        }

        _currentAnimation = animationName;
        _animationPlayer.Play(animationName);
    }

    private static AnimationPlayer? FindAnimationPlayer(Node node)
    {
        if (node is AnimationPlayer animationPlayer)
        {
            return animationPlayer;
        }

        foreach (Node child in node.GetChildren())
        {
            AnimationPlayer? found = FindAnimationPlayer(child);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
