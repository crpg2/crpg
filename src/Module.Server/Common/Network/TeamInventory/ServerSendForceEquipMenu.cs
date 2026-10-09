using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class ServerSendForceEquipMenu : GameNetworkMessage
{
    public bool Show { get; set; }
    public bool ShowReadyButton { get; set; } = false;
    public string Msg { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteBoolToPacket(Show);
        WriteBoolToPacket(ShowReadyButton);
        WriteStringToPacket(Msg);
    }

    protected override bool OnRead()
    {
        bool valid = true;
        Show = ReadBoolFromPacket(ref valid);
        ShowReadyButton = ReadBoolFromPacket(ref valid);
        Msg = ReadStringFromPacket(ref valid);
        return valid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;
    protected override string OnGetLogFormat() => $"Server force {(Show ? "Show" : "close")} showbutton:{ShowReadyButton} msg:{Msg}";
}
