using Godot;
using ProjetoRpg.Shared.Protocol;

namespace ProjetoRpg.Client.Scripts;

public partial class Bootstrap : Control
{
    public override void _Ready()
    {
        var godotVersion = Engine.GetVersionInfo()["string"].AsString();
        GetNode<Label>("%Status").Text = $"ProjetoRPG — client foundation\nGodot {godotVersion} · protocol {ProtocolConstants.CurrentVersion}\nGameplay is not initialized.";
        GD.Print($"CLIENT_BOOT_OK godot={godotVersion} protocol={ProtocolConstants.CurrentVersion}");
    }
}
