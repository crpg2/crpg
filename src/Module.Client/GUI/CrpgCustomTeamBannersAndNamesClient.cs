using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module;
internal class CrpgCustomTeamBannersAndNamesClient : MissionNetwork
{
    public delegate void BannerNameChangedEventHandler(string attackerBanner, string defenderBanner, string attackerName, string defenderName);
    public event BannerNameChangedEventHandler? BannersChanged;
    public string AttackerBannerCode { get; private set; } = "";
    public string DefenderBannerCode { get; private set; } = "";
    public string AttackerName { get; private set; } = "";
    public string DefenderName { get; private set; } = "";

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UpdateTeamBannersAndNames>(HandleUpdateTeamBannersAndNames);
    }

    private void HandleUpdateTeamBannersAndNames(UpdateTeamBannersAndNames message)
    {
        string cultureTeam1Str = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue();
        var cultureTeam1 = MBObjectManager.Instance?.GetObject<BasicCultureObject>(cultureTeam1Str);
        string cultureNameTeam1 = cultureTeam1?.Name.ToString() ?? "";

        string cultureTeam2Str = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue();
        var cultureTeam2 = MBObjectManager.Instance?.GetObject<BasicCultureObject>(cultureTeam2Str);
        string cultureNameTeam2 = cultureTeam2?.Name.ToString() ?? "";

        AttackerBannerCode = message.AttackerBanner.BannerCode.Length != 0
            ? message.AttackerBanner.BannerCode
            : Mission.Teams.Attacker.Banner.BannerCode;
        DefenderBannerCode = message.DefenderBanner.BannerCode.Length != 0
            ? message.DefenderBanner.BannerCode
            : Mission.Teams.Defender.Banner.BannerCode;
        AttackerName = message.AttackerName.Length != 0
            ? message.AttackerName
            : cultureNameTeam1;
        DefenderName = message.DefenderName.Length != 0
            ? message.DefenderName
            : cultureNameTeam2;
        BannersChanged?.Invoke(AttackerBannerCode, DefenderBannerCode, AttackerName, DefenderName);
    }
}
