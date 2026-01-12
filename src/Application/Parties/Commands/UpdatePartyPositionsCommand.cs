using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyPositionsCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler(ICrpgDbContext db, IStrategusMap strategusMap, IStrategusSpeedModel strategusSpeedModel, IStrategusRouting strategusRouting) : IMediatorRequestHandler<UpdatePartyPositionsCommand>
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
        private readonly IStrategusRouting _strategusRouting = strategusRouting;

        public async ValueTask<Result> Handle(UpdatePartyPositionsCommand req, CancellationToken cancellationToken)
        {
            var parties = await _db.Parties
                .AsSplitQuery()
                .Where(p => p.Orders.Any())
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedBattle)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedSettlement) // TODO: Settlement owner
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedParty)
                        .ThenInclude(p => p!.User)
                // Load mounts items to compute movement speed.
                .Include(p => p.Items.Where(oi => oi.Item!.Type == ItemType.Mount)).ThenInclude(oi => oi.Item)
                .ToArrayAsync(cancellationToken);

            // Load all terrains for terrain-based speed calculations
            var terrains = await _db.Terrains.ToArrayAsync(cancellationToken);

            foreach (var party in parties)
            {
                double baseSpeed = _strategusSpeedModel.ComputePartySpeed(party);
                double remainingTime = req.DeltaTime.TotalSeconds;

                while (remainingTime > 0 && party.Orders.Count != 0)
                {
                    var order = party.Orders.OrderBy(o => o.OrderIndex).First();

                    remainingTime = order.Type switch
                    {
                        PartyOrderType.MoveToPoint =>
                            MoveToPoint(party, order, baseSpeed, remainingTime, terrains),

                        PartyOrderType.FollowParty or PartyOrderType.AttackParty or PartyOrderType.TransferOfferParty =>
                            await MoveToParty(party, order, baseSpeed, remainingTime, terrains, cancellationToken),

                        PartyOrderType.MoveToSettlement or PartyOrderType.AttackSettlement =>
                            await MoveToSettlement(party, order, baseSpeed, remainingTime, terrains, cancellationToken),

                        PartyOrderType.JoinBattle =>
                            await MoveToBattle(party, order, baseSpeed, remainingTime, terrains, cancellationToken),

                        _ => remainingTime,
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

        private double MoveToPoint(Party party, PartyOrder order, double baseSpeed, double remainingTime, Terrain[] terrains)
        {
            if (order.Waypoints.IsEmpty)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without any points to go to",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingTime;
            }

            var waypoints = order.Waypoints.Cast<Point>().ToList();
            remainingTime = MoveAlongWaypoints(party, waypoints, baseSpeed, remainingTime, terrains);
            order.Waypoints = new MultiPoint([.. waypoints]);

            if (order.Waypoints.Count == 0)
            {
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
            }

            return remainingTime;
        }

        private async Task<double> MoveToParty(Party party, PartyOrder order, double baseSpeed, double remainingTime, Terrain[] terrains, CancellationToken cancellationToken)
        {
            var target = order.TargetedParty;
            if (target == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target party",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingTime;
            }

            if (!party.Position.IsWithinDistance(target.Position, _strategusMap.ViewDistance))
            {
                // Followed party is not in sight anymore. Stop.
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingTime;
            }

            // TODO: FIXME: обработать кейс, когда на Party, за которой мы следуем или атакуем - напали

            if (order.Type == PartyOrderType.FollowParty)
            {
                MoveTowardsWithTerrainSegmentation(party, target.Position, baseSpeed, remainingTime, terrains, out _);
                return 0; // to avoid an endless while
            }

            double interactionDist = _strategusMap.InteractionDistance;
            double dist = party.Position.Distance(target.Position);

            if (dist <= interactionDist)
            {
                if (order.Type == PartyOrderType.AttackParty && !UnattackablePartyStatuses.Contains(target.Status))
                {
                    await StartBattle(party, target);
                }

                if (order.Type == PartyOrderType.TransferOfferParty)
                {
                    // TODO: move to startOffer
                    var partyTransferOffer = await _db.PartyTransferOffers
                            .FirstOrDefaultAsync(to => to.PartyId == party.Id && to.TargetPartyId == target.Id, cancellationToken);

                    if (partyTransferOffer == null)
                    {
                        Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without partyTransferOffer",
                        party.Id, order.Type);
                    }
                    else
                    {
                        partyTransferOffer.Status = PartyTransferOfferStatus.Pending;
                        party.Status = PartyStatus.AwaitingPartyOfferDecision;
                        party.CurrentParty = target;
                        party.Orders.Clear();
                    }
                }

                return remainingTime;
            }

            return MoveTowardsWithTerrainSegmentation(party, target.Position, baseSpeed, remainingTime, terrains, out _);
        }

        private async Task<double> MoveToSettlement(Party party, PartyOrder order, double baseSpeed, double remainingTime, Terrain[] terrains, CancellationToken cancellationToken)
        {
            var target = order.TargetedSettlement;
            if (target == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target settlement",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle;
                party.Orders.Remove(order);
                return remainingTime;
            }

            double interactionDist = _strategusMap.InteractionDistance;
            double dist = party.Position.Distance(target.Position);

            if (dist <= interactionDist)
            {
                if (order.Type == PartyOrderType.MoveToSettlement)
                {
                    party.Status = PartyStatus.IdleInSettlement;
                    party.CurrentSettlement = target;
                }
                else
                {
                    await StartSettlementBattle(party, target, cancellationToken);
                }

                party.Position = target.Position;
                party.Orders.Remove(order);
                return remainingTime;
            }

            return MoveTowardsWithTerrainSegmentation(party, target.Position, baseSpeed, remainingTime, terrains, out _);
        }

        private async Task<double> MoveToBattle(Party party, PartyOrder order, double baseSpeed, double remainingTime, Terrain[] terrains, CancellationToken cancellationToken)
        {
            var target = order.TargetedBattle;
            if (target == null)
            {
                Logger.LogWarning("Party '{partyId}' was in order type '{orderType}' without target battle",
                    party.Id, order.Type);
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingTime;
            }

            double interactionDist = _strategusMap.InteractionDistance;
            double dist = party.Position.Distance(target.Position);

            if (dist <= interactionDist)
            {
                // party.Status = PartyStatus.Idle; // TODO:
                party.Orders.Remove(order);
                return remainingTime;
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

                return remainingTime;
            }

            return MoveTowardsWithTerrainSegmentation(party, target.Position, baseSpeed, remainingTime, terrains, out _);
        }

        private double MoveAlongWaypoints(Party party, List<Point> waypoints, double baseSpeed, double remainingTime, Terrain[] terrains)
        {
            int i = 0;

            while (remainingTime > 0 && i < waypoints.Count)
            {
                var target = waypoints[i];
                remainingTime = MoveTowardsWithTerrainSegmentation(party, target, baseSpeed, remainingTime, terrains, out bool reachedTarget);

                if (reachedTarget)
                {
                    i++;
                }
                else
                {
                    // Didn't reach the target, no time left
                    break;
                }
            }

            // Remove the points reached
            for (int j = 0; j < i; j++)
            {
                waypoints.RemoveAt(0);
            }

            return remainingTime;
        }

        /// <summary>
        /// Moves party towards target, segmenting the path by terrain boundaries.
        /// </summary>
        private double MoveTowardsWithTerrainSegmentation(Party party, Point target, double baseSpeed, double remainingTime, Terrain[] terrains, out bool reachedTarget)
        {
            reachedTarget = false;
            double totalDistance = party.Position.Distance(target);

            if (totalDistance < 1e-10)
            {
                reachedTarget = true;
                return remainingTime;
            }

            // Find all intersection points with terrain boundaries
            var segments = _strategusRouting.BuildPathSegments(party.Position, target, terrains);

            // Move through segments
            Point currentPosition = party.Position;
            foreach (var segment in segments)
            {
                double segmentDistance = currentPosition.Distance(segment.EndPoint);
                double terrainMultiplier = segment.TerrainMultiplier;
                double currentSpeed = baseSpeed * terrainMultiplier;
                double maxDistanceWithCurrentTime = currentSpeed * remainingTime;

                if (segmentDistance <= maxDistanceWithCurrentTime)
                {
                    // Can complete this segment
                    double timeSpent = segmentDistance / currentSpeed;
                    remainingTime -= timeSpent;
                    currentPosition = segment.EndPoint;
                    party.Position = segment.EndPoint;
                }
                else
                {
                    // Can only partially complete this segment
                    party.Position = _strategusMap.MovePointTowards(currentPosition, segment.EndPoint, maxDistanceWithCurrentTime);
                    remainingTime = 0;
                    return remainingTime;
                }
            }

            reachedTarget = true;
            return remainingTime;
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
