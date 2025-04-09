using System.ComponentModel;
using Crpg.Module.Api;
using Crpg.Module.Api.Models.Strategus;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Modes.TrainingGround;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Strategus;

internal class CrpgStrategusServer : MissionMultiplayerGameModeBase
{
    public CrpgStrategusBattle Battle = default!;
    public bool IsInWarmup { get; private set; } = false;
    private bool _battleStarted = false;
    private readonly ICrpgClient _crpgClient;
    private readonly CrpgRewardServer _rewardServer;
    private MissionTimer? _startTimer;

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
        return MultiplayerGameType.Siege;
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

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        // Synchronize health with all clients to make the spectator health bar work.
        agent.UpdateSyncHealthToAllClients(true);
    }

    public void OnPeerSpwaned(Agent agent)
    {
        if (!IsInWarmup)
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

        if (!_battleStarted && !IsInWarmup)
        {
            CreateWarmup();
            ConfigureBattle();
        }

        if (_battleStarted)
        {
            CheckForEnd();
        }
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<CrpgStrategusMissionRepresentative>();
    }

    protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        if (!IsInWarmup)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgStrategusTicketCountUpdateMessage
            {
                AttackerTickets = SpawningBehavior.Tickets[BattleSideEnum.Attacker],
                DefenderTickets = SpawningBehavior.Tickets[BattleSideEnum.Defender],
            });
            GameNetwork.EndModuleEventAsServer();
        }
        else
        {
            SendDataToPeers(new CrpgSetGameTimerMessage
            {
                StartTime = (int)TimerComponent.GetCurrentTimerStartTime().ToSeconds,
                Duration = (int)_startTimer!.GetTimerDuration(),
            });
        }

        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User != null)
        {
            var rep = networkPeer.GetComponent<CrpgStrategusMissionRepresentative>();
            CrpgStrategusFighter? fighter = Battle.GetFighterByUser(crpgPeer.User);
            if (fighter != null)
            {
                rep.Side = fighter.Side;
            }
            else
            {
                CrpgStrategusMercenary? mercenary = Battle.GetMercenaryByUser(crpgPeer.User);
                if (mercenary != null)
                {
                    rep.Side = mercenary.Side;
                }
                else
                {
                    // user is not signed up for this battle!
                }
            }

            MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();

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
        }

        return;
    }

    /// <summary>Work around the 60 minutes limit of MapTimeLimit.</summary>
    private void SetTimeLimit()
    {
/*        TimerComponent.StartTimerAsServer(MapDuration);
        SendDataToPeers(new CrpgSetGameTimerMessage
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
        bool isEnd = false;
        if (Mission.Current.AttackerTeam.ActiveAgents.Count <= 0 && SpawningBehavior.Tickets[BattleSideEnum.Attacker] <= 0)
        {
            _ = SendBattleUpdate(winner: BattleSide.Defender);
            isEnd = true;
        }
        else if (Mission.Current.DefenderTeam.ActiveAgents.Count <= 0 && SpawningBehavior.Tickets[BattleSideEnum.Defender] <= 0)
        {
            _ = SendBattleUpdate(winner: BattleSide.Attacker);
            isEnd = true;
        }

        if (isEnd)
        {
            MissionLobbyComponent.SetStateEndingAsServer();
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

    private async Task SendBattleUpdate(BattleSide? winner)
    {
        CrpgStrategusBattleUpdate crpgStrategusBattleUpdate = new()
        {
            Instance = CrpgServerConfiguration.Instance,
            BattleId = Battle.Id,
            AttackerTickets = SpawningBehavior.Tickets[BattleSideEnum.Attacker],
            DefenderTickets = SpawningBehavior.Tickets[BattleSideEnum.Defender],
            Winner = winner,
        };

        var res = await _crpgClient.UpdateStrategusBattleAsync(Battle.Id, new() { Update = crpgStrategusBattleUpdate });
        if (res.Errors != null)
        {
            // sad times
        }
    }

    private void ConfigureBattle()
    {
        SpawningBehavior.SetTickets(BattleSideEnum.Attacker, Battle.AttackerTotalTroops);
        SpawningBehavior.SetTickets(BattleSideEnum.Defender, Battle.DefenderTotalTroops);
    }

    private void CreateWarmup()
    {
        int secondsUntilBattle = (int)(Battle.ScheduledFor! - DateTime.UtcNow).Value.TotalSeconds;
        _startTimer = new MissionTimer(secondsUntilBattle);
        TimerComponent.StartTimerAsServer(secondsUntilBattle);
        SendDataToPeers(new CrpgSetGameTimerMessage
        {
            StartTime = (int)TimerComponent.GetCurrentTimerStartTime().ToSeconds,
            Duration = (int)_startTimer.GetTimerDuration(),
        });
        IsInWarmup = true;
    }

    private async Task GetStrategusBattle()
    {
        var battleRes = (await _crpgClient.GetStrategusBattleAsync(CrpgServerConfiguration.StrategusBattleId)).Data!;
        if (battleRes.Phase != BattlePhase.Scheduled && battleRes.Phase != BattlePhase.Live)
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
