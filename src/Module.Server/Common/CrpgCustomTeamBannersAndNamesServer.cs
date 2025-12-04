using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Modes.Conquest;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module;
internal class CrpgCustomTeamBannersAndNamesServer : MissionNetwork
{
    private Dictionary<int, (int count, CrpgClan clan)> attackerClanNumber = new();
    private Dictionary<int, (int count, CrpgClan clan)> defenderClanNumber = new();
    private int previousAttackerClanId;
    private int previousDefenderClanId;
    public Banner AttackerBanner { get; private set; } = new(string.Empty);
    public Banner DefenderBanner { get; private set; } = new(string.Empty);
    public string AttackerName { get; private set; } = string.Empty;
    public string DefenderName { get; private set; } = string.Empty;
    private MultiplayerRoundController? _roundController;
    internal CrpgCustomTeamBannersAndNamesServer(MultiplayerRoundController? roundController)
    {
        _roundController = roundController;
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    public void UpdateBanner()
    {

        var attackerMaxClan = attackerClanNumber.DefaultIfEmpty().MaxBy(c => c.Value.count).Value.clan;
        var defenderMaxClan = defenderClanNumber.DefaultIfEmpty().MaxBy(c => c.Value.count).Value.clan;
        int attackerMaxClanId = attackerMaxClan?.Id ?? -1;
        int defenderMaxClanId = defenderMaxClan?.Id ?? -1;
        if (attackerMaxClanId == previousAttackerClanId && defenderMaxClanId == previousDefenderClanId)
        {
            return;
        }

        string attackerTeamName = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).Name.ToString();
        string defenderTeamName = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).Name.ToString();
        Banner? attackerBanner = Mission.Current?.Teams.Attacker?.Banner;
        Banner? defenderBanner = Mission.Current?.Teams.Defender?.Banner;

        if (attackerMaxClan != null)
        {
            AttackerBanner = new(new Banner(attackerMaxClan.BannerKey));

            attackerTeamName = attackerMaxClan.Name;
        }

        if (defenderMaxClan != null)
        {
            DefenderBanner = new(new Banner(defenderMaxClan.BannerKey));
            defenderTeamName = defenderMaxClan.Name;
        }

        previousAttackerClanId = attackerMaxClanId;
        previousDefenderClanId = defenderMaxClanId;
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

        if(attackerBanner != null)
        {
            AttackerBanner = new(attackerBanner);
        }

        if (defenderBanner != null)
        {
            DefenderBanner = new(defenderBanner);
        }

        var attackerName = MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions))?.Name.ToString() ?? string.Empty;
        var defenderName = MBObjectManager.Instance?.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions)).Name.ToString() ?? string.Empty;
        if (attackerName != string.Empty)
        {
            AttackerName = attackerName;
        }

        if (defenderName != string.Empty)
        {
            DefenderName = defenderName;
        }
    }
    private void InitializeClanDictionaries()
    {
        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer?.GetComponent<CrpgPeer>();
            var missionPeer = networkPeer?.GetComponent<MissionPeer>();

            if (missionPeer == null || crpgPeer?.User == null || crpgPeer?.Clan == null || missionPeer.Team == null)
            {
                continue;
            }

            if (missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            Dictionary<int, (int count, CrpgClan clan)> ClanNumber;
            int peerClanId = crpgPeer!.Clan!.Id;
            if (missionPeer.Team.Side == BattleSideEnum.Attacker)
            {
                ClanNumber = attackerClanNumber;
            }
            else
            {
                ClanNumber = defenderClanNumber;
            }

            if (ClanNumber.TryGetValue(peerClanId, out var clan))
            {
                clan.count++;
                ClanNumber[peerClanId] = clan;
            }
            else
            {
                ClanNumber.Add(peerClanId, (1, crpgPeer.Clan));
            }
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
}
