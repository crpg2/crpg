using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Modes.TrainingGround;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Rating;
using Polly;
using Polly.Retry;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Rewards;

/// <summary>
/// Gives xp/gold, update rating and stats.
/// </summary>
internal class CrpgRewardServer : MissionLogic
{
    public const int ExperienceMultiplierMin = 1;
    public const int ExperienceMultiplierMax = 5;

    private readonly ICrpgClient _crpgClient;
    private readonly CrpgConstants _constants;
    private readonly CrpgWarmupComponent? _warmupComponent;
    private readonly Dictionary<int, CrpgPlayerRating> _characterRatings;
    private readonly CrpgRatingPeriodResults _ratingResults;
    private readonly Random _random;
    private readonly PeriodStatsHelper _periodStatsHelper;
    private readonly Dictionary<int, AgentHitRegistry> _agentsThatGotTeamHitThisRoundByCrpgUserId;
    private readonly bool _isTeamHitCompensationsEnabled;
    private readonly bool _isRatingEnabled;
    private readonly bool _isLowPopulationUpkeepEnabled;

    private ResiliencePipeline _pipeline = default!;
    private bool _lastRewardDuringHappyHours;

    public CrpgRewardServer(
        ICrpgClient crpgClient,
        CrpgConstants constants,
        CrpgWarmupComponent? warmupComponent,
        bool enableTeamHitCompensations,
        bool enableRating,
        bool enableLowPopulationUpkeep = false)
    {
        _crpgClient = crpgClient;
        _constants = constants;
        _warmupComponent = warmupComponent;
        _characterRatings = new Dictionary<int, CrpgPlayerRating>();
        _ratingResults = new CrpgRatingPeriodResults();
        _random = new Random();
        _periodStatsHelper = new PeriodStatsHelper();
        _lastRewardDuringHappyHours = false;
        _agentsThatGotTeamHitThisRoundByCrpgUserId = new();
        _isTeamHitCompensationsEnabled = enableTeamHitCompensations;
        _isRatingEnabled = enableRating;
        _isLowPopulationUpkeepEnabled = enableLowPopulationUpkeep;
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        BuildResiliencePipeline();
        if (_warmupComponent != null)
        {
            _warmupComponent.OnWarmupEnded += OnWarmupEnded;
            _warmupComponent.OnWarmupRewardTick += OnWarmupRewardTick;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();

        if (_warmupComponent != null)
        {
            _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
            _warmupComponent.OnWarmupRewardTick -= OnWarmupRewardTick;
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
        if (_warmupComponent == null || _warmupComponent.IsInWarmup)
        {
            return;
        }

        if (affectedAgent == affectorAgent) // Self hit.
        {
            return;
        }

        if (affectedAgent.Team == affectorAgent.Team) // Team hit.
        {
            RegisterHitForCompensation(affectedAgent, affectorAgent, (int)damagedHp);
            return;
        }

        if (isSiegeEngineHit)
        {
            return;
        }

        if (!_isRatingEnabled
            || !TryGetRating(affectedAgent, out var affectedRating)
            || !TryGetRating(affectorAgent, out var affectorRating))
        {
            return;
        }

        float inflictedRatio = MathF.Clamp(blow.InflictedDamage / affectedAgent.BaseHealthLimit, 0f, 1f);
        _ratingResults.AddResult(affectorRating!, affectedRating!, inflictedRatio);
    }

    public async Task OnDuelEnded(CrpgPeer winnerPeer, CrpgPeer loserPeer)
    {
        List<CrpgUserUpdate> userUpdates = new();
        Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId = new();
        CrpgCharacterStatistics winnerStats = winnerPeer.User!.Character.Statistics;
        CrpgCharacterStatistics loserStats = loserPeer.User!.Character.Statistics;
        CrpgRatingPeriodResults duelResults = new();

        CrpgPlayerRating winnerRating = new(winnerStats.Rating.Value, winnerStats.Rating.Deviation, winnerStats.Rating.Volatility);
        CrpgPlayerRating loserRating = new(loserStats.Rating.Value, loserStats.Rating.Deviation, loserStats.Rating.Volatility);

        CrpgUserUpdate winnerUpdate = new()
        {
            UserId = winnerPeer.User.Id,
            CharacterId = winnerPeer.User.Character.Id,
            Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
            Statistics = new CrpgCharacterStatistics
            {
                Kills = 1,
                Deaths = 0,
                Assists = 0,
                PlayTime = TimeSpan.Zero,
                Rating = winnerStats.Rating,
            },
            BrokenItems = Array.Empty<CrpgUserDamagedItem>(),
            Instance = CrpgServerConfiguration.Instance,
        };
        crpgPeerByCrpgUserId[winnerUpdate.UserId] = winnerPeer;
        _characterRatings[winnerUpdate.CharacterId] = winnerRating;
        duelResults.AddParticipant(winnerRating);
        userUpdates.Add(winnerUpdate);

        CrpgUserUpdate loserUpdate = new()
        {
            UserId = loserPeer.User.Id,
            CharacterId = loserPeer.User.Character.Id,
            Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
            Statistics = new CrpgCharacterStatistics
            {
                Kills = 0,
                Deaths = 1,
                Assists = 0,
                PlayTime = TimeSpan.Zero,
                Rating = loserStats.Rating,
            },
            BrokenItems = Array.Empty<CrpgUserDamagedItem>(),
            Instance = CrpgServerConfiguration.Instance,
        };
        crpgPeerByCrpgUserId[loserUpdate.UserId] = loserPeer;
        _characterRatings[loserUpdate.CharacterId] = loserRating;
        duelResults.AddParticipant(loserRating);
        userUpdates.Add(loserUpdate);

        duelResults.AddResult(winnerRating, loserRating, 1);
        CrpgRatingCalculator.UpdateRatings(duelResults);

        winnerUpdate.Statistics.Rating = GetNewRating(winnerPeer);
        loserUpdate.Statistics.Rating = GetNewRating(loserPeer);

        if (userUpdates.Count == 0)
        {
            return;
        }

        Guid idempotencyKey = Guid.NewGuid();

        try
        {
            await _pipeline.ExecuteAsync(async cancellationToken =>
            {
                var request = new CrpgGameUsersUpdateRequest
                {
                    Updates = userUpdates,
                    Key = idempotencyKey.ToString(),
                };

                SetUserAsLoading(userUpdates.Select(u => u.UserId), crpgPeerByCrpgUserId, loading: true);
                var res = (await _crpgClient.UpdateUsersAsync(request)).Data!;
                SendDuelResultToPeers(res.UpdateResults, crpgPeerByCrpgUserId, winnerUpdate.UserId);
            });
        }
        catch (Exception e)
        {
            Debug.Print($"Couldn't update users - {e}");

            SendErrorToPeers(crpgPeerByCrpgUserId);
        }
        finally
        {
            SetUserAsLoading(userUpdates.Select(u => u.UserId), crpgPeerByCrpgUserId, loading: false);
        }
    }

    /// <summary>
    /// Update rating and statistics from the last time this method was called and also give rewards.
    /// </summary>
    /// <param name="durationRewarded">Duration for which the users should be rewarded.</param>
    /// <param name="durationUpkeep">Duration for which the users will pay for their equipment.</param>
    /// <param name="defenderMultiplierGain">Multiplier to add to the defenders. Can be negative.</param>
    /// <param name="attackerMultiplierGain">Multiplier to add to the attackers. Can be negative.</param>
    /// <param name="valourTeamSide">Team to give valour to.</param>
    /// <param name="constantMultiplier">Multiplier that should be given to everyone disregarding any other parameters.</param>
    /// <param name="updateUserStats">True if score and rating should be saved.</param>
    public async Task UpdateCrpgUsersAsync(
        float durationRewarded,
        float? durationUpkeep = null,
        int defenderMultiplierGain = 0,
        int attackerMultiplierGain = 0,
        BattleSideEnum? valourTeamSide = null,
        int? constantMultiplier = null,
        bool updateUserStats = true,
        bool isDuel = false)
    {
        var networkPeers = GameNetwork.NetworkPeers.ToArray();
        if (networkPeers.Length == 0)
        {
            return;
        }

        var playingPeers = networkPeers
            .Select(p => p.GetComponent<MissionPeer>())
            .Where(mp => mp != null && mp.Team != null && mp.Team.Side != BattleSideEnum.None)
            .ToList();

        bool veryLowPopulationServer = playingPeers.Count < 2;
        bool lowPopulationServer = !_isLowPopulationUpkeepEnabled && playingPeers.Count < 12;
        // Force constant multiplier if there is low population.
        constantMultiplier = veryLowPopulationServer ? ExperienceMultiplierMin : constantMultiplier;

        CrpgRatingCalculator.UpdateRatings(_ratingResults);

        Dictionary<PlayerId, PeriodStats> periodStats = updateUserStats
            ? _periodStatsHelper.ComputePeriodStats()
            : new Dictionary<PlayerId, PeriodStats>();

        HashSet<PlayerId> valorousPlayerIds;
        valorousPlayerIds = veryLowPopulationServer || !valourTeamSide.HasValue
            ? new HashSet<PlayerId>()
            : GetValorousPlayers(networkPeers, periodStats, valourTeamSide.Value);

        Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId = new();
        List<CrpgUserUpdate> userUpdates = new();
        Dictionary<int, IList<CrpgUserDamagedItem>> brokenItems = lowPopulationServer ? new() : GetBrokenItemsByCrpgUserId(networkPeers, durationUpkeep ?? durationRewarded);

        var compensationByCrpgUserId = _isTeamHitCompensationsEnabled ? CalculateCompensationByCrpgUserId(brokenItems) : new();
        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var playerId = networkPeer.VirtualPlayer.Id;

            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer?.User == null)
            {
                continue;
            }

            int crpgUserId = crpgPeer.User.Id;
            crpgPeerByCrpgUserId[crpgUserId] = crpgPeer;

            CrpgUserUpdate userUpdate = new()
            {
                UserId = crpgPeer.User.Id,
                CharacterId = crpgPeer.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                Statistics = new CrpgCharacterStatistics
                {
                    Kills = 0,
                    Deaths = 0,
                    Assists = 0,
                    PlayTime = TimeSpan.Zero,
                    Rating = crpgPeer.User.Character.Statistics.Rating,
                },
                BrokenItems = Array.Empty<CrpgUserDamagedItem>(),
                Instance = CrpgServerConfiguration.Instance,
            };

            if (CrpgFeatureFlags.IsEnabled(CrpgFeatureFlags.FeatureTournament))
            {
                userUpdates.Add(userUpdate);
                continue;
            }

            if (updateUserStats)
            {
                SetStatistics(userUpdate, networkPeer, periodStats);
            }

            if (_isRatingEnabled && updateUserStats)
            {
                userUpdate.Statistics.Rating = GetNewRating(crpgPeer);
            }

            bool isPlayerInSpectator = missionPeer.Team == null || missionPeer.Team?.Side == BattleSideEnum.None;
            if (crpgPeer.LastSpawnInfo != null && !isPlayerInSpectator)
            {
                bool isValorousPlayer = valorousPlayerIds.Contains(playerId);
                int compensationForCrpgUser = compensationByCrpgUserId.TryGetValue(crpgUserId, out int compensation) ? compensation : 0;
                SetRewardForConnectedPlayer(userUpdate, crpgPeer, durationRewarded, compensationForCrpgUser, isValorousPlayer,
                    defenderMultiplierGain, attackerMultiplierGain, constantMultiplier);

                if (brokenItems.TryGetValue(crpgUserId, out var userBrokenItems))
                {
                    userUpdate.BrokenItems = userBrokenItems;
                }

                // TODO should probably implement compensation for disconnected users here
            }
            else if (crpgPeer.LastSpawnInfo != null && isPlayerInSpectator) // update spectators multiplier based on their previous team
            {
                SetRewardForConnectedPlayer(userUpdate, crpgPeer, 0, 0, false,
                    defenderMultiplierGain, attackerMultiplierGain, constantMultiplier);
            }

            userUpdates.Add(userUpdate);
        }

        _ratingResults.Clear();
        _characterRatings.Clear();
        _agentsThatGotTeamHitThisRoundByCrpgUserId.Clear();

        if (userUpdates.Count == 0)
        {
            return;
        }

        Guid idempotencyKey = Guid.NewGuid();

        try
        {
            await _pipeline.ExecuteAsync(async cancellationToken =>
            {
                var request = new CrpgGameUsersUpdateRequest
                {
                    Updates = userUpdates,
                    Key = idempotencyKey.ToString(),
                };

                SetUserAsLoading(userUpdates.Select(u => u.UserId), crpgPeerByCrpgUserId, true);
                var res = (await _crpgClient.UpdateUsersAsync(request)).Data!;
                SendRewardToPeers(res.UpdateResults, crpgPeerByCrpgUserId, valorousPlayerIds, compensationByCrpgUserId, lowPopulationServer, isDuel);
            });
        }
        catch (Exception e)
        {
            Debug.Print($"Couldn't update users - {e}");

            SendErrorToPeers(crpgPeerByCrpgUserId);

        }
        finally
        {
            SetUserAsLoading(userUpdates.Select(u => u.UserId), crpgPeerByCrpgUserId, false);
        }
    }

