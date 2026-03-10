using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Rewards;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.TrainingGround;

internal class CrpgTrainingGroundServer : MissionMultiplayerGameModeBase
{
    private class DuelInfo
    {
        private enum ChallengerType
        {
            None = -1,
            Requester,
            Requestee,
            NumChallengerType,
        }

        private struct Challenger
        {
            public readonly MissionPeer MissionPeer = null!;
            public readonly NetworkCommunicator? NetworkPeer;
            private readonly CrpgTrainingGroundServer _crpgTrainingGroundServer;
            public Agent? DuelingAgent { get; private set; }
            public Agent? MountAgent { get; private set; }
            public int KillCountInDuel { get; private set; }

            public Challenger(MissionPeer missionPeer, CrpgTrainingGroundServer crpgTrainingGroundServer)
            {
                MissionPeer = missionPeer;
                NetworkPeer = MissionPeer.GetNetworkPeer();
                DuelingAgent = null;
                MountAgent = null;
                KillCountInDuel = 0;
                _crpgTrainingGroundServer = crpgTrainingGroundServer;
            }

            public void OnDuelPreparation(Team duelingTeam)
            {
                Agent? agent = MissionPeer.ControlledAgent;
                if (agent != null)
                {
                    agent.SetTeam(duelingTeam, true);
                    _crpgTrainingGroundServer.RefillAgentHealthAndAmmo(agent);
                    SetAgents(agent);
                }

                MissionPeer.Team = duelingTeam;
                MissionPeer.HasSpawnedAgentVisuals = true;
            }

            public void OnDuelEnded()
            {
                if (MissionPeer.Peer.Communicator.IsConnectionActive)
                {
                    MissionPeer.Team = Mission.Current.AttackerTeam;
                }
            }

            public void IncreaseWinCount()
            {
                KillCountInDuel++;
            }

            public void SetAgents(Agent agent)
            {
                DuelingAgent = agent;
                MountAgent = DuelingAgent.MountAgent;
            }
        }

        private const float DuelStartCountdown = 5f;
        private readonly Challenger[] _challengers;

        private readonly CrpgTrainingGroundServer _crpgTrainingGroundServer;

        private ChallengerType _winnerChallengerType = ChallengerType.None;
        public MissionPeer RequesterPeer => _challengers[0].MissionPeer;
        public MissionPeer RequesteePeer => _challengers[1].MissionPeer;
        public MissionTime Timer { get; private set; }
        public Team DuelingTeam { get; private set; } = null!;
        public bool Started { get; private set; }
        public bool ChallengeEnded { get; private set; }
        public MissionPeer? ChallengeWinnerPeer
        {
            get
            {
                if (_winnerChallengerType != ChallengerType.None)
                {
                    return _challengers[(int)_winnerChallengerType].MissionPeer;
                }

                return null;
            }
        }

        public MissionPeer? ChallengeLoserPeer
        {
            get
            {
                if (_winnerChallengerType != ChallengerType.None)
                {
                    return _challengers[_winnerChallengerType == ChallengerType.Requester ? 1 : 0].MissionPeer;
                }

                return null;
            }
        }

        public DuelInfo(MissionPeer requesterPeer, MissionPeer requesteePeer, CrpgTrainingGroundServer crpgTrainingGroundServer)
        {
            _challengers = new Challenger[2];
            _challengers[0] = new Challenger(requesterPeer, crpgTrainingGroundServer);
            _challengers[1] = new Challenger(requesteePeer, crpgTrainingGroundServer);
            Timer = MissionTime.Now + MissionTime.Seconds(DuelRequestTimeOutInSeconds + DuelRequestTimeOutServerToleranceInSeconds);
            _crpgTrainingGroundServer = crpgTrainingGroundServer;
        }

        public void OnDuelPreparation(Team duelTeam)
        {
            if (!Started)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new TrainingGroundDuelPreparationStartedForTheFirstTime { RequesterPeer = _challengers[0].MissionPeer.GetNetworkPeer(), RequesteePeer = _challengers[1].MissionPeer.GetNetworkPeer() });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            Started = false;
            DuelingTeam = duelTeam;
            _winnerChallengerType = ChallengerType.None;
            for (int i = 0; i < 2; i++)
            {
                _challengers[i].OnDuelPreparation(DuelingTeam);
                _challengers[i].MissionPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>().OnDuelPreparation(_challengers[0].MissionPeer, _challengers[1].MissionPeer);
            }

