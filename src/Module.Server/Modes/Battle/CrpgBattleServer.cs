﻿using System.Reflection;
using Crpg.Module.Modes.Battle.FlagSystems;
using Crpg.Module.Modes.Skirmish;
using Crpg.Module.Rewards;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.MountAndBlade.MissionLobbyComponent;

namespace Crpg.Module.Modes.Battle;

internal class CrpgBattleServer : MissionMultiplayerGameModeBase
{
    private const float BattleMoraleGainOnTick = 0.01f;
    private const float BattleMoraleGainMultiplierLastFlag = 2f;
    private const float SkirmishMoraleGainOnTick = 0.00125f;
    private const float SkirmishMoraleGainMultiplierLastFlag = 2f;
    private const int LossMultiplier = 2;

    private readonly CrpgBattleClient _battleClient;
    private readonly MultiplayerGameType _gametype;
    private readonly CrpgRewardServer _rewardServer;
    private AbstractFlagSystem _flagSystem = default!;

    /// <summary>A number between -1.0 and 1.0. Less than 0 means the defenders are winning. Greater than 0 for attackers.</summary>
    private float _morale;
    private int _attackersSpawned;
    private int _defendersSpawned;
    private bool _hasSpawnDelayEnded = false;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgBattleServer(CrpgBattleClient battleClient, MultiplayerGameType gametype,
        CrpgRewardServer rewardServer)
    {
        _battleClient = battleClient;
        _gametype = gametype;
        _rewardServer = rewardServer;
        typeof(TaleWorlds.MountAndBlade.CompressionMission)
            .GetField(nameof(TaleWorlds.MountAndBlade.CompressionMission.AgentOffsetCompressionInfo), BindingFlags.Public | BindingFlags.Static)?
            .SetValue(null, new CompressionInfo.Integer(0, 16));
    }

    public override MultiplayerGameType GetMissionType()
    {
        return _gametype;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        MissionPeer.OnPreTeamChanged += this.OnPreTeamChanged;
        RoundController.OnPreRoundEnding += OnPreRoundEnding;

        AddTeams();
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (_gametype == MultiplayerGameType.Skirmish || !affectedAgent.IsHuman)
        {
            return;
        }

        ((CrpgBattleFlagSystem)_flagSystem).CheckForDeadPlayerFlagSpawnThreshold(_attackersSpawned, _defendersSpawned);
    }

    protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
    {
        if (this.RoundController.IsRoundInProgress && MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) > 0)
        {
            this.MakePlayerFormationCharge(networkPeer);
        }
    }



    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _flagSystem = _gametype switch { 
            MultiplayerGameType.Skirmish => new CrpgSkirmishFlagSystem(Mission, NotificationsComponent, _battleClient),
            _ => new CrpgBattleFlagSystem(Mission, NotificationsComponent, _battleClient),
            };

        _flagSystem.ResetFlags();
        _morale = 0f;
        // TODO: SetTeamColorsWithAllSynched
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        MissionPeer.OnPreTeamChanged -= this.OnPreTeamChanged;
        base.OnRemoveBehavior();
    }

    public override void OnClearScene()
    {
        _flagSystem.ResetFlags();
        _morale = 0.0f;
        _flagSystem.SetCheckFlagRemovalTimer(null);
        _flagSystem.SetHasFlagCountChanged(false);
        _attackersSpawned = 0;
        _defendersSpawned = 0;
        _hasSpawnDelayEnded = false;
    }

    public override bool CheckForWarmupEnd()
    {
        int playersInTeam = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
            {
                playersInTeam += 1;
            }
        }

        return playersInTeam >= MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue();
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !RoundController.IsRoundInProgress
            || !CanGameModeSystemsTickThisFrame
            || _flagSystem.HasNoFlags()) // Protection against scene with no flags.
        {
            return;
        }

        if (SpawnComponent.SpawningBehavior is CrpgBattleSpawningBehavior s && s.SpawnDelayEnded() && !_hasSpawnDelayEnded)
        {
            _hasSpawnDelayEnded = true;
            _attackersSpawned = Mission.AttackerTeam.ActiveAgents.Count;
            _defendersSpawned = Mission.DefenderTeam.ActiveAgents.Count;
        }

        CheckForPlayersSpawningAsBots();

        _flagSystem.CheckForManipulationOfFlags();
        CheckMorales();
        _flagSystem.TickFlags();
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        if (!_flagSystem.HasNoFlags() && Math.Abs(_morale) >= 1.0)
        {
            var lastFlag = _flagSystem.GetLastFlag();
            var lastFlagOwner = _flagSystem.GetFlagOwner(lastFlag);
            if (lastFlagOwner == null)
            {
                return true;
            }

            var winningSide = _morale > 0.0f
                ? BattleSideEnum.Attacker
                : BattleSideEnum.Defender;
            return lastFlagOwner.Side == winningSide
                   && lastFlag.IsFullyRaised
                   && _flagSystem.GetNumberOfAttackersAroundFlag(lastFlag) == 0;
        }

        if (SpawnComponent.SpawningBehavior is CrpgBattleSpawningBehavior s && !s.SpawnDelayEnded())
        {
            return false;
        }

        bool defenderTeamDepleted = Mission.DefenderTeam.ActiveAgents.Count == 0;
        bool attackerTeamDepleted = Mission.AttackerTeam.ActiveAgents.Count == 0;
        if (!(_gametype == MultiplayerGameType.Skirmish))
        {
            return defenderTeamDepleted || attackerTeamDepleted;
        }

        bool defenderCanSpawn = false;
        bool attackerCanSpawn = false;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (missionPeer?.Team == null || missionPeer.SpawnCountThisRound >= CrpgSkirmishSpawningBehavior.MaxSpawns)
            {
                continue;
            }

            if (missionPeer.Team.Side == BattleSideEnum.Defender)
            {
                defenderCanSpawn = true;
            }
            else if (missionPeer.Team.Side == BattleSideEnum.Attacker)
            {
                attackerCanSpawn = true;
            }
        }

        return (defenderTeamDepleted && !defenderCanSpawn) || (attackerTeamDepleted && !attackerCanSpawn);
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        // Synchronize health with all clients to make the spectator health bar work.
        agent.UpdateSyncHealthToAllClients(true);
    }

    public override bool CheckIfOvertime()
    {
        if (!_flagSystem.HasFlagCountChanged())
        {
            return false;
        }

        var lastFlag = _flagSystem.GetLastFlag();
        var owner = _flagSystem.GetFlagOwner(lastFlag);
        if (owner == null)
        {
            return false;
        }

        int moraleOwnerSide = owner.Side == BattleSideEnum.Defender ? -1 : 1;
        return moraleOwnerSide * _morale < 0.0 || _flagSystem.GetNumberOfAttackersAroundFlag(lastFlag) > 0;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndModuleEventAsServer();
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<FlagDominationMissionRepresentative>();
    }

    private void AddTeams()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, bannerTeam1, false, true);
        for (int i = 0; i < 199; i++)
        {
            Formation f = new(Mission.Current.Teams.Attacker, 1);
            Mission.Teams.Attacker.FormationsIncludingEmpty.Add(f);
        }

        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, bannerTeam2, false, true);
        for (int i = 0; i < 199; i++)
        {
            Formation f = new(Mission.Current.Teams.Defender, 1);
            Mission.Teams.Defender.FormationsIncludingEmpty.Add(f);
        }
    }

    private void CheckMorales()
    {
        float moraleGain = GetMoraleGain();
        if (moraleGain == 0)
        {
            return;
        }

        _morale += moraleGain;
        _morale = MBMath.ClampFloat(_morale, -1f, 1f);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        _battleClient.ChangeMorale(_morale);
    }

    private float GetMoraleGain()
    {
        FlagCapturePoint[] capturedFlags = _flagSystem.GetCapturedFlags();
        int teamFlagsDelta = capturedFlags.Count(flag => _flagSystem.GetFlagOwner(flag)!.Side == BattleSideEnum.Attacker)
                             - capturedFlags.Count(flag => _flagSystem.GetFlagOwner(flag)!.Side == BattleSideEnum.Defender);
        if (teamFlagsDelta == 0)
        {
            return 0f;
        }

        float moraleGainOnTick = _gametype == MultiplayerGameType.Skirmish ? SkirmishMoraleGainOnTick : BattleMoraleGainOnTick;
        float moraleGainMultiplierLastFlag = _gametype == MultiplayerGameType.Skirmish ? SkirmishMoraleGainMultiplierLastFlag : BattleMoraleGainMultiplierLastFlag;

        float moraleMultiplier = moraleGainOnTick * Math.Abs(teamFlagsDelta);
        float moraleGain = teamFlagsDelta <= 0
            ? MBMath.ClampFloat(-1 - _morale, -2f, -1f) * moraleMultiplier
            : MBMath.ClampFloat(1 - _morale, 1f, 2f) * moraleMultiplier;
        if (_flagSystem.HasFlagCountChanged()) // For the last flag, the morale is moving faster.
        {
            moraleGain *= moraleGainMultiplierLastFlag;
        }

        return moraleGain;
    }

    private void OnPreRoundEnding()
    {
        foreach (var flag in _flagSystem.GetAllFlags())
        {
            if (!flag.IsDeactivated)
            {
                flag.SetMoveNone();
            }
        }

        bool timedOut = RoundController.RemainingRoundTime <= 0.0 && !CheckIfOvertime();

        BattleSideEnum winnerSide = BattleSideEnum.None;
        for (BattleSideEnum side = BattleSideEnum.Defender; side < BattleSideEnum.NumSides; side += 1)
        {
            int moraleSide = side == BattleSideEnum.Defender ? -1 : 1;
            if ((timedOut && moraleSide * _morale > 0.0f) || (!timedOut && moraleSide * _morale >= 1.0f))
            {
                winnerSide = side;
                break;
            }
        }

        CaptureTheFlagCaptureResultEnum roundResult;
        if (winnerSide != BattleSideEnum.None)
        {
            roundResult = winnerSide == BattleSideEnum.Defender
                ? CaptureTheFlagCaptureResultEnum.DefendersWin
                : CaptureTheFlagCaptureResultEnum.AttackersWin;
            RoundController.RoundWinner = winnerSide;
            RoundController.RoundEndReason = timedOut ? RoundEndReason.RoundTimeEnded : RoundEndReason.GameModeSpecificEnded;
        }
        else
        {
            bool defenderTeamAlive = Mission.DefenderTeam.ActiveAgents.Count > 0;
            bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
            if (defenderTeamAlive && attackerTeamAlive)
            {
                if (_morale > 0.0f)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                    RoundController.RoundWinner = BattleSideEnum.Attacker;
                }
                else if (_morale < 0.0)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                    RoundController.RoundWinner = BattleSideEnum.Defender;
                }
                else
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                    RoundController.RoundWinner = BattleSideEnum.None;
                }

                RoundController.RoundEndReason = RoundEndReason.RoundTimeEnded;
            }
            else if (defenderTeamAlive)
            {
                roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                RoundController.RoundWinner = BattleSideEnum.Defender;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else if (attackerTeamAlive)
            {
                roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                RoundController.RoundWinner = BattleSideEnum.Attacker;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else // Everyone ded
            {
                roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                RoundController.RoundWinner = BattleSideEnum.None;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
        }

        Debug.Print($"Team {RoundController.RoundWinner} won on map {Mission.SceneName} with {GameNetwork.NetworkPeers.Count()} players");
        CheerForRoundEnd(roundResult);

        float roundDuration = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - RoundController.RemainingRoundTime;
        var roundWinner = RoundController.RoundWinner;
        _ = _rewardServer.UpdateCrpgUsersAsync(
            durationRewarded: roundDuration,
            defenderMultiplierGain: roundWinner == BattleSideEnum.Defender ? 1 : -LossMultiplier,
            attackerMultiplierGain: roundWinner == BattleSideEnum.Attacker ? 1 : -LossMultiplier,
            valourTeamSide: roundWinner.GetOppositeSide());
    }

    private void CheerForRoundEnd(CaptureTheFlagCaptureResultEnum roundResult)
    {
        AgentVictoryLogic missionBehavior = Mission.GetMissionBehavior<AgentVictoryLogic>();
        if (roundResult == CaptureTheFlagCaptureResultEnum.AttackersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Attacker);
        }
        else if (roundResult == CaptureTheFlagCaptureResultEnum.DefendersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Defender);
        }
    }
    private void CheckForPlayersSpawningAsBots()
    {
        foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
        {
            if (networkCommunicator.IsSynchronized)
            {
                MissionPeer component = networkCommunicator.GetComponent<MissionPeer>();
                if (component != null && component.ControlledAgent == null && component.Team != null && component.ControlledFormation != null && component.SpawnCountThisRound > 0)
                {
                    if (!component.HasSpawnTimerExpired && component.SpawnTimer.Check(base.Mission.CurrentTime))
                    {
                        component.HasSpawnTimerExpired = true;
                    }

                    if (component.HasSpawnTimerExpired)
                    {
                        if (component.ControlledFormation.HasUnitsWithCondition((Agent agent) => agent.IsActive() && agent.IsAIControlled))
                        {
                            Agent? newAgent = null;
                            Agent followingAgent = component.FollowedAgent;
                            if (followingAgent != null && followingAgent.IsActive() && followingAgent.IsAIControlled && component.ControlledFormation.HasUnitsWithCondition((Agent agent) => agent == followingAgent))
                            {
                                newAgent = followingAgent;
                            }
                            else
                            {
                                float maxHealth = 0f;
                                component.ControlledFormation.ApplyActionOnEachUnit(delegate (Agent agent)
                                {
                                    if (agent.Health > maxHealth)
                                    {
                                        maxHealth = agent.Health;
                                        newAgent = agent;
                                    }
                                }, null);
                            }

                            Mission.Current.ReplaceBotWithPlayer(newAgent, component);
                            component.WantsToSpawnAsBot = false;
                            component.HasSpawnTimerExpired = false;
                        }
                    }
                }
            }
        }
    }
    private void MakePlayerFormationCharge(NetworkCommunicator peer)
    {
        if (peer.IsSynchronized)
        {
            MissionPeer component = peer.GetComponent<MissionPeer>();
            if (component.ControlledFormation != null)
            {
                component.ControlledFormation.SetMovementOrder(MovementOrder.MovementOrderCharge);
            }
        }
    }
    private void OnPreTeamChanged(NetworkCommunicator peer, Team currentTeam, Team newTeam)
    {
        if (peer.IsSynchronized && peer.GetComponent<MissionPeer>().ControlledAgent != null)
        {
            this.MakePlayerFormationCharge(peer);
        }
    }
}
