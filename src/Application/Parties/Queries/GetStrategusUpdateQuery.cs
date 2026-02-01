using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Queries;

public record GetStrategusUpdateQuery : IMediatorRequest<StrategusUpdate>
{
    public int PartyId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap, IStrategusSpeedModel strategusSpeedModel) : IMediatorRequestHandler<GetStrategusUpdateQuery, StrategusUpdate>
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

        public async Task<Result<StrategusUpdate>> Handle(GetStrategusUpdateQuery req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
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

            var partyVm = _mapper.Map<PartyViewModel>(party);
            partyVm.Speed = speed;

            // TODO: refactoring
            foreach (var orderVm in partyVm.Orders)
            {
                double distance = orderVm.Type switch
                {
                    PartyOrderType.MoveToPoint =>
                        CalculateWaypointsDistance(party.Position, orderVm.Waypoints),

                    PartyOrderType.FollowParty or PartyOrderType.AttackParty =>
                        orderVm.TargetedParty != null
                            ? party.Position.Distance(orderVm.TargetedParty.Position)
                            : 0,
                    PartyOrderType.MoveToSettlement or PartyOrderType.AttackSettlement =>
                        orderVm.TargetedSettlement != null
                            ? party.Position.Distance(orderVm.TargetedSettlement.Position)
                            : 0,
                    PartyOrderType.JoinBattle =>
                        orderVm.TargetedBattle != null
                            ? party.Position.Distance(orderVm.TargetedBattle.Position)
                            : 0,
                    _ => 0,
                };

                orderVm.Distance = distance;

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

                // TODO: в 2х местах у меня CurrentTransferOffers
                if (orderVm.Type == PartyOrderType.TransferOfferParty && orderVm.TargetedParty != null)
                {
                    var partyTransferOffer = await _db.PartyTransferOffers
                        .FirstOrDefaultAsync(to => to.PartyId == party.Id && to.TargetPartyId == orderVm.TargetedParty.Id && to.Status == PartyTransferOfferStatus.Intent, cancellationToken);

                    orderVm.TransferOfferPartyIntent = partyTransferOffer != null ? _mapper.Map<PartyTransferOfferViewModel>(partyTransferOffer) : null;
                }
            }

            // TODO: в 2х местах у меня CurrentTransferOffers
            var partyTransferOffers = await db.PartyTransferOffers
                .AsSplitQuery()
                .Include(to => to.Items).ThenInclude(oi => oi.Item)
                .Include(to => to.TargetParty).ThenInclude(p => p!.User)
                .Include(to => to.Party).ThenInclude(p => p!.User)
                .Where(to => to.PartyId == party.Id || to.TargetPartyId == party.Id)
                // .ProjectTo<PartyTransferOfferViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var partyTransferOffer in partyTransferOffers)
            {
                partyVm.CurrentTransferOffers.Add(new()
                {
                    Party = _mapper.Map<PartyVisibleViewModel>(partyTransferOffer.Party),
                    TargetParty = _mapper.Map<PartyVisibleViewModel>(partyTransferOffer.TargetParty),
                    Gold = partyTransferOffer.Gold,
                    Status = partyTransferOffer.Status,
                    Troops = partyTransferOffer.Troops,
                    // Items = partyTransferOffer.Items,
                    Items = [],
                });
            }

            return new(new StrategusUpdate
            {
                // Party = _mapper.Map<PartyViewModel>(party),
                Party = partyVm,
                VisibleParties = visibleParties,
                VisibleSettlements = visibleSettlements,
                VisibleBattles = visibleBattles,
            });
        }

        // TODO: refactoring, to service
        private static double CalculateWaypointsDistance(Point start, MultiPoint waypoints)
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                return 0;
            }

            double total = 0;
            var points = waypoints.Geometries
                .Cast<Point>()
                .Prepend(start)
                .ToArray();

            for (int i = 0; i < points.Length - 1; i++)
            {
                total += points[i].Distance(points[i + 1]);
            }

            return total;
        }
    }
}