            Timer = MissionTime.Now + MissionTime.Seconds(DuelStartCountdown);
        }

        public void OnDuelStarted()
        {
            Started = true;
            DuelingTeam.SetIsEnemyOf(DuelingTeam, isEnemyOf: true);
        }

        public void OnDuelEnding()
        {
            Timer = MissionTime.Now + MissionTime.Seconds(DuelEndInSeconds);
        }

        public void OnDuelEnded()
        {
            if (Started)
            {
                DuelingTeam.SetIsEnemyOf(DuelingTeam, isEnemyOf: false);
            }

            DecideRoundWinner();
            for (int i = 0; i < 2; i++)
            {
                _challengers[i].OnDuelEnded();
                Agent agent = _challengers[i].DuelingAgent ?? _challengers[i].MissionPeer.ControlledAgent;
                if (ChallengeEnded && agent != null && agent.IsActive())
                {
                    _crpgTrainingGroundServer.RefillAgentHealthAndAmmo(agent);
                }

                _challengers[i].MissionPeer.HasSpawnedAgentVisuals = true;
            }

            for (int j = 0; j < 2; j++)
            {
                if (_challengers[j].MountAgent != null && _challengers[j].MountAgent!.IsActive() && (ChallengeEnded || _challengers[j].MountAgent!.RiderAgent == null))
                {
                    _challengers[j].MountAgent!.FadeOut(hideInstantly: true, hideMount: false);
                }
            }
        }

        public void OnAgentBuild(Agent agent)
        {
            for (int i = 0; i < 2; i++)
            {
                if (_challengers[i].MissionPeer == agent.MissionPeer)
                {
                    _challengers[i].SetAgents(agent);
                    break;
                }
            }
        }

        public bool IsDuelStillValid(bool doNotCheckAgent = false)
        {
            for (int i = 0; i < 2; i++)
            {
                if (!_challengers[i].MissionPeer.Peer.Communicator.IsConnectionActive || (!doNotCheckAgent && !_challengers[i].MissionPeer.IsControlledAgentActive))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsPeerInThisDuel(MissionPeer peer)
        {
            for (int i = 0; i < 2; i++)
            {
                if (_challengers[i].MissionPeer == peer)
                {
                    return true;
                }
            }

            return false;
        }

        private void DecideRoundWinner()
        {
            bool isConnectionActive = _challengers[0].MissionPeer.Peer.Communicator.IsConnectionActive;
            bool isConnectionActive2 = _challengers[1].MissionPeer.Peer.Communicator.IsConnectionActive;
            if (!Started)
            {
                if (isConnectionActive == isConnectionActive2)
                {
                    ChallengeEnded = true;
                }
                else
                {
                    _winnerChallengerType = !isConnectionActive ? ChallengerType.Requestee : ChallengerType.Requester;
                }
            }
            else
            {
                Agent? duelingAgent = _challengers[0].DuelingAgent;
                Agent? duelingAgent2 = _challengers[1].DuelingAgent;
                if (duelingAgent != null && duelingAgent.IsActive())
                {
                    _winnerChallengerType = ChallengerType.Requester;
                }
                else if (duelingAgent2 != null && duelingAgent2.IsActive())
                {
                    _winnerChallengerType = ChallengerType.Requestee;
                }
                else
                {
                    if (!isConnectionActive && !isConnectionActive2)
                    {
                        ChallengeEnded = true;
                    }

                    _winnerChallengerType = ChallengerType.None;
                }
            }

            if (_winnerChallengerType != ChallengerType.None)
            {
                _challengers[(int)_winnerChallengerType].IncreaseWinCount();
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new DuelRoundEnded(_challengers[(int)_winnerChallengerType].NetworkPeer));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                if (_challengers[(int)_winnerChallengerType].KillCountInDuel == 1 || !isConnectionActive || !isConnectionActive2)
                {
                    ChallengeEnded = true;
                }
            }
        }
    }

    public delegate void OnDuelEndedDelegate(MissionPeer winnerPeer);

