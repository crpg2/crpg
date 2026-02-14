using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Terrains;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Queries;

public record GetStrategusUpdateQuery : IMediatorRequest<StrategusUpdate>
{
    public int PartyId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap, IStrategusSpeedModel strategusSpeedModel, IStrategusRouting strategusRouting) : IMediatorRequestHandler<GetStrategusUpdateQuery, StrategusUpdate>
    {
        private static readonly PartyStatus[] VisibleStatuses =
        [
            PartyStatus.Idle,
            PartyStatus.AwaitingBattleJoinDecision,
            PartyStatus.AwaitingPartyOfferDecision,
        ];

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IStrategusMap _strategusMap = strategusMap;
        private readonly IStrategusSpeedModel _strategusSpeedModel = strategusSpeedModel;
        private readonly IStrategusRouting _strategusRouting = strategusRouting;

        public async ValueTask<Result<StrategusUpdate>> Handle(GetStrategusUpdateQuery req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .AsNoTracking() // see PartiesController.UpdatePartyOrders. СCll this query once after executing the UpdatePartyOrdersCommand, we must reset the entire EF cache
                .AsSplitQuery()
                .Include(p => p.User!).ThenInclude(u => u.ClanMembership!.Clan)
                // Load mounts items to compute movement speed.
                .Include(p => p.Items.Where(oi => oi.Item!.Type == ItemType.Mount)).ThenInclude(oi => oi.Item)
                .Include(p => p.CurrentParty!.User)
                .Include(p => p.CurrentSettlement)
                .Include(p => p.CurrentBattle)
                .Include(p => p.Orders).ThenInclude(o => o.TargetedBattle)
                .Include(p => p.Orders).ThenInclude(o => o.TargetedSettlement)
                .Include(p => p.Orders).ThenInclude(o => o.TargetedParty!.User)
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var visibleParties = await _db.Parties
                .Where(h => h.Id != party.Id
                            && h.Position.IsWithinDistance(party.Position, _strategusMap.ViewDistance)
                            && VisibleStatuses.Contains(h.Status))
                .ProjectTo<PartyVisibleViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            var visibleSettlements = await _db.Settlements
                .Where(s => s.Position.IsWithinDistance(party.Position, _strategusMap.ViewDistance))
                .ProjectTo<SettlementPublicViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            var visibleBattles = await _db.Battles
                .Where(b => b.Position.IsWithinDistance(party.Position, _strategusMap.ViewDistance)
                            && b.Phase != BattlePhase.End)
                .ProjectTo<BattleViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            double speed = _strategusSpeedModel.ComputePartySpeed(party);
            var terrains = await _db.Terrains.ToArrayAsync(cancellationToken);

            var partyVm = _mapper.Map<PartyViewModel>(party);
            partyVm.Speed = speed;

            // TODO: refactoring
            Point currentPosition = party.Position;
            foreach (var orderVm in partyVm.Orders)
            {
                orderVm.PathSegments = BuildOrderPathSegments(currentPosition, orderVm, terrains, speed);

                // Update current position to the end of this order for the next order in the queue
                if (orderVm.PathSegments.Count > 0)
                {
                    currentPosition = orderVm.PathSegments[^1].EndPoint;
                }

                if (orderVm.Type == PartyOrderType.JoinBattle && orderVm.TargetedBattle != null)
                {
                    var battleJoinIntents = await _db.BattleFighterApplications
                        .Where(bfa => bfa.PartyId == party.Id && bfa.BattleId == orderVm.TargetedBattle.Id && bfa.Status == BattleFighterApplicationStatus.Intent)
                        .ToArrayAsync(cancellationToken);

                    foreach (var battleJoinIntent in battleJoinIntents)
                    {
                        orderVm.BattleJoinIntents.Add(new BattleJoinIntentViewModel() { Side = battleJoinIntent.Side, });
                    }
                }

                // TODO: в 2х местах у меня TransferOffers, мб убрать из приказов
                if (orderVm.Type == PartyOrderType.TransferOfferParty && orderVm.TargetedParty != null)
                {
                    var partyTransferOffer = await _db.PartyTransferOffers
                        .Include(to => to.Items).ThenInclude(i => i.Item)
                        .FirstOrDefaultAsync(to => to.PartyId == party.Id && to.TargetPartyId == orderVm.TargetedParty.Id && to.Status == PartyTransferOfferStatus.Intent, cancellationToken);

                    orderVm.TransferOfferPartyIntent = partyTransferOffer != null ? _mapper.Map<PartyTransferOfferViewModel>(partyTransferOffer) : null;
                }
            }

            // TODO: в 2х местах у меня TransferOffers
            var partyTransferOffers = await _db.PartyTransferOffers
                .AsSplitQuery()
                .Include(to => to.Items).ThenInclude(oi => oi.Item)
                .Include(to => to.TargetParty).ThenInclude(p => p!.User)
                .Include(to => to.Party).ThenInclude(p => p!.User)
                .Where(to => to.PartyId == party.Id || to.TargetPartyId == party.Id)
                .ToListAsync(cancellationToken);

            partyVm.CurrentTransferOffers.AddRange(_mapper.Map<List<PartyTransferOfferViewModel>>(partyTransferOffers));

            return new(new StrategusUpdate
            {
                Party = partyVm,
                VisibleParties = visibleParties,
                VisibleSettlements = visibleSettlements,
                VisibleBattles = visibleBattles,
            });
        }

        private static List<Point> GetOrderPoints(Point start, PartyOrderViewModel orderVm)
        {
            var points = new List<Point> { start };

            switch (orderVm.Type)
            {
                case PartyOrderType.MoveToPoint:
                    if (orderVm.Waypoints != null && orderVm.Waypoints.Count > 0)
                    {
                        points.AddRange(orderVm.Waypoints.Geometries.Cast<Point>());
                    }

                    break;

                case PartyOrderType.FollowParty:
                case PartyOrderType.AttackParty:
                case PartyOrderType.TransferOfferParty:
                    if (orderVm.TargetedParty != null)
                    {
                        points.Add(orderVm.TargetedParty.Position);
                    }

                    break;

                case PartyOrderType.MoveToSettlement:
                case PartyOrderType.AttackSettlement:
                    if (orderVm.TargetedSettlement != null)
                    {
                        points.Add(orderVm.TargetedSettlement.Position);
                    }

                    break;

                case PartyOrderType.JoinBattle:
                    if (orderVm.TargetedBattle != null)
                    {
                        points.Add(orderVm.TargetedBattle.Position);
                    }

                    break;
            }

            return points;
        }

        private List<PartyOrderPathSegmentViewModel> BuildOrderPathSegments(Point start, PartyOrderViewModel orderVm, Terrain[] terrains, double baseSpeed)
        {
            var points = GetOrderPoints(start, orderVm);
            if (points.Count < 2)
            {
                return [];
            }

            var segments = new List<PartyOrderPathSegmentViewModel>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                var pathSegments = _strategusRouting.BuildPathSegments(points[i], points[i + 1], terrains);
                foreach (var segment in pathSegments)
                {
                    double distance = segment.StartPoint.Distance(segment.EndPoint);
                    double speedMultiplier = segment.TerrainMultiplier;
                    double speed = baseSpeed * speedMultiplier;
                    segments.Add(new PartyOrderPathSegmentViewModel
                    {
                        StartPoint = segment.StartPoint,
                        EndPoint = segment.EndPoint,
                        Distance = distance,
                        SpeedMultiplier = speedMultiplier,
                        Speed = speed,
                    });
                }
            }

            return segments;
        }
    }
}
