using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module;

internal class CrpgCustomTeamBannersAndNamesServer : MissionNetwork
{
    private readonly MultiplayerRoundController? _roundController;
    private int _previousAttackerClanId;
    private int _previousDefenderClanId;

    internal CrpgCustomTeamBannersAndNamesServer(MultiplayerRoundController? roundController)
    {
        _roundController = roundController;
    }

    public Banner AttackerBanner { get; private set; } = new("");
    public Banner DefenderBanner { get; private set; } = new("");
    public string AttackerName { get; private set; } = "";
    public string DefenderName { get; private set; } = "";
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public void UpdateBanner()
    {
        Dictionary<int, (int count, CrpgClan clan)> attackerClans = [];
        Dictionary<int, (int count, CrpgClan clan)> defenderClans = [];

        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer?.GetComponent<CrpgPeer>();
            var missionPeer = networkPeer?.GetComponent<MissionPeer>();

            if (missionPeer == null || crpgPeer?.User == null || crpgPeer.Clan == null || missionPeer.Team == null)
            {
                continue;
            }

            if (missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            int peerClanId = crpgPeer.Clan!.Id;
            var clanCounts = missionPeer.Team.Side == BattleSideEnum.Attacker
                ? attackerClans
                : defenderClans;

            if (clanCounts.TryGetValue(peerClanId, out var entry))
            {
                entry.count++;
                clanCounts[peerClanId] = entry;
            }
            else
            {
                clanCounts.Add(peerClanId, (1, crpgPeer.Clan));
            }
        }

        var attackerMaxClan = Extensions.MaxBy(attackerClans.DefaultIfEmpty(), c => c.Value.count).Value.clan;
        var defenderMaxClan = Extensions.MaxBy(defenderClans.DefaultIfEmpty(), c => c.Value.count).Value.clan;
        int attackerMaxClanId = attackerMaxClan?.Id ?? -1;
        int defenderMaxClanId = defenderMaxClan?.Id ?? -1;
        if (attackerMaxClanId == _previousAttackerClanId && defenderMaxClanId == _previousDefenderClanId)
        {
            return;
        }

        string attackerTeamName = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue()).Name.ToString();
        string defenderTeamName = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue()).Name.ToString();

        if (attackerMaxClan != null)
        {
            AttackerBanner = new Banner(attackerMaxClan.BannerKey);
            attackerTeamName = attackerMaxClan.Name;
        }

        if (defenderMaxClan != null)
        {
            DefenderBanner = new Banner(defenderMaxClan.BannerKey);
            defenderTeamName = defenderMaxClan.Name;
        }

        _previousAttackerClanId = attackerMaxClanId;
        _previousDefenderClanId = defenderMaxClanId;
        AttackerName = attackerTeamName;
        DefenderName = defenderTeamName;
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateTeamBannersAndNames
        {
            AttackerBanner = AttackerBanner,
            DefenderBanner = DefenderBanner,
            AttackerName = attackerTeamName,
            DefenderName = defenderTeamName,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    public override void OnAddTeam(Team team)
    {
        base.OnAddTeam(team);
        Banner? attackerBanner = Mission.Current?.Teams.Attacker?.Banner;
        Banner? defenderBanner = Mission.Current?.Teams.Defender?.Banner;

        if (attackerBanner != null)
        {
            AttackerBanner = new Banner(attackerBanner);
        }

        if (defenderBanner != null)
        {
            DefenderBanner = new Banner(defenderBanner);
        }

        string attackerName = MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue())?.Name.ToString() ?? "";
        string defenderName = MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue()).Name.ToString() ?? "";
        if (attackerName != "")
        {
            AttackerName = attackerName;
        }

        if (defenderName != "")
        {
            DefenderName = defenderName;
        }
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted += UpdateBanner;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted -= UpdateBanner;
        }
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new UpdateTeamBannersAndNames
        {
            AttackerBanner = AttackerBanner,
            DefenderBanner = DefenderBanner,
            AttackerName = AttackerName,
            DefenderName = DefenderName,
        });
        GameNetwork.EndModuleEventAsServer();
    }
}