    public const float DuelRequestTimeOutInSeconds = 10f;
    public const float DuelEndInSeconds = 2f;
    private const float DuelRequestTimeOutServerToleranceInSeconds = 0.5f;
    private const float CorpseFadeOutTimeInSeconds = 1f;

    private CrpgTrainingGroundSpawningBehavior SpawningBehavior => (CrpgTrainingGroundSpawningBehavior)SpawnComponent.SpawningBehavior;

    private readonly CrpgRewardServer _rewardServer;
    private readonly Queue<Team> _deactiveDuelTeams = new();
    private readonly List<DuelInfo> _duelRequests = new();
    private readonly List<DuelInfo> _activeDuels = new();
    private readonly List<DuelInfo> _endingDuels = new();
    private readonly List<DuelInfo> _restartingDuels = new();
    private readonly List<DuelInfo> _restartPreparationDuels = new();
    private readonly List<KeyValuePair<MissionPeer, TroopType>> _peersAndSelections = new();
    private MissionTimer? _rewardTickTimer;

    public CrpgTrainingGroundServer(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
    }

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => false;
    public event OnDuelEndedDelegate? OnDuelEnded;

    public override MultiplayerGameType GetMissionType()
    {
        return MultiplayerGameType.Duel;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        Mission.Current.SetMissionCorpseFadeOutTimeInSeconds(CorpseFadeOutTimeInSeconds);
        BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner banner = @object.Banner;
        Mission.Teams.Add(BattleSideEnum.Attacker, @object.BackgroundColor1, @object.ForegroundColor1, banner, isPlayerGeneral: false);
        _rewardTickTimer = new(60);
    }

    public override bool CheckIfPlayerCanDespawn(MissionPeer missionPeer)
    {
        for (int i = 0; i < _activeDuels.Count; i++)
        {
            if (_activeDuels[i].IsPeerInThisDuel(missionPeer))
            {
                return false;
            }
        }

        return true;
    }

    public void OnPlayerDespawn(MissionPeer missionPeer)
    {
        missionPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>();
    }

    public void DuelRequestReceived(MissionPeer requesterPeer, MissionPeer requesteePeer)
    {
        if (!IsThereARequestBetweenPeers(requesterPeer, requesteePeer) && !IsHavingDuel(requesterPeer) && !IsHavingDuel(requesteePeer))
        {
            DuelInfo duelInfo = new(requesterPeer, requesteePeer, this);
            _duelRequests.Add(duelInfo);
            ((CrpgTrainingGroundMissionRepresentative)requesteePeer.Representative).DuelRequested(requesterPeer.ControlledAgent);
        }
    }

    public void DuelRequestAccepted(Agent requesterAgent, Agent requesteeAgent)
    {
        DuelInfo? duelInfo = _duelRequests.FirstOrDefault(dr => dr.IsPeerInThisDuel(requesterAgent.MissionPeer) && dr.IsPeerInThisDuel(requesteeAgent.MissionPeer));
        if (duelInfo != null)
        {
            PrepareDuel(duelInfo);
        }
    }

