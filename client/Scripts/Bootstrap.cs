using Godot;
using ProjetoRpg.Shared.Protocol;

namespace ProjetoRpg.Client.Scripts;

public partial class Bootstrap : Control
{
    public override void _Ready()
    {
        var godotVersion = Engine.GetVersionInfo()["string"].AsString();
        GetNode<Label>("%Status").Visible = false;

        var accountGateway = new AccountGateway();
        accountGateway.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(accountGateway);

        GD.Print($"CLIENT_BOOT_OK godot={godotVersion} protocol={ProtocolConstants.CurrentVersion}");
        GD.Print("CLIENT_ACCOUNT_FLOW_READY: server-backed registration, login, and persistent character list/create.");
    }
}
