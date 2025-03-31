using System.Xml.Serialization;
using Crpg.Module.Api;
using Crpg.Module.Api.Models.Strategus;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using Crpg.Module.Common.AiComponents;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Modes.TrainingGround;
using Crpg.Module.Rewards;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Strategus;

internal class CrpgStrategusServer : MissionMultiplayerGameModeBase
{
    public CrpgStrategusBattle Battle = default!;
    private readonly ICrpgClient _crpgClient;
    private readonly CrpgRewardServer _rewardServer;

    public CrpgStrategusServer(ICrpgClient crpgClient, CrpgRewardServer rewardServer)
    {
        _crpgClient = crpgClient;
        _rewardServer = rewardServer;
        _ = GetStrategusBattle();
    }

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => false;

    private CrpgStrategusSpawningBehavior SpawningBehavior => (CrpgStrategusSpawningBehavior)SpawnComponent.SpawningBehavior;

    public override MultiplayerGameType GetMissionType()
    {
        return MultiplayerGameType.Battle;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        AddTeams();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
    }

    public override bool CheckForWarmupEnd()
    {
        return true;
    }

    public override void OnPeerChangedTeam(NetworkCommunicator networkPeer, Team oldTeam, Team newTeam)
    {
        // force peer onto correct team based on stratbattle

        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (newTeam != Mission.SpectatorTeam)
        {
            var rep = networkPeer.GetComponent<CrpgStrategusMissionRepresentative>();
            if (rep.Side == BattleSide.Defender)
            {
                missionPeer.Team = Mission.DefenderTeam;
            }
            else if (rep.Side == BattleSide.Attacker)
            {
                missionPeer.Team = Mission.AttackerTeam;
            }
            else
            {
                missionPeer.Team = Mission.SpectatorTeam;
            }

            return;
        }
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        // Synchronize health with all clients to make the spectator health bar work.
        agent.UpdateSyncHealthToAllClients(true);
    }

    public void OnPeerSpwaned(Agent agent)
    {
        if (!MissionLobbyComponent.IsInWarmup)
        {
            SendDataToPeers(new CrpgStrategusTicketCountUpdateMessage
            {
                AttackerTickets = SpawningBehavior.Tickets[BattleSideEnum.Attacker],
                DefenderTickets = SpawningBehavior.Tickets[BattleSideEnum.Defender],
            });
        }
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !CanGameModeSystemsTickThisFrame)
        {
            return;
        }

        if (!MissionLobbyComponent.IsInWarmup)
        {
            CheckForEnd();
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent? affectorAgent, AgentState agentState, KillingBlow blow)
    {

    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<CrpgStrategusMissionRepresentative>();
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        if (!MissionLobbyComponent.IsInWarmup)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgStrategusTicketCountUpdateMessage
            {
                AttackerTickets = SpawningBehavior.Tickets[BattleSideEnum.Attacker],
                DefenderTickets = SpawningBehavior.Tickets[BattleSideEnum.Defender],
            });
            GameNetwork.EndModuleEventAsServer();
        }

        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User != null)
        {
            CrpgStrategusFighter? fighter = Battle.GetFighterByUser(crpgPeer.User);
            if (fighter != null)
            {
                networkPeer.GetComponent<CrpgStrategusMissionRepresentative>().Side = fighter.Side;
            }
            else
            {
                CrpgStrategusMercenary? mercenary = Battle.GetMercenaryByUser(crpgPeer.User);
                if (mercenary != null)
                {
                    networkPeer.GetComponent<CrpgStrategusMissionRepresentative>().Side = mercenary.Side;
                }
                else
                {
                    // user is not signed up for this battle!
                }
            }
        }
    }

    /// <summary>Work around the 60 minutes limit of MapTimeLimit.</summary>
    private void SetTimeLimit()
    {
/*        TimerComponent.StartTimerAsServer(MapDuration);
        SendDataToPeers(new CrpgDtvSetTimerMessage
        {
            StartTime = (int)TimerComponent.GetCurrentTimerStartTime().ToSeconds,
            Duration = MapDuration,
        });*/
    }

    private void SendDataToPeers(GameNetworkMessage message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(message);
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void CheckForEnd()
    {
        if (Mission.Current.AttackerTeam.ActiveAgents.Count <= 0 && SpawningBehavior.Tickets[BattleSideEnum.Attacker] <= 0)
        {
            // attackers lose
        }
        else if (Mission.Current.DefenderTeam.ActiveAgents.Count <= 0 && SpawningBehavior.Tickets[BattleSideEnum.Defender] <= 0)
        {
            // defenders lose
        }
    }

    private void AddTeams()
    {
        BasicCultureObject attackerTeamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(attackerTeamCulture.BannerKey, attackerTeamCulture.BackgroundColor1, attackerTeamCulture.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, attackerTeamCulture.BackgroundColor1, attackerTeamCulture.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject defenderTeamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(defenderTeamCulture.BannerKey, defenderTeamCulture.BackgroundColor2, defenderTeamCulture.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, defenderTeamCulture.BackgroundColor2, defenderTeamCulture.ForegroundColor2, bannerTeam2, false, true);
    }

    private async Task GetStrategusBattle()
    {
        var battleRes = (await _crpgClient.GetStrategusBattleAsync(CrpgServerConfiguration.StrategusBattleId)).Data!;
        if (battleRes == null)
        {
            // throw error that battle can't be found
            return;
        }

        Battle = battleRes;

        var fighterRes = await _crpgClient.GetStrategusBattleFightersAsync(Battle.Id);
        if (fighterRes.Data == null)
        {
            // throw error
            return;
        }

        Battle.Fighters = fighterRes.Data;

        var mercenaryRes = await _crpgClient.GetStrategusBattleMercenariesAsync(Battle.Id);
        if (mercenaryRes.Data == null)
        {
            // throw error
            return;
        }

        Battle.Mercenaries = mercenaryRes.Data;
    }
}