    public override void OnMissionTick(float dt)
    {
        RewardUsers();
        base.OnMissionTick(dt);
        CheckRestartPreparationDuels();
        CheckForRestartingDuels();
        CheckDuelsToStart();
        CheckDuelRequestTimeouts();
        CheckEndedDuels();
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        if (!agent.IsHuman || agent.Team == null)
        {
            return;
        }

        var networkPeer = agent.MissionPeer.GetNetworkPeer();

        if (networkPeer != null)
        {
            var component = networkPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>();

            CrpgPeer? crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            CrpgCharacterStatistics stats = crpgPeer.User!.Character.Statistics;

            if (!component.HasLoadedStats)
            {
                component.NumberOfWins = stats.Kills;
                component.NumberOfLosses = stats.Deaths;
                component.Rating = (int)stats.Rating.CompetitiveValue;
                component.HasLoadedStats = true;
            }

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new TrainingGroundDuelPointsUpdateMessage { NetworkCommunicator = component.GetNetworkPeer(), NumberOfWins = component.NumberOfWins, NumberOfLosses = component.NumberOfLosses, Rating = component.Rating });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        if (!agent.Team.IsDefender)
        {
            return;
        }

        for (int i = 0; i < _activeDuels.Count; i++)
        {
            if (_activeDuels[i].IsPeerInThisDuel(agent.MissionPeer))
            {
                _activeDuels[i].OnAgentBuild(agent);
                break;
            }
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (!affectedAgent.IsHuman)
        {
            return;
        }

        if (affectedAgent.Team.IsDefender)
        {
            DuelInfo? duelInfo = null;
            for (int i = 0; i < _activeDuels.Count; i++)
            {
                if (_activeDuels[i].IsPeerInThisDuel(affectedAgent.MissionPeer))
                {
                    duelInfo = _activeDuels[i];
                }
            }

            if (duelInfo != null && !_endingDuels.Contains(duelInfo))
            {
                duelInfo.OnDuelEnding();
                _endingDuels.Add(duelInfo);
            }

            return;
        }

        for (int num = _duelRequests.Count - 1; num >= 0; num--)
        {
            if (_duelRequests[num].IsPeerInThisDuel(affectedAgent.MissionPeer))
            {
                _duelRequests.RemoveAt(num);
            }
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.RegisterBaseHandler<TrainingGroundClientDuelRequest>(HandleClientEventDuelRequest);
        registerer.RegisterBaseHandler<DuelResponse>(HandleClientEventDuelRequestAccepted);
        registerer.RegisterBaseHandler<ClientRequestRefreshCharacter>(HandleClientRequestRefreshCharacter);
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<CrpgTrainingGroundMissionRepresentative>();
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
        component.Team = Mission.AttackerTeam;
        _peersAndSelections.Add(new KeyValuePair<MissionPeer, TroopType>(component, TroopType.Invalid));
    }

    protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        if (networkPeer.IsServerPeer)
        {
            return;
        }

        foreach (NetworkCommunicator networkPeer2 in GameNetwork.NetworkPeers)
        {
            CrpgTrainingGroundMissionRepresentative component = networkPeer2.GetComponent<CrpgTrainingGroundMissionRepresentative>();
            if (component != null)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new TrainingGroundDuelPointsUpdateMessage { NetworkCommunicator = component.GetNetworkPeer(), NumberOfWins = component.NumberOfWins, NumberOfLosses = component.NumberOfLosses, Rating = component.Rating });
                GameNetwork.EndModuleEventAsServer();
            }

            if (networkPeer != networkPeer2)
            {
                MissionPeer component2 = networkPeer2.GetComponent<MissionPeer>();
                if (component2 != null)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SyncPerksForCurrentlySelectedTroop(networkPeer2, component2.Perks[component2.SelectedTroopIndex]));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        if (SpawnComponent?.SpawningBehavior is CrpgTrainingGroundSpawningBehavior)
        {
            MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
            missionPeer?.SpawnTimer?.AdjustStartTime(-3f); // Used to reduce the initial spawn on connect.
        }

        for (int i = 0; i < _activeDuels.Count; i++)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new TrainingGroundDuelPreparationStartedForTheFirstTime { RequesterPeer = _activeDuels[i].RequesterPeer.GetNetworkPeer(), RequesteePeer = _activeDuels[i].RequesteePeer.GetNetworkPeer() });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
    {
        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
        for (int i = 0; i < _peersAndSelections.Count; i++)
        {
            if (_peersAndSelections[i].Key == component)
            {
                _peersAndSelections.RemoveAt(i);
                break;
            }
        }
    }

    protected override void HandlePlayerDisconnect(NetworkCommunicator networkPeer)
    {
        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
        if (component != null)
        {
            component.Team = null;
        }
    }

    private bool RefreshPlayer(NetworkCommunicator networkPeer)
    {
        return SpawningBehavior.RefreshPlayer(networkPeer);
    }

    private bool HandleClientEventDuelRequest(NetworkCommunicator? peer, GameNetworkMessage baseMessage)
    {
        TrainingGroundClientDuelRequest duelRequest = (TrainingGroundClientDuelRequest)baseMessage;
        MissionPeer? missionPeer = peer?.GetComponent<MissionPeer>();
        if (missionPeer != null)
        {
            Agent agentFromIndex = Mission.MissionNetworkHelper.GetAgentFromIndex(duelRequest.RequestedAgentIndex);
            if (agentFromIndex != null && agentFromIndex.IsActive())
            {
                DuelRequestReceived(missionPeer, agentFromIndex.MissionPeer);
            }
        }

        return true;
    }

