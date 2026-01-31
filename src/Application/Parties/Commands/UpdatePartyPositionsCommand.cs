using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
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
                .Where(p => p.Orders.Any())
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedBattle)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedSettlement)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedParty)
                // Load mounts items to compute movement speed.
                .Include(p => p.Items.Where(oi => oi.Item!.Type == ItemType.Mount)).ThenInclude(oi => oi.Item)
                .ToArrayAsync(cancellationToken);

            foreach (var party in parties)
            {
                if (party.Orders.Count == 0)
                {
                    continue;
                }

                double speed = _strategusSpeedModel.ComputePartySpeed(party);
                double remainingDistance = speed * req.DeltaTime.TotalSeconds;

                while (remainingDistance > 0 && party.Orders.Count != 0)
                {
                    var order = party.Orders.OrderBy(o => o.OrderIndex).First();

                    remainingDistance = order.Type switch
                    {
                        PartyOrderType.MoveToPoint =>
                            MoveToPoint(party, order, remainingDistance),

                        PartyOrderType.FollowParty or PartyOrderType.AttackParty =>
                            MoveToParty(party, order, remainingDistance),

                        PartyOrderType.MoveToSettlement or PartyOrderType.AttackSettlement =>
                            await MoveToSettlement(party, order, remainingDistance, cancellationToken),

                        PartyOrderType.JoinBattle =>
                            await MoveToBattle(party, order, remainingDistance, cancellationToken),

                        _ => remainingDistance,
                    };
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }

        private static Point GetMidPoint(Point pointA, Point pointB)
        {
            return new((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2);
        }

        private double MoveToPoint(Party party, PartyOrder order, double remainingDistance)
        {
            if (order.Waypoints.IsEmpty)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without any points to go to",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingDistance;
                // return;
            }

            var waypoints = order.Waypoints.Cast<Point>().ToList();
            remainingDistance = MoveAlongWaypoints(party, waypoints, remainingDistance);
            order.Waypoints = new MultiPoint([.. waypoints]);

            if (order.Waypoints.Count == 0)
            {
                // party.Status = PartyStatus.Idle; // TODO: FIXME:
                party.Orders.Remove(order);
                // TODO: next order
            }

            return remainingDistance;

            // var targetPoint = (Point)order.Waypoints[0];
            // if (!MovePartyTowardsPoint(party, targetPoint, deltaTime, false))
            // {
            //     return;
            // }

            // order.Waypoints = new MultiPoint([.. order.Waypoints.Skip(1).Cast<Point>()]);
            // if (order.Waypoints.Count == 0)
            // {
            //     party.Orders.Remove(order);
            // }
        }

        private double MoveToParty(Party party, PartyOrder order, double remainingDistance)
        {
            var target = order.TargetedParty;
            if (target == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target party",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingDistance;
            }

            if (!party.Position.IsWithinDistance(target.Position, _strategusMap.ViewDistance))
            {
                // Followed party is not in sight anymore. Stop.
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingDistance;
            }

            // TODO: FIXME: кейс, когда на Party, за которой мы следуем или атакуем - напали
            if (_strategusMap.InteractionDistance <= party.Position.Distance(target.Position))
            {
                if (order.Type == PartyOrderType.AttackParty && !UnattackablePartyStatuses.Contains(target.Status))
                {
                    StartBattle(party, target);
                }

                // continue to execute the FollowParty order
                return remainingDistance;
            }

            return MoveTowards(party, target.Position, remainingDistance);
        }

        private async Task<double> MoveToSettlement(Party party, PartyOrder order, double remainingDistance, CancellationToken cancellationToken)
        {
            var target = order.TargetedSettlement;
            if (target == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target settlement",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle;
                party.Orders.Remove(order);
                return remainingDistance;
            }

            if (_strategusMap.InteractionDistance <= party.Position.Distance(target.Position))
            {
                if (order.Type == PartyOrderType.MoveToSettlement)
                {
                    party.Status = PartyStatus.IdleInSettlement;
                }
                else
                {
                    await StartSettlementBattle(party, target, cancellationToken);
                }

                party.Position = target.Position;
                party.Orders.Remove(order);
                return remainingDistance;
            }

            return MoveTowards(party, target.Position, remainingDistance);
        }

        private async Task<double> MoveToBattle(Party party, PartyOrder order, double remainingDistance, CancellationToken cancellationToken)
        {
            var target = order.TargetedBattle;
            if (target == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target battle",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingDistance;
            }

            if (!party.Position.IsWithinDistance(target.Position, _strategusMap.ViewDistance))
            {
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingDistance;
            }

            if (_strategusMap.InteractionDistance <= party.Position.Distance(target.Position))
            {
                party.Status = PartyStatus.AwaitingBattleJoinDecision;
                party.Position = target.Position;
                party.CurrentBattle = target;

                var intentBattleFighterApplications = await _db.BattleFighterApplications
                                .Where(bfa => bfa.BattleId == target.Id && bfa.PartyId == party.Id)
                                .ToListAsync(cancellationToken);
                intentBattleFighterApplications.ForEach(bfa => bfa.Status = BattleFighterApplicationStatus.Pending);

                party.Orders.Remove(order);

                return remainingDistance;
            }

            return MoveTowards(party, target.Position, remainingDistance);
        }

        private double MoveAlongWaypoints(Party party, List<Point> waypoints, double remainingDistance)
        {
            int i = 0;

            while (remainingDistance > 0 && i < waypoints.Count)
            {
                var target = waypoints[i];
                double dist = party.Position.Distance(target);

                if (dist <= remainingDistance)
                {
                    party.Position = (Point)target.Copy();
                    remainingDistance -= dist;
                    i++;
                }
                else
                {
                    party.Position = _strategusMap.MovePointTowards(party.Position, target, remainingDistance);
                    remainingDistance = 0;
                }
            }

            // удаляем достигнутые точки
            for (int j = 0; j < i; j++)
            {
                waypoints.RemoveAt(0);
            }

            return remainingDistance;
        }

        private double MoveTowards(Party party, Point target, double remainingDistance)
        {
            double dist = party.Position.Distance(target);

            if (dist > remainingDistance)
            {
                party.Position = _strategusMap.MovePointTowards(
                    party.Position,
                    target,
                    remainingDistance);
                return 0;
            }

            party.Position = (Point)target.Copy();
            return remainingDistance - dist;
        }

        private Task StartBattle(Party attacker, Party defender)
        {
            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Region = defender.User!.Region, // Region of the defender
                Position = GetMidPoint(attacker.Position, defender.Position),
                Fighters =
                    {
                        new BattleFighter
                        {
                            Party = attacker,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Party = defender,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                // TODO: FIXME: add BattleParticipant in UpdateBattlePhasesCommand BattlePhase.Preparation -> BattlePhase.Hiring
            };

            attacker.Status = PartyStatus.InBattle;
            attacker.CurrentBattle = battle;

            defender.Status = PartyStatus.InBattle;
            defender.CurrentBattle = battle;

            // TODO:FIXME:
            // There may be many cases, so it is necessary to check
            // for example, if the defender was going to a battle, it is necessary to delete his battleFighterApplication in the Intent status.
            attacker.Orders.Clear();
            defender.Orders.Clear();

            _db.Battles.Add(battle);
            Logger.LogInformation("Party '{0}' initiated a battle against party '{1}'",
                attacker.Id, defender.Id);
            return Task.CompletedTask;
        }

        private async Task StartSettlementBattle(Party party, Settlement settlement, CancellationToken cancellationToken)
        {
            bool attackInProgress = await _db.Battles
                .AnyAsync(b => b.Phase != BattlePhase.End && b.Fighters.Any(f => f.SettlementId == settlement.Id), cancellationToken);
            if (attackInProgress)
            {
                return;
            }

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Region = settlement.Region, // Region of the defender
                Position = GetMidPoint(party.Position, settlement.Position),
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
                            Settlement = settlement,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                // TODO: FIXME: add BattleParticipant in UpdateBattlePhasesCommand BattlePhase.Preparation -> BattlePhase.Hiring
            };

            _db.Battles.Add(battle);
            Logger.LogInformation("Party '{0}' initiated a battle against settlement '{1}'",
                party.Id, settlement.Id);
        }
    }
}
