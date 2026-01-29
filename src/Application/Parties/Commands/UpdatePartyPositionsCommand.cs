using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyPositionsCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler(ICrpgDbContext db, IStrategusMap strategusMap, IStrategusSpeedModel strategusSpeedModel) : IMediatorRequestHandler<UpdatePartyPositionsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdatePartyPositionsCommand>();

        // private static readonly PartyStatus[] MovementStatuses =
        // [
        //     PartyStatus.MovingToPoint,
        //     PartyStatus.FollowingParty,
        //     PartyStatus.MovingToSettlement,
        //     PartyStatus.MovingToAttackParty,
        //     PartyStatus.MovingToAttackSettlement,
        //     PartyStatus.MovingToBattle,
        // ];

        private static readonly PartyStatus[] UnattackablePartyStatuses =
        [
            PartyStatus.IdleInSettlement,
            PartyStatus.RecruitingInSettlement,
            PartyStatus.InBattle,
        ];

        private readonly ICrpgDbContext _db = db;
        private readonly IStrategusMap _strategusMap = strategusMap;
        private readonly IStrategusSpeedModel _strategusSpeedModel = strategusSpeedModel;

        public async Task<Result> Handle(UpdatePartyPositionsCommand req, CancellationToken cancellationToken)
        {
            var parties = await _db.Parties
                .AsSplitQuery()
                // .Where(p => MovementStatuses.Contains(p.Status)) // TODO:
                .Include(p => p.Orders)
                .Include(p => p.TargetedParty).ThenInclude(p => p!.User)
                .Include(p => p.TargetedSettlement)
                .Include(p => p.TargetedBattle)
                .Include(p => p.BattleJoinIntents)
                // Load mounts items to compute movement speed.
                .Include(p => p.Items.Where(oi => oi.Item!.Type == ItemType.Mount)).ThenInclude(oi => oi.Item)
                .ToArrayAsync(cancellationToken);

            foreach (var party in parties)
            {
                if (party.Orders.Count == 0)
                {
                    continue;
                }

                var order = party.Orders.OrderBy(o => o.OrderIndex).First();

                // switch (order.Type)
                // {
                //     case PartyOrderType.MoveToPoint:
                //         MoveToPoint(req.DeltaTime, party, order);
                //         break;
                //     case PartyOrderType.FollowParty:
                //     case PartyOrderType.AttackParty:
                //         MoveToParty(req.DeltaTime, party, order);
                //         break;
                //     case PartyOrderType.MoveToSettlement:
                //     case PartyOrderType.AttackSettlement:
                //         await MoveToSettlement(req.DeltaTime, party, order, cancellationToken);
                //         break;
                //     case PartyOrderType.JoinBattle:
                //         MoveToBattle(req.DeltaTime, party, order);
                //         break;
                // }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }

        private static Point GetMidPoint(Point pointA, Point pointB)
        {
            return new((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2);
        }

        private void MoveToPoint(TimeSpan deltaTime, Party party, PartyOrder order)
        {
            // TODO: to fluent validator
            if (order.Waypoints.IsEmpty)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without any points to go to",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return;
            }

            var targetPoint = (Point)order.Waypoints[0];
            if (!MovePartyTowardsPoint(party, targetPoint, deltaTime, false))
            {
                return;
            }

            order.Waypoints = new MultiPoint([.. order.Waypoints.Skip(1).Cast<Point>()]);
            if (order.Waypoints.Count == 0)
            {
                // party.Status = PartyStatus.Idle; // TODO: FIXME:
                party.Orders.Remove(order);
            }
        }

        private void MoveToParty(TimeSpan deltaTime, Party party, PartyOrder order)
        {
            if (order.TargetedParty == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target party",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return;
            }

            if (!party.Position.IsWithinDistance(order.TargetedParty.Position, _strategusMap.ViewDistance))
            {
                // Followed party is not in sight anymore. Stop.
                // party.Status = PartyStatus.Idle; // TODO:
                // party.TargetedParty = null;
                party.Orders.Remove(order);
                return;
            }

            if (order.Type == PartyOrderType.FollowParty)
            {
                // Set canInteractWithTarget to false to the party doesn't stop to interaction range
                // but stops on the target party itself.
                MovePartyTowardsPoint(party, order.TargetedParty.Position, deltaTime, false);
            }
            else if (order.Type == PartyOrderType.AttackParty)
            {
                if (!MovePartyTowardsPoint(party, order.TargetedParty.Position, deltaTime, true))
                {
                    return;
                }

                if (UnattackablePartyStatuses.Contains(order.TargetedParty.Status))
                {
                    return;
                }

                Battle battle = new()
                {
                    Phase = BattlePhase.Preparation,
                    Region = order.TargetedParty.User!.Region, // Region of the defender.
                    Position = GetMidPoint(party.Position, order.TargetedParty.Position),
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Party = party,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Party = order.TargetedParty,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                    // Participants =
                    // {
                    //     new BattleParticipant
                    //     {
                    //         Character = // TODO: FIXME:
                    //         Side = BattleSide.Attacker,
                    //         Type = BattleParticipantType.Party,
                    //     },
                    //     ...
                    // },
                };

                // TODO: FIXME: проверить
                party.Status = PartyStatus.InBattle;
                // TODO: FIXME: Добавить в party currentBattle, currentSettlement, currentParty
                // party.TargetedBattle = battle;
                // party.TargetedSettlementId = null;
                // party.TargetedPartyId = null;

                order.TargetedParty.Status = PartyStatus.InBattle;
                // party.TargetedParty.TargetedBattle = battle;
                // party.TargetedParty.TargetedSettlementId = null;
                // party.TargetedParty.TargetedPartyId = null;

                _db.Battles.Add(battle);
                Logger.LogInformation("Party '{0}' initiated a battle against party '{1}'",
                    party.Id, order.TargetedPartyId);

                // party.TargetedParty.TargetedPartyId = null;
                party.Orders.Remove(order);
            }
        }

        private async Task MoveToSettlement(TimeSpan deltaTime, Party party, PartyOrder order, CancellationToken cancellationToken)
        {
            if (order.TargetedSettlement == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target settlement",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle;
                party.Orders.Remove(order);
                return;
            }

            if (!MovePartyTowardsPoint(party, order.TargetedSettlement.Position, deltaTime, true))
            {
                return;
            }

            if (order.Type == PartyOrderType.MoveToSettlement)
            {
                party.Status = PartyStatus.IdleInSettlement;
                party.Position = order.TargetedSettlement.Position;
            }
            else if (order.Type == PartyOrderType.AttackSettlement)
            {
                bool attackInProgress = await _db.Battles
                    .AnyAsync(b =>
                            b.Phase != BattlePhase.End
                            && b.Fighters.Any(f => f.SettlementId == party.TargetedSettlementId),
                        cancellationToken);
                if (attackInProgress)
                {
                    return;
                }

                party.Status = PartyStatus.InBattle;
                Battle battle = new()
                {
                    Phase = BattlePhase.Preparation,
                    Region = order.TargetedSettlement.Region, // Region of the defender.
                    Position = GetMidPoint(party.Position, order.TargetedSettlement.Position),
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Party = party,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        // TODO: FIXME: if Settlement has an owner, then he must be the commander
                        new BattleFighter
                        {
                            Settlement = order.TargetedSettlement,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                };
                _db.Battles.Add(battle);
                Logger.LogInformation("Party '{0}' initiated a battle against settlement '{1}'",
                    party.Id, order.TargetedSettlementId);
            }

            party.Orders.Remove(order);
        }

        private void MoveToBattle(TimeSpan deltaTime, Party party, PartyOrder order)
        {
            if (order.TargetedBattle == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target battle",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle;
                party.Orders.Remove(order);
                return;
            }

            if (order.BattleJoinIntents.Count == 0)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without battle join intents",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle;
                party.Orders.Remove(order);
                return;
            }

            if (!MovePartyTowardsPoint(party, order.TargetedBattle.Position, deltaTime, true))
            {
                return;
            }

            foreach (var intent in party.BattleJoinIntents)
            {
                _db.BattleFighterApplications.Add(new BattleFighterApplication
                {
                    Battle = order.TargetedBattle,
                    Party = party,
                    Side = intent.Side,
                    Status = BattleFighterApplicationStatus.Pending,
                });
            }

            _db.BattleJoinIntents.RemoveRange(party.BattleJoinIntents);

            party.Status = PartyStatus.AwaitingBattleJoinDecision;
            party.Position = order.TargetedBattle.Position;

            party.Orders.Remove(order);
        }

        private bool MovePartyTowardsPoint(Party party, Point targetPoint, TimeSpan deltaTime, bool canInteractWithTarget)
        {
            double speed = _strategusSpeedModel.ComputePartySpeed(party);
            double distance = speed * deltaTime.TotalSeconds;
            party.Position = _strategusMap.MovePointTowards(party.Position, targetPoint, distance);
            return canInteractWithTarget
                ? _strategusMap.ArePointsAtInteractionDistance(party.Position, targetPoint)
                : _strategusMap.ArePointsEquivalent(party.Position, targetPoint);
        }
    }
}