    private Dictionary<int, IList<CrpgUserDamagedItem>> GetBrokenItemsByCrpgUserId(NetworkCommunicator[] networkPeers, float duration)
    {
        Dictionary<int, IList<CrpgUserDamagedItem>> brokenItems = new();
        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer?.User == null)
            {
                continue;
            }

            if (crpgPeer.LastSpawnInfo != null)
            {
                int crpgUserId = crpgPeer.User.Id;
                var crpgUserDamagedItems = BreakItems(crpgPeer, duration);
                brokenItems[crpgUserId] = crpgUserDamagedItems;
            }
        }

        return brokenItems;
    }

    private void RegisterHitForCompensation(Agent affectedAgent, Agent affectorAgent, int inflictedDamage)
    {
        if (!TryGetCrpgUserId(affectedAgent, out int affectedCrpgUserId) || !TryGetCrpgUserId(affectorAgent, out int affectorCrpgUserId))
        {
            return;
        }

        if (!_agentsThatGotTeamHitThisRoundByCrpgUserId.TryGetValue(affectedCrpgUserId, out var affectedAgentHitRegistry))
        {
            affectedAgentHitRegistry = new AgentHitRegistry(affectedCrpgUserId, (int)affectedAgent.BaseHealthLimit);
            _agentsThatGotTeamHitThisRoundByCrpgUserId.Add(affectedCrpgUserId, affectedAgentHitRegistry);
        }

        if (affectedAgentHitRegistry.DamageByUserId.ContainsKey(affectorCrpgUserId))
        {
            affectedAgentHitRegistry.DamageByUserId[affectorCrpgUserId] += inflictedDamage;
        }
        else
        {
            affectedAgentHitRegistry.DamageByUserId.Add(affectorCrpgUserId, inflictedDamage);
        }
    }

    private bool TryGetCrpgUserId(Agent agent, out int crpgUserId)
    {
        crpgUserId = 0;
        var user = agent.MissionPeer?.GetNetworkPeer()?.GetComponent<CrpgPeer>()?.User;
        if (user == null)
        {
            return false;
        }

        crpgUserId = user.Id;
        return true;
    }

    private Dictionary<int, int> CalculateCompensationByCrpgUserId(Dictionary<int, IList<CrpgUserDamagedItem>> brokenItemsByCrpgUserId)
    {
        Dictionary<int, int> compensationByCrpgUserId = new();
        foreach (var affectedEntry in _agentsThatGotTeamHitThisRoundByCrpgUserId)
        {
            int affectedCrpgUserId = affectedEntry.Key;
            AgentHitRegistry affectedAgentHitRegistry = affectedEntry.Value;
            foreach (var damageByUserIdEntry in affectedAgentHitRegistry.DamageByUserId)
            {
                int affectorCrpgUserId = damageByUserIdEntry.Key;
                int damageDone = damageByUserIdEntry.Value;

                if (affectedCrpgUserId == affectorCrpgUserId)
                {
                    continue;
                }

                float compensationRatio = damageDone / (float)affectedAgentHitRegistry.BaseHealthLimit;
                int repairCostOfAffectedUser = brokenItemsByCrpgUserId.TryGetValue(affectedCrpgUserId, out var brokenItems) ? brokenItems.Sum(r => r.RepairCost) : 0;
                int compensatedRepairCost = (int)Math.Floor(repairCostOfAffectedUser * compensationRatio);

                if (!compensationByCrpgUserId.TryAdd(affectedCrpgUserId, compensatedRepairCost))
                {
                    compensationByCrpgUserId[affectedCrpgUserId] += compensatedRepairCost;
                }

                if (!compensationByCrpgUserId.TryAdd(affectorCrpgUserId, compensatedRepairCost * -1))
                {
                    compensationByCrpgUserId[affectorCrpgUserId] -= compensatedRepairCost;
                }
            }
        }

        return compensationByCrpgUserId;
    }

    private bool TryGetRating(Agent agent, out CrpgPlayerRating? rating)
    {
        rating = null!;
        if (agent.MissionPeer == null)
        {
            return false;
        }

        var crpgPeer = agent.MissionPeer.Peer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User == null)
        {
            return false;
        }

        int characterId = crpgPeer.User.Character.Id;
        if (!_characterRatings.TryGetValue(characterId, out rating))
        {
            // If the user has no region yet or they are not playing locally, act like they weren't there. That is, don't
            // change their rating or their opponent rating.
            if (crpgPeer.User.Region != CrpgServerConfiguration.Region)
            {
                return false;
            }

            var characterRating = crpgPeer.User.Character.Statistics.Rating;
            rating = new CrpgPlayerRating(characterRating.Value, characterRating.Deviation, characterRating.Volatility);
            _characterRatings[characterId] = rating;
            _ratingResults.AddParticipant(rating);
        }

        return true;
    }

    private void OnWarmupEnded()
    {
        _ = UpdateCrpgUsersAsync(durationRewarded: 0, updateUserStats: false);
    }

    private void OnWarmupRewardTick(float durationReward)
    {
        _ = UpdateCrpgUsersAsync(durationRewarded: durationReward, durationUpkeep: 0, constantMultiplier: 2, updateUserStats: false);
    }

    private void SetRewardForConnectedPlayer(
        CrpgUserUpdate userUpdate,
        CrpgPeer crpgPeer,
        float durationRewarded,
        int compensationAmount,
        bool isValorousPlayer,
        int defenderMultiplierGain,
        int attackerMultiplierGain,
        int? constantMultiplier)
    {
        float serverXpMultiplier = CrpgServerConfiguration.ServerExperienceMultiplier;
        serverXpMultiplier *= IsHappyHour() ? 1.5f : 1f;
        userUpdate.Reward = new CrpgUserReward
        {
            Experience = (int)(serverXpMultiplier * durationRewarded * (_constants.BaseExperienceGainPerSecond
                + crpgPeer.RewardMultiplier * _constants.MultipliedExperienceGainPerSecond)),
            Gold = (int)(durationRewarded * (_constants.BaseGoldGainPerSecond
                + crpgPeer.RewardMultiplier * _constants.MultipliedGoldGainPerSecond)
                + compensationAmount),
        };

        if (constantMultiplier != null)
        {
            crpgPeer.RewardMultiplier = constantMultiplier.Value;
        }
        else if (crpgPeer.LastSpawnInfo != null)
        {
            int rewardMultiplier = crpgPeer.RewardMultiplier;
            if (crpgPeer.LastSpawnInfo.Team.Side == BattleSideEnum.Defender)
            {
                rewardMultiplier += isValorousPlayer ? Math.Max(defenderMultiplierGain, 1) : defenderMultiplierGain;
            }
            else if (crpgPeer.LastSpawnInfo.Team.Side == BattleSideEnum.Attacker)
            {
                rewardMultiplier += isValorousPlayer ? Math.Max(attackerMultiplierGain, 1) : attackerMultiplierGain;
            }

            crpgPeer.RewardMultiplier = Math.Max(Math.Min(rewardMultiplier, ExperienceMultiplierMax), ExperienceMultiplierMin);
        }
    }

    private bool IsHappyHour()
    {
        var happyHours = CrpgServerConfiguration.HappyHours;
        if (happyHours == null)
        {
            return false;
        }

        TimeSpan timeOfDay = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, happyHours.Item3).TimeOfDay;
        if (timeOfDay < happyHours.Item1 || happyHours.Item2 < timeOfDay)
        {
            if (_lastRewardDuringHappyHours)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new CrpgRewardHappyHour { Started = false });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            _lastRewardDuringHappyHours = false;
            return false;
        }

        if (!_lastRewardDuringHappyHours)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CrpgRewardHappyHour { Started = true });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        _lastRewardDuringHappyHours = true;
        return true;
    }

    /// <summary>Valorous players are the top X% of the round of the defeated team.</summary>
    private HashSet<PlayerId> GetValorousPlayers(NetworkCommunicator[] networkPeers,
        Dictionary<PlayerId, PeriodStats> allPeriodStats, BattleSideEnum valourTeamSide)
    {
        var defeatedTeamPlayersWithRoundScore = new List<(PlayerId playerId, int score)>();
        foreach (var networkPeer in networkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer == null)
            {
                continue;
            }

            if (crpgPeer.LastSpawnInfo?.Team.Side == valourTeamSide)
            {
                var playerId = networkPeer.VirtualPlayer.Id;
                int roundScore = allPeriodStats.TryGetValue(playerId, out var s) ? s.Score : 0;
                defeatedTeamPlayersWithRoundScore.Add((playerId, roundScore));
            }
        }

        int numberOfPlayersToGiveValour = (defeatedTeamPlayersWithRoundScore.Count is >= 2 and <= 5)
            ? 1
            : (int)(0.2f * defeatedTeamPlayersWithRoundScore.Count);
        Debug.Print($"Giving valour to {numberOfPlayersToGiveValour} out of the {defeatedTeamPlayersWithRoundScore.Count} players in the defeated team");
        return defeatedTeamPlayersWithRoundScore
            .OrderByDescending(p => p.score)
            .Take(numberOfPlayersToGiveValour)
            .Select(p => p.playerId)
            .ToHashSet();
    }

    private CrpgCharacterRating GetNewRating(CrpgPeer crpgPeer)
    {
        int characterId = crpgPeer.User!.Character.Id;

        if (!_characterRatings.TryGetValue(characterId, out var rating))
        {
            return crpgPeer.User!.Character.Statistics.Rating;
        }

        // Values are clamped in case there is an issue in the rating algorithm.
        return new CrpgCharacterRating
        {
            Value = MathF.Clamp(rating.Rating, -100_000, 100_000),
            Deviation = MathF.Clamp(rating.RatingDeviation, -100_000, 100_000),
            Volatility = MathF.Clamp(rating.Volatility, -100_000, 100_000),
        };
    }

    private IList<CrpgUserDamagedItem> BreakItems(CrpgPeer crpgPeer, float roundDuration)
    {
        List<CrpgUserDamagedItem> brokenItems = new();
        foreach (var equippedItem in crpgPeer.LastSpawnInfo!.EquippedItems)
        {
            var mbItem = Game.Current.ObjectManager.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            if (_random.NextDouble() >= _constants.ItemBreakChance)
            {
                continue;
            }

            int repairCost = (int)(mbItem.Value * roundDuration * _constants.ItemRepairCostPerSecond);
            brokenItems.Add(new CrpgUserDamagedItem
            {
                UserItemId = equippedItem.UserItem.Id,
                RepairCost = repairCost,
            });
        }

        return brokenItems;
    }

    private void SetStatistics(CrpgUserUpdate userUpdate, NetworkCommunicator networkPeer,
        Dictionary<PlayerId, PeriodStats> allPeriodStats)
    {
        if (allPeriodStats.TryGetValue(networkPeer.VirtualPlayer.Id, out var peerPeriodStats))
        {
            userUpdate.Statistics = new CrpgCharacterStatistics
            {
                Kills = peerPeriodStats.Kills,
                Deaths = peerPeriodStats.Deaths,
                Assists = peerPeriodStats.Assists,
                PlayTime = peerPeriodStats.PlayTime,
                Rating = userUpdate.Statistics.Rating,
            };
        }
    }

    private void SetUserAsLoading(IEnumerable<int> userIds, Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId, bool loading)
    {
        foreach (int userId in userIds)
        {
            if (crpgPeerByCrpgUserId.TryGetValue(userId, out var crpgPeer))
            {
                crpgPeer.UserLoading = loading;
            }
        }
    }

    private void SendRewardToPeers(IList<UpdateCrpgUserResult> updateResults,
        Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId, HashSet<PlayerId> valorousPlayerIds, Dictionary<int, int> compensationByCrpgUserId, bool lowPopulation, bool isDuel = false)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgPeerByCrpgUserId.TryGetValue(updateResult.User.Id, out var crpgPeer))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            var networkPeer = crpgPeer.GetNetworkPeer();
            if (!networkPeer.IsConnectionActive)
            {
                return;
            }

            crpgPeer.User = updateResult.User;
            if (crpgPeer.User.Character.ForTournament && !CrpgFeatureFlags.IsEnabled(CrpgFeatureFlags.FeatureTournament) && !isDuel)
            {
                KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "tournament_only");
                continue;
            }

            int compensationForCrpgUser = compensationByCrpgUserId.TryGetValue(crpgPeer.User.Id, out int compensation) ? compensation : 0;
            // subtracting the compensation gold that was previously added onto the reward for the API call
            // it is for clarity reasons for the message that is being sent to the user
            updateResult.EffectiveReward.Gold -= compensationForCrpgUser;
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgRewardUser
            {
                Reward = updateResult.EffectiveReward,
                Valour = valorousPlayerIds.Contains(networkPeer.VirtualPlayer.Id),
                LowPopulation = lowPopulation,
                RepairCost = updateResult.RepairedItems.Sum(r => r.RepairCost),
                Compensation = compensationForCrpgUser,
                BrokeItemIds = updateResult.RepairedItems.Where(r => r.Broke).Select(r => r.ItemId).ToList(),
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void SendDuelResultToPeers(IList<UpdateCrpgUserResult> updateResults,
        Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId, int winnerUserId)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgPeerByCrpgUserId.TryGetValue(updateResult.User.Id, out var crpgPeer))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            var networkPeer = crpgPeer.GetNetworkPeer();
            if (!networkPeer.IsConnectionActive)
            {
                return;
            }

            crpgPeer.User = updateResult.User;

            GameNetwork.BeginModuleEventAsServer(crpgPeer.GetNetworkPeer());
            GameNetwork.WriteMessage(new TrainingGroundDuelResultMessage { HasWonDuel = updateResult.User.Id == winnerUserId ? true : false, RatingChange = (int)crpgPeer.User.Character.Statistics.Rating.CompetitiveValue });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void SendErrorToPeers(Dictionary<int, CrpgPeer> crpgPeerByUserId)
    {
        foreach (var crpgPeer in crpgPeerByUserId.Values)
        {
            GameNetwork.BeginModuleEventAsServer(crpgPeer.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardError());
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void BuildResiliencePipeline()
    {
        _pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        })
        .Build();
    }
}
