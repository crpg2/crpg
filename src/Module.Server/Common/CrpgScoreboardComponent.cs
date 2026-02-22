using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
#if CRPG_SERVER
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Helpers;
using Crpg.Module.Rating;
using NetworkMessages.FromServer;
#endif

namespace Crpg.Module.Common;

internal class CrpgScoreboardComponent : MissionScoreboardComponent
{
    private const int ProximityScoreRange = 8;
    private const float ProximityScoreMultiplier = 0.15f;

    public CrpgScoreboardComponent(IScoreboardData scoreboardData)
        : base(scoreboardData)
    {
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
#if CRPG_SERVER
        if (damagedHp < 0 || collisionData.InflictedDamage < 0) // Happens when the damage is disabled on round end.
        {
            return;
        }

        Agent affectorCharacterAgent = affectorAgent.IsMount ? affectorAgent.RiderAgent : affectorAgent;
        Agent affectedCharacterAgent = affectedAgent.IsMount ? affectedAgent.RiderAgent : affectedAgent;
        if (affectedCharacterAgent == null)
        {
            return;
        }

        float ratingFactor;
        var affectedCrpgUser = affectedCharacterAgent.MissionPeer?.GetComponent<CrpgPeer>()?.User;
        if (affectedCrpgUser == null)
        {
            // The affected agent is probably a bot.
            ratingFactor = 0.8f;
        }
        else
        {
            CrpgCharacterRating rating = affectedCrpgUser.Character.Statistics.Rating;
            double ratingWithUncertainty = 0.01 * (rating.Value - 2 * rating.Deviation);
            float competitiveRating = (float)(ratingWithUncertainty < 0
                ? 0.03f * -Math.Pow(-ratingWithUncertainty, 3.98)
                : 0.03f * Math.Pow(ratingWithUncertainty, 3.98));
            const float ratingToScoreScaler = 1 / 1250f;
            ratingFactor = competitiveRating * ratingToScoreScaler * CrpgRatingHelper.ComputeRegionRatingPenalty(affectedCrpgUser.Region);
        }

        float score = damagedHp / affectedCharacterAgent.BaseHealthLimit * 100f * ratingFactor;
        if (isBlocked)
        {
            if (!collisionData.AttackBlockedWithShield)
            {
                return;
            }

            score = (collisionData.InflictedDamage + collisionData.AbsorbedByArmor) / 12.5f;
        }
        else if (affectedAgent.IsMount)
        {
            score = damagedHp / affectedAgent.BaseHealthLimit * 70f * ratingFactor;
        }

        if (affectorCharacterAgent == affectedCharacterAgent)
        {
            return;
        }

        if (isSiegeEngineHit)
        {
            score *= 0.2f;
        }

        var affectorMissionPeer = affectorCharacterAgent?.MissionPeer;
        if (affectorMissionPeer != null)
        {
            score = affectorCharacterAgent!.IsFriendOf(affectedCharacterAgent) ? score * -1f : score;
            ReflectionHelper.SetProperty(
                affectorMissionPeer,
                nameof(affectorMissionPeer.Score),
                (int)(affectorMissionPeer.Score + score));

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new KillDeathCountChange(affectorMissionPeer.GetNetworkPeer(),
                null, affectorMissionPeer.KillCount, affectorMissionPeer.AssistCount, affectorMissionPeer.DeathCount,
                affectorMissionPeer.Score));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

            if (score > 0f)
            {
                foreach (var agent in EnumerateAgentsAroundScorer(affectorAgent))
                {
                    if (agent != affectorAgent)
                    {
                        if (agent.Team == affectorAgent.Team)
                        {
                            var agentMissionPeer = agent.MissionPeer;
                            if (agentMissionPeer != null)
                            {
                                ReflectionHelper.SetProperty(
                                    agentMissionPeer,
                                    nameof(agentMissionPeer.Score),
                                    (int)(agentMissionPeer.Score + score * ProximityScoreMultiplier));

                                GameNetwork.BeginBroadcastModuleEvent();
                                GameNetwork.WriteMessage(new KillDeathCountChange(agentMissionPeer.GetNetworkPeer(),
                                    null, agentMissionPeer.KillCount, agentMissionPeer.AssistCount, agentMissionPeer.DeathCount,
                                    agentMissionPeer.Score));
                                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                            }
                        }
                    }
                }
            }
        }
#endif
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            // Not the best place to register to that event but heh.
            registerer.Register<UpdateRoundSpawnCount>(OnUpdateRoundSpawnCount);
        }
    }

    private void OnUpdateRoundSpawnCount(UpdateRoundSpawnCount message)
    {
        var missionPeer = message.Peer.GetComponent<MissionPeer>();
        if (missionPeer != null)
        {
            missionPeer.SpawnCountThisRound = message.SpawnCount;
        }
    }

    private IEnumerable<Agent> EnumerateAgentsAroundScorer(Agent agent)
    {
        var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, agent.Position.AsVec2, ProximityScoreRange);
        for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
        {
            yield return proximitySearch.LastFoundAgent;
        }
    }
}
