using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyOrdersCommand : IMediatorRequest<PartyViewModel>
{
    public record TransferOfferPartyItem
    {
        public string ItemId { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public record TransferOfferPartyIntent
    {
        public int Gold { get; set; }
        public float Troops { get; set; }
        public List<TransferOfferPartyItem> Items { get; set; } = [];
    }

    public record PartyOrderCommandItemDto
    {
        public PartyOrderType Type { get; init; }
        public int OrderIndex { get; set; }
        public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
        public int TargetedPartyId { get; init; }
        public int TargetedSettlementId { get; init; }
        public int TargetedBattleId { get; init; }
        public BattleJoinIntentViewModel[] BattleJoinIntents { get; init; } = [];
        [JsonRequired]
        public TransferOfferPartyIntent? TransferOfferPartyIntent { get; init; }
    }

    [JsonIgnore]
    public int PartyId { get; set; }
    public PartyOrderCommandItemDto[] Orders { get; init; } = [];

    public class Validator : AbstractValidator<UpdatePartyOrdersCommand>
    {
        public Validator()
        {
            RuleForEach(c => c.Orders).SetValidator(new PartyOrderValidator());
        }
    }

    public class PartyOrderValidator : AbstractValidator<PartyOrderCommandItemDto>
    {
        public PartyOrderValidator()
        {
            RuleFor(o => o.Type).IsInEnum();
            When(o => o.Type == PartyOrderType.MoveToPoint, () =>
            {
                RuleFor(o => o.Waypoints)
                    .Must(wp => wp != null && wp.Count > 0)
                    .WithMessage("MoveToPoint requires waypoints");
            });
            When(o => o.Type == PartyOrderType.FollowParty || o.Type == PartyOrderType.AttackParty, () =>
            {
                RuleFor(o => o.TargetedPartyId)
                    .GreaterThan(0)
                    .WithMessage("TargetedPartyId is required");
            });
            When(o => o.Type == PartyOrderType.MoveToSettlement || o.Type == PartyOrderType.AttackSettlement, () =>
            {
                RuleFor(o => o.TargetedSettlementId)
                    .GreaterThan(0)
                    .WithMessage("TargetedSettlementId is required");
            });
            When(o => o.Type == PartyOrderType.JoinBattle, () =>
                {
                    RuleFor(o => o.TargetedBattleId)
                        .GreaterThan(0)
                        .WithMessage("TargetedBattleId is required");
                    RuleFor(o => o.BattleJoinIntents)
                        .NotEmpty()
                        .WithMessage("BattleJoinIntents is required");
                });
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap) : IMediatorRequestHandler<UpdatePartyOrdersCommand, PartyViewModel>
    {
        // private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdatePartyOrdersCommand>();
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IStrategusMap _strategusMap = strategusMap;

        public async Task<Result<PartyViewModel>> Handle(UpdatePartyOrdersCommand req, CancellationToken cancellationToken)
        {
            // TODO: FIXME:
            // TODO: FIXME:
            // TODO: FIXME:
            // TODO: FIXME: ДОБАВИТЬ ВАЛИДАЦИЮ - есть конечные приказы, например "атаковать город/отряд", после них нельзя добавлять новые - выкидывать 400

            var party = await _db.Parties
                        .Include(p => p.User!)
                            .ThenInclude(u => u.ClanMembership!.Clan)
                        .Include(p => p.Orders)
                        .FirstOrDefaultAsync(p => p.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if (party.Status == PartyStatus.InBattle || party.Status == PartyStatus.AwaitingBattleJoinDecision)
            {
                return new(CommonErrors.PartyInBattle(req.PartyId));
            }

            // clear previous
            _db.PartyOrders.RemoveRange(party.Orders);
            // TODO:FIXME: очищать связанные с приказами временные данные, например battleFighterApplication в статусе Intent и тд (добавить в party service) или PartyTransferOffer

            List<PartyOrder> partyOrders = [];

            foreach (var (order, idx) in req.Orders.Select((order, idx) => (order, idx)))
            {
                switch (order.Type)
                {
                    case PartyOrderType.FollowParty:
                    case PartyOrderType.AttackParty:
                    case PartyOrderType.TransferOfferParty:
                        {
                            // We are not particularly concerned about database requests in the loop,
                            // as there will almost always be no more than two orders
                            var targetParty = await _db.Parties
                                .Include(p => p.User)
                                .FirstOrDefaultAsync(h => h.Id == order.TargetedPartyId, cancellationToken);
                            if (targetParty == null)
                            {
                                return new(CommonErrors.PartyNotFound(order.TargetedPartyId));
                            }

                            if (!party.Position.IsWithinDistance(targetParty.Position, _strategusMap.ViewDistance))
                            {
                                return new(CommonErrors.PartyNotInSight(order.TargetedPartyId));
                            }

                            if (order.Type == PartyOrderType.TransferOfferParty)
                            {
                                _db.PartyTransferOffers.Add(new PartyTransferOffer
                                {
                                    Party = party,
                                    TargetParty = targetParty,
                                    Status = PartyTransferOfferStatus.Intent,
                                });
                            }

                            break;
                        }

                    case PartyOrderType.MoveToSettlement:
                    case PartyOrderType.AttackSettlement:
                        {
                            var targetSettlement = await _db.Settlements
                                .Include(s => s.Owner!.User)
                                .FirstOrDefaultAsync(s => s.Id == order.TargetedSettlementId, cancellationToken);
                            if (targetSettlement == null)
                            {
                                return new(CommonErrors.SettlementNotFound(order.TargetedSettlementId));
                            }

                            break;
                        }

                    case PartyOrderType.JoinBattle:
                        {
                            var targetBattle = await _db.Battles
                                .FirstOrDefaultAsync(b => b.Id == order.TargetedBattleId, cancellationToken);
                            if (targetBattle == null)
                            {
                                return new(CommonErrors.BattleNotFound(order.TargetedBattleId));
                            }

                            foreach (var intent in order.BattleJoinIntents)
                            {
                                _db.BattleFighterApplications.Add(new BattleFighterApplication
                                {
                                    Battle = targetBattle,
                                    Party = party,
                                    Side = intent.Side,
                                    Status = BattleFighterApplicationStatus.Intent,
                                });
                            }

                            break;
                        }
                }

                partyOrders.Add(new PartyOrder
                {
                    Party = party,
                    Type = order.Type,
                    OrderIndex = idx,
                    Waypoints = order.Waypoints,
                    TargetedPartyId = order.TargetedPartyId,
                    TargetedSettlementId = order.TargetedSettlementId,
                    TargetedBattleId = order.TargetedBattleId,
                });
            }

            _db.PartyOrders.AddRange(partyOrders);

            await _db.SaveChangesAsync(cancellationToken);
            return new(_mapper.Map<PartyViewModel>(party));
        }
    }
}