    private bool HandleClientEventDuelRequestAccepted(NetworkCommunicator? peer, GameNetworkMessage baseMessage)
    {
        DuelResponse duelResponse = (DuelResponse)baseMessage;
        if (peer?.GetComponent<MissionPeer>() != null && peer.GetComponent<MissionPeer>().ControlledAgent != null && duelResponse.Peer?.GetComponent<MissionPeer>() != null && duelResponse.Peer.GetComponent<MissionPeer>().ControlledAgent != null)
        {
            DuelRequestAccepted(duelResponse.Peer.GetComponent<CrpgTrainingGroundMissionRepresentative>().ControlledAgent, peer.GetComponent<CrpgTrainingGroundMissionRepresentative>().ControlledAgent);
        }

        return true;
    }

    private bool HandleClientRequestRefreshCharacter(NetworkCommunicator peer, GameNetworkMessage baseMessage)
    {
        bool result = RefreshPlayer(peer);
        return result;
    }

    private Team ActivateAndGetDuelTeam()
    {
        if (_deactiveDuelTeams.Count <= 0)
        {
            return Mission.Teams.Add(BattleSideEnum.Defender, isPlayerGeneral: true, isPlayerSergeant: false, isSettingRelations: false);
        }

        return _deactiveDuelTeams.Dequeue();
    }

    private void DeactivateDuelTeam(Team team)
    {
        _deactiveDuelTeams.Enqueue(team);
    }

    private bool IsHavingDuel(MissionPeer peer)
    {
        return _activeDuels.AnyQ(d => d.IsPeerInThisDuel(peer));
    }

