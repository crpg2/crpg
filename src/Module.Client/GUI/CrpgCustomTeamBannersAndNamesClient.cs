using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module;
internal class CrpgCustomTeamBannersAndNamesClient : MissionNetwork
{
    public delegate void BannerNameChangedEventHandler(string attackerBanner, string defenderBanner, string attackerName, string defenderName);
    public event BannerNameChangedEventHandler? BannersChanged;
    public string AttackerBannerCode { get; private set; } = string.Empty;
    public string DefenderBannerCode { get; private set; } = string.Empty;
    public string AttackerName { get; private set; } = string.Empty;
    public string DefenderName { get; private set; } = string.Empty;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UpdateTeamBannersAndNames>(HandleUpdateTeamBannersAndNames);
    }

    private void HandleUpdateTeamBannersAndNames(UpdateTeamBannersAndNames message)
    {
        AttackerBannerCode = message.AttackerBanner.BannerCode != string.Empty ? message.AttackerBanner.BannerCode : Mission.Current.Teams.Attacker.Banner.BannerCode;
        DefenderBannerCode = message.DefenderBanner.BannerCode != string.Empty ? message.DefenderBanner.BannerCode : Mission.Current.Teams.Defender.Banner.BannerCode;
        AttackerName = message.AttackerName != string.Empty ? message.AttackerName : MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue())?.Name.ToString() ?? string.Empty;
        DefenderName = message.DefenderName != string.Empty ? message.DefenderName : MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue())?.Name.ToString() ?? string.Empty;
        BannersChanged?.Invoke(AttackerBannerCode, DefenderBannerCode, AttackerName, DefenderName);
    }
}
