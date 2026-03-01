using System.Xml.Serialization;
using Crpg.Module.Common;
using Crpg.Module.Common.AiComponents;
using Crpg.Module.Rewards;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Objects.Siege;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvServer : MissionMultiplayerGameModeBase
{
    private const int RewardMultiplier = 2;
    private const int MapDuration = 60 * 120;

    private const float UpkeepMultiplier = 0.50f;
    private const int BoulderRefillTime = 30;
    private const int FirePotRefillTime = 60;
    private const int BallistaRefillTime = 30;

    private readonly CrpgRewardServer _rewardServer;
    private readonly CrpgDtvData _dtvData;
    private readonly List<Agent> _mountsToKillNextRound = new();

    private int _currentRound;
    private int _currentRoundDefendersCount;
    private int _currentWave;
    private bool _gameStarted;
    private bool _waveStarted;
    private bool _timerExpired;
    private int? _vipAgentIndex;
    private MissionTimer? _waveStartTimer;
    private MissionTimer? _endGameTimer;
    private MissionTimer _boulderRefillTimer = null!;
    private MissionTimer _firePotRefillTimer = null!;
    private MissionTimer _ballistaRefillTimer = null!;
    private MissionTime _currentRoundStartTime;

    public CrpgDtvServer(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
        _dtvData = ReadDtvData();
        _gameStarted = false;
        _currentRound = -1;
        _timerExpired = false;
    }

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => false;

    private CrpgDtvSpawningBehavior SpawningBehavior => (CrpgDtvSpawningBehavior)SpawnComponent.SpawningBehavior;
    private int RoundsCount => _dtvData.Rounds.Count;
    private CrpgDtvRound CurrentRoundData => _dtvData.Rounds[_currentRound];
    private CrpgDtvWave CurrentWaveData => _dtvData.Rounds[_currentRound].Waves[_currentWave];
    private int WavesCountForCurrentRound => CurrentRoundData.Waves.Count;
    public event Action<int>? DtvRoundStarted;

    [Flags]
    private enum SiegeWeaponTypes
    {
        All = ~0x0,
        Boulder = 0x1,
        FirePot = 0x2,
        Ballista = 0x4,
    }

    public override MultiplayerGameType GetMissionType()
    {
        return MultiplayerGameType.Battle;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        AddTeams();
        _boulderRefillTimer = new MissionTimer(BoulderRefillTime);
        _firePotRefillTimer = new MissionTimer(FirePotRefillTime);
        _ballistaRefillTimer = new MissionTimer(BallistaRefillTime);
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        MissionLobbyComponent.CurrentMultiplayerStateChanged += RewardIfEndOfMission;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        MissionLobbyComponent.CurrentMultiplayerStateChanged -= RewardIfEndOfMission;
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

    public override void OnPeerChangedTeam(NetworkCommunicator networkPeer, Team oldTeam, Team newTeam)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (missionPeer == null)
        {
            return;
        }

        if (newTeam != Mission.SpectatorTeam)
        {
            missionPeer.Team = Mission.DefenderTeam;
            return;
        }

        if (crpgPeer != null)
        {
            crpgPeer.LastSpawnInfo = null;
        }
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        if (agent.IsAIControlled && agent.Team == Mission.DefenderTeam) // VIP agent spawned
        {
            _vipAgentIndex = agent.Index;

            // Send VIP agent index to the players.
            SendDataToPeers(new CrpgDtvVipSpawn { VipAgentIndex = agent.Index });
        }

        // Synchronize health with all clients to make the spectator health bar work.
        agent.UpdateSyncHealthToAllClients(true);
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !CanGameModeSystemsTickThisFrame)
        {
            return;
        }

        if (_endGameTimer != null)
        {
            if (_endGameTimer.Check())
            {
                _endGameTimer = null;
                MissionLobbyComponent.SetStateEndingAsServer();
            }

            // If the game is ending don't process the tick any further.
            return;
        }

        if (_boulderRefillTimer.Check(true))
        {
            RefillSiegeWeaponAmmo(SiegeWeaponTypes.Boulder);
        }

        if (_firePotRefillTimer.Check(true))
        {
            RefillSiegeWeaponAmmo(SiegeWeaponTypes.FirePot);
        }

        if (_ballistaRefillTimer.Check(true))
        {
            RefillSiegeWeaponAmmo(SiegeWeaponTypes.Ballista);
        }

        if (!_gameStarted)
        {
            _gameStarted = true;
            SetTimeLimit();
            ClearPeerCounts();
            Mission.GetMissionBehavior<MissionScoreboardComponent>().ResetBotScores();
            StartNextRound();
        }
        else if (_waveStarted)
        {
            CheckForWaveEnd();
        }
        else if (_waveStartTimer != null && _waveStartTimer.Check())
        {
            _waveStartTimer = null;
            StartNextWave();
        }
    }

    public void RewardIfEndOfMission(MissionLobbyComponent.MultiplayerGameState newState)
    {
        if (!_gameStarted)
        {
            return;
        }

        if (!_timerExpired && TimerComponent.CheckIfTimerPassed() && newState == MissionLobbyComponent.MultiplayerGameState.Ending) // Award players if timer expires
        {
            _timerExpired = true;
            float roundDuration = _currentRoundStartTime.ElapsedSeconds;
            _ = _rewardServer.UpdateCrpgUsersAsync(
                durationRewarded: ComputeRoundReward(CurrentRoundData, wavesWon: Math.Max(_currentWave, 0)),
                durationUpkeep: roundDuration * UpkeepMultiplier,
                constantMultiplier: RewardMultiplier);
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (!_gameStarted)
        {
            return;
        }

        if (affectedAgent.IsAIControlled && affectedAgent.Team == Mission.DefenderTeam) // VIP under attack
        {
            SendDataToPeers(new CrpgDtvGameEnd { VipDead = true, VipAgentIndex = affectedAgent.Index });
        }
    }

    public override void OnScoreHit(
        Agent affectedAgent,
        Agent affectorAgent,
        WeaponComponentData attackerWeapon,
        bool isBlocked,
        bool isSiegeEngineHit,
        in Blow blow,
        in AttackCollisionData collisionData,
        float damagedHp,
        float hitDistance,
        float shotDifficulty)
    {
        if (!_gameStarted)
        {
            return;
        }

        if (affectedAgent.IsAIControlled && affectedAgent.Team == Mission.DefenderTeam && affectorAgent.Team != Mission.DefenderTeam) // VIP under attack
        {
            affectedAgent.MakeVoice(SkinVoiceManager.VoiceType.Fear, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
            SendDataToPeers(new CrpgDtvVipUnderAttackMessage { AgentAttackerIndex = affectorAgent.Index, AgentVictimIndex = affectedAgent.Index });
        }

        if (affectorAgent.Team == Mission.AttackerTeam && affectorAgent.IsAIControlled && affectedAgent.Team == Mission.DefenderTeam)
        {
            DtvAiComponent component = affectorAgent.GetComponent<DtvAiComponent>();
            component?.ResetTargetTimer();
        }
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        if (!_gameStarted)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new CrpgDtvSetTimerMessage
        {
            StartTime = (int)TimerComponent.GetCurrentTimerStartTime().ToSeconds,
            Duration = MapDuration,
        });
        GameNetwork.EndModuleEventAsServer();
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new CrpgDtvCurrentProgressMessage
        {
            Wave = _currentWave,
            Round = _currentRound,
        });
        GameNetwork.EndModuleEventAsServer();

        // If player joins after the VIP agent has spawned, then send them the vip agent index. (Could maybe use to rename the message since its not only used when the VIP spawns...)
        if (_vipAgentIndex != null)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgDtvVipSpawn { VipAgentIndex = (int)_vipAgentIndex });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    /// <summary>Work around the 60 minutes limit of MapTimeLimit.</summary>
    private void SetTimeLimit()
    {
        TimerComponent.StartTimerAsServer(MapDuration);
        SendDataToPeers(new CrpgDtvSetTimerMessage
        {
            StartTime = (int)TimerComponent.GetCurrentTimerStartTime().ToSeconds,
            Duration = MapDuration,
        });
    }

    private void StartNextRound()
    {
        _currentRound += 1;
        _currentWave = -1;
        SpawningBehavior.RequestSpawnSessionForRoundStart(firstRound: _currentRound == 0);
        SendDataToPeers(new CrpgDtvRoundStartMessage { Round = _currentRound });
        var mountToKill = Mission.MountsWithoutRiders.Select(m => m.Key).Intersect(_mountsToKillNextRound).ToList();
        foreach (var mount in mountToKill) // Select because MountWithoutRiders is an <Agent, Time> keyValuePair
        {
            DamageHelper.DamageAgent(mount, (int)mount.Health + 2);
        }

        _mountsToKillNextRound.Clear();
        foreach (var mount in Mission.MountsWithoutRiders) // force mounts to flee and mark them to die next round
        {
            Agent mountAgent = mount.Key;
            _mountsToKillNextRound.Add(mountAgent);
            mountAgent.CommonAIComponent.Panic();
        }

        _currentRoundStartTime = MissionTime.Now;
        _waveStartTimer = new MissionTimer(15f);
        _waveStarted = false;

        DtvRoundStarted?.Invoke(_currentRound);
    }

    private void StartNextWave()
    {
        _currentWave += 1;
        RefillDefendersHealthPointsAndAmmo();
        _currentRoundDefendersCount = _currentWave == 0 ? GetDefendersCount() : _currentRoundDefendersCount;
        SpawningBehavior.RequestSpawnSessionForWaveStart(CurrentWaveData, _currentRoundDefendersCount);
        SendDataToPeers(new CrpgDtvWaveStartMessage { Wave = _currentWave });
        _waveStarted = true;
    }

    private void CheckForWaveEnd()
    {
        bool vipDead = !Mission.DefenderTeam.HasBots;
        bool defendersDepleted = Mission.DefenderTeam.ActiveAgents.Count == (vipDead ? 0 : 1);
        float roundDuration = _currentRoundStartTime.ElapsedSeconds;

        if (defendersDepleted)
        {
            SendDataToPeers(new CrpgDtvGameEnd { VipDead = false });
        }

        if (vipDead || defendersDepleted)
        {
            _ = _rewardServer.UpdateCrpgUsersAsync(
                durationRewarded: ComputeRoundReward(CurrentRoundData, wavesWon: _currentWave),
                durationUpkeep: roundDuration * UpkeepMultiplier,
                constantMultiplier: RewardMultiplier);
            EndGame(Mission.AttackerTeam);
            return;
        }

        bool attackersDepleted = !Mission.AttackerTeam.HasBots;
        if (!attackersDepleted)
        {
            return;
        }

        if (_currentWave < WavesCountForCurrentRound - 1)
        {
            StartNextWave();
            return;
        }

        _ = _rewardServer.UpdateCrpgUsersAsync(
            durationRewarded: ComputeRoundReward(CurrentRoundData, wavesWon: _currentWave + 1),
            durationUpkeep: roundDuration * UpkeepMultiplier,
            constantMultiplier: RewardMultiplier);

        if (_currentRound < RoundsCount - 1)
        {
            StartNextRound();
        }
        else
        {
            EndGame(Mission.DefenderTeam);
        }
    }

    private void EndGame(Team winnerTeam)
    {
        Mission.GetMissionBehavior<MissionScoreboardComponent>().ChangeTeamScore(winnerTeam, 1);
        _endGameTimer = new MissionTimer(8f);
    }

    private void RefillDefendersHealthPointsAndAmmo()
    {
        foreach (Agent agent in Mission.DefenderTeam.ActiveAgents)
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
    }

    private void RefillSiegeWeaponAmmo(SiegeWeaponTypes types = SiegeWeaponTypes.All)
    {
        // Boulder: StonePile.GivenItemID == "boulder" or (is Mangonel and not is FireMangonel)
        // Pot: StonePile.GivenItemID == "pot" or (is Mangonel and not is FireMangonel)

        foreach (var usableMachine in Mission.MissionObjects.FindAllWithType<UsableMachine>())
        {
            int ammoCount;
            int startingAmmoCount;
            Action<int> setAmmo;
            Func<int, GameNetworkMessage> createNetworkMessage;
            if (usableMachine is StonePile stonePile)
            {
                if ((stonePile.GivenItemID != "boulder" || !types.HasFlag(SiegeWeaponTypes.Boulder))
                    && (stonePile.GivenItemID != "pot" || !types.HasFlag(SiegeWeaponTypes.FirePot)))
                {
                    continue;
                }

                ammoCount = stonePile.AmmoCount;
                startingAmmoCount = stonePile.StartingAmmoCount;
                setAmmo = a => stonePile.SetAmmo(a);
                createNetworkMessage = a => new SetStonePileAmmo(stonePile.Id, a);
            }
            else if (usableMachine is RangedSiegeWeapon siegeWeapon)
            {
                var type = usableMachine.GetType();
                if ((usableMachine is not Ballista || !types.HasFlag(SiegeWeaponTypes.Ballista))
                    && (type != typeof(Mangonel) || !types.HasFlag(SiegeWeaponTypes.Boulder))
                    && (type != typeof(Trebuchet) || !types.HasFlag(SiegeWeaponTypes.Boulder))
                    && (type != typeof(FireMangonel) || !types.HasFlag(SiegeWeaponTypes.FirePot))
                    && (type != typeof(FireTrebuchet) || !types.HasFlag(SiegeWeaponTypes.FirePot)))
                {
                    continue;
                }

                ammoCount = siegeWeapon.AmmoCount;
                startingAmmoCount = siegeWeapon.StartingAmmoCount;
                setAmmo = a => siegeWeapon.SetAmmo(a);
                createNetworkMessage = a => new SetRangedSiegeWeaponAmmo(siegeWeapon.Id, a);
            }
            else
            {
                continue;
            }

            if (ammoCount != startingAmmoCount)
            {
                int newAmmoCount = ammoCount + 1;
                Debug.Print($"Setting ammo from {ammoCount} to {newAmmoCount} for {usableMachine.GetType().Name}");
                newAmmoCount = Math.Min(newAmmoCount, startingAmmoCount);
                setAmmo(newAmmoCount);
                usableMachine.Activate();

                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(createNetworkMessage(newAmmoCount));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord);
            }
        }
    }

    private int GetDefendersCount()
    {
        int defendersCount = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (!networkPeer.IsSynchronized
                || crpgPeer.LastSpawnInfo == null)
            {
                continue;
            }

            defendersCount += 1;
        }

        return defendersCount;
    }

    private float ComputeRoundReward(CrpgDtvRound data, int wavesWon)
    {
        float defendersScale = _currentRoundDefendersCount <= 10
            ? 1 + (_currentRoundDefendersCount - 1) * 0.0125f
            : 1 + 9 * 0.0125f + (_currentRoundDefendersCount - 10) * 0.0035f;

        float lostRoundPenalty = (float)wavesWon / data.Waves.Count;
        return data.Reward * defendersScale * lostRoundPenalty;
    }

    private void SendDataToPeers(GameNetworkMessage message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(message);
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void AddTeams()
    {
        BasicCultureObject attackerTeamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = attackerTeamCulture.Banner;
        Mission.Teams.Add(BattleSideEnum.Attacker, attackerTeamCulture.BackgroundColor1, attackerTeamCulture.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject defenderTeamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = defenderTeamCulture.Banner;
        Mission.Teams.Add(BattleSideEnum.Defender, defenderTeamCulture.BackgroundColor2, defenderTeamCulture.ForegroundColor2, bannerTeam2, false, true);
    }

    private CrpgDtvData ReadDtvData()
    {
        XmlSerializer ser = new(typeof(CrpgDtvData));
        using StreamReader sr = new(ModuleHelper.GetXmlPath("Crpg", "dtv\\dtv_data"));
        return (CrpgDtvData)ser.Deserialize(sr)!;
    }
}