    private bool IsThereARequestBetweenPeers(MissionPeer requesterAgent, MissionPeer requesteeAgent)
    {
        for (int i = 0; i < _duelRequests.Count; i++)
        {
            if (_duelRequests[i].IsPeerInThisDuel(requesterAgent) && _duelRequests[i].IsPeerInThisDuel(requesteeAgent))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckDuelsToStart()
    {
        for (int num = _activeDuels.Count - 1; num >= 0; num--)
        {
            DuelInfo duelInfo = _activeDuels[num];
            if (!duelInfo.Started && duelInfo.Timer.IsPast && duelInfo.IsDuelStillValid())
            {
                StartDuel(duelInfo);
            }
        }
    }

    private void CheckDuelRequestTimeouts()
    {
        for (int num = _duelRequests.Count - 1; num >= 0; num--)
        {
            DuelInfo duelInfo = _duelRequests[num];
            if (duelInfo.Timer.IsPast)
            {
                _duelRequests.Remove(duelInfo);
            }
        }
    }

    private void CheckForRestartingDuels()
    {
        for (int num = _restartingDuels.Count - 1; num >= 0; num--)
        {
            if (!_restartingDuels[num].IsDuelStillValid(doNotCheckAgent: true))
            {
                Debug.Print("!_restartingDuels[i].IsDuelStillValid(true)");
            }

            _duelRequests.Add(_restartingDuels[num]);
            PrepareDuel(_restartingDuels[num]);
            _restartingDuels.RemoveAt(num);
        }
    }

    private void CheckEndedDuels()
    {
        for (int num = _endingDuels.Count - 1; num >= 0; num--)
        {
            DuelInfo duelInfo = _endingDuels[num];
            if (duelInfo.Timer.IsPast)
            {
                EndDuel(duelInfo);
                _endingDuels.RemoveAt(num);
                if (!duelInfo.ChallengeEnded)
                {
                    _restartPreparationDuels.Add(duelInfo);
                }
            }
        }
    }

    private void CheckRestartPreparationDuels()
    {
        for (int num = _restartPreparationDuels.Count - 1; num >= 0; num--)
        {
            DuelInfo duelInfo = _restartPreparationDuels[num];
            Agent controlledAgent = duelInfo.RequesterPeer.ControlledAgent;
            Agent controlledAgent2 = duelInfo.RequesteePeer.ControlledAgent;
            if ((controlledAgent == null || controlledAgent.IsActive()) && (controlledAgent2 == null || controlledAgent2.IsActive()))
            {
                _restartPreparationDuels.RemoveAt(num);
                _restartingDuels.Add(duelInfo);
            }
        }
    }

    private void PrepareDuel(DuelInfo duel)
    {
        _duelRequests.Remove(duel);
        if (!IsHavingDuel(duel.RequesteePeer) && !IsHavingDuel(duel.RequesterPeer))
        {
            _activeDuels.Add(duel);
            Team duelTeam = duel.Started ? duel.DuelingTeam : ActivateAndGetDuelTeam();
            duel.OnDuelPreparation(duelTeam);
        }
    }

    private void StartDuel(DuelInfo duel)
    {
        duel.OnDuelStarted();
    }

    private void EndDuel(DuelInfo duel)
    {
        _activeDuels.Remove(duel);
        duel.OnDuelEnded();
        if (duel.ChallengeEnded)
        {
            MissionPeer? challengeWinnerPeer = duel.ChallengeWinnerPeer;
            if (challengeWinnerPeer?.ControlledAgent != null)
            {
                OnDuelEnded?.Invoke(challengeWinnerPeer);
            }

            DeactivateDuelTeam(duel.DuelingTeam);
            HandleEndedChallenge(duel);
        }
    }

    private void HandleEndedChallenge(DuelInfo duel)
    {
        MissionPeer? challengeWinnerPeer = duel.ChallengeWinnerPeer;
        MissionPeer? challengeLoserPeer = duel.ChallengeLoserPeer;
        if (challengeWinnerPeer != null && challengeLoserPeer != null)
        {
            CrpgTrainingGroundMissionRepresentative component = challengeWinnerPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>();
            CrpgTrainingGroundMissionRepresentative component2 = challengeLoserPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>();
            CrpgPeer? winnerCrpgPeer = challengeWinnerPeer.GetComponent<CrpgPeer>();
            CrpgPeer? loserCrpgPeer = challengeLoserPeer.GetComponent<CrpgPeer>();

            if (winnerCrpgPeer != null && loserCrpgPeer != null)
            {
                _ = _rewardServer.OnDuelEnded(winnerCrpgPeer, loserCrpgPeer);
            }
            else
            {
                return;
            }

            CrpgCharacterRating newWinnerRating = winnerCrpgPeer.User!.Character.Statistics.Rating;
            CrpgCharacterRating newLoserRating = loserCrpgPeer.User!.Character.Statistics.Rating;

            component.OnDuelWon();
            component.Rating = (int)newWinnerRating.CompetitiveValue;
            if (challengeWinnerPeer.Peer.Communicator.IsConnectionActive)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new TrainingGroundDuelPointsUpdateMessage { NetworkCommunicator = component.GetNetworkPeer(), NumberOfWins = component.NumberOfWins, NumberOfLosses = component.NumberOfLosses, Rating = component.Rating });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            component2.OnDuelLost();
            component2.Rating = (int)newLoserRating.CompetitiveValue;
            if (challengeLoserPeer.Peer.Communicator.IsConnectionActive)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new TrainingGroundDuelPointsUpdateMessage { NetworkCommunicator = component2.GetNetworkPeer(), NumberOfWins = component2.NumberOfWins, NumberOfLosses = component2.NumberOfLosses, Rating = component2.Rating });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        MissionPeer peerComponent = challengeWinnerPeer ?? duel.RequesterPeer;
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new DuelEnded(peerComponent.GetNetworkPeer()));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void RefillAgentHealthAndAmmo(Agent agent)
    {
        agent.Health = agent.HealthLimit;
        if (agent.HasMount)
        {
            agent.MountAgent.Health = agent.MountAgent.HealthLimit;
        }

        for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i += 1)
        {
            var weapon = agent.Equipment[i];
            if (!weapon.IsEmpty && (weapon.IsAnyConsumable() || weapon.CurrentUsageItem.IsShield))
            {
                agent.SetWeaponAmountInSlot(i, weapon.ModifiedMaxAmount, false);
            }
        }
    }

    private void RewardUsers()
    {
        if (_rewardTickTimer!.Check(reset: true))
        {
            _ = _rewardServer.UpdateCrpgUsersAsync(
                durationRewarded: _rewardTickTimer.GetTimerDuration() * 0.75f,
                durationUpkeep: 0,
                updateUserStats: false);
        }
    }
}
