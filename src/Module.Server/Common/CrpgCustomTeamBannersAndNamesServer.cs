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
    private readonly Dictionary<int, (int count, CrpgClan clan)> _attackerClanNumber;
    private readonly Dictionary<int, (int count, CrpgClan clan)> _defenderClanNumber;
    private int _previousAttackerClanId;
    private int _previousDefenderClanId;

    internal CrpgCustomTeamBannersAndNamesServer(MultiplayerRoundController? roundController)
    {
        _roundController = roundController;
        _attackerClanNumber = new Dictionary<int, (int count, CrpgClan clan)>();
        _defenderClanNumber = new Dictionary<int, (int count, CrpgClan clan)>();
    }

    public Banner AttackerBanner { get; private set; } = new(string.Empty);
    public Banner DefenderBanner { get; private set; } = new(string.Empty);
    public string AttackerName { get; private set; } = string.Empty;
    public string DefenderName { get; private set; } = string.Empty;
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public void UpdateBanner()
    {
        var attackerMaxClan = Extensions.MaxBy(_attackerClanNumber.DefaultIfEmpty(), c => c.Value.count).Value.clan;
        var defenderMaxClan = Extensions.MaxBy(_defenderClanNumber.DefaultIfEmpty(), c => c.Value.count).Value.clan;
        int attackerMaxClanId = attackerMaxClan?.Id ?? -1;
        int defenderMaxClanId = defenderMaxClan?.Id ?? -1;
        if (attackerMaxClanId == _previousAttackerClanId && defenderMaxClanId == _previousDefenderClanId)
        {
            return;
        }

        string attackerTeamName = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).Name.ToString();
        string defenderTeamName = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).Name.ToString();

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
            DefenderBanner = new(defenderBanner);
        }

        string attackerName = MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue())?.Name.ToString() ?? string.Empty;
        string defenderName = MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue()).Name.ToString() ?? string.Empty;
        if (attackerName != string.Empty)
        {
            AttackerName = attackerName;
        }

        if (defenderName != string.Empty)
        {
            DefenderName = defenderName;
        }
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted += InitializeClanDictionaries;
            _roundController.OnRoundStarted += UpdateBanner;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted -= InitializeClanDictionaries;
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

    private void InitializeClanDictionaries()
    {
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
            Dictionary<int, (int count, CrpgClan clan)> clanNumber = missionPeer.Team.Side == BattleSideEnum.Attacker
                ? _attackerClanNumber
                : _defenderClanNumber;

            if (clanNumber.TryGetValue(peerClanId, out var clan))
            {
                clan.count++;
                clanNumber[peerClanId] = clan;
            }
            else
            {
                clanNumber.Add(peerClanId, (1, crpgPeer.Clan));
            }
        }
    }
}
