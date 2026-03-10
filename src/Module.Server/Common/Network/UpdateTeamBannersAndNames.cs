using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateTeamBannersAndNames : GameNetworkMessage
{
    public Banner AttackerBanner { get; set; } = new(string.Empty);
    public Banner DefenderBanner { get; set; } = new(string.Empty);
    public string AttackerName { get; set; } = string.Empty;
    public string DefenderName { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteBannerCodeToPacket(AttackerBanner.BannerCode);
        WriteBannerCodeToPacket(DefenderBanner.BannerCode);
        WriteStringToPacket(AttackerName);
        WriteStringToPacket(DefenderName);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        AttackerBanner = new(ReadBannerCodeFromPacket(ref bufferReadValid));
        DefenderBanner = new(ReadBannerCodeFromPacket(ref bufferReadValid));
        AttackerName = ReadStringFromPacket(ref bufferReadValid);
        DefenderName = ReadStringFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update Team Banner And Names";
    }
}
