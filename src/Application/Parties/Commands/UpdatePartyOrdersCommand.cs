using System.Text.Json.Serialization;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Application.Parties.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyOrdersCommand : IMediatorRequest
{
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
        public PartyTransferOfferUpdate? TransferOfferPartyIntent { get; init; }
    }

    [JsonIgnore]
    public int PartyId { get; set; }
    public PartyOrderCommandItemDto[] Orders { get; init; } = [];

    public class Validator : AbstractValidator<UpdatePartyOrdersCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Orders)
                .Must(ValidOrdersSequence)
                .WithMessage("Only MoveToPoint can be non-terminal. Any other order must be the last one.");
            RuleForEach(c => c.Orders).SetValidator(new PartyOrderValidator());
        }
    }

    private static bool ValidOrdersSequence(PartyOrderCommandItemDto[] orders)
    {
        for (int i = 0; i < orders.Length - 1; i++)
        {
            if (orders[i].Type != PartyOrderType.MoveToPoint)
            {
                return false;
            }
        }

        return true;
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
            When(o => o.Type == PartyOrderType.TransferOfferParty, () =>
            {
                RuleFor(o => o.TransferOfferPartyIntent)
                    .NotNull()
                    .WithMessage("TransferOfferPartyIntent is required");
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

    internal class Handler(ICrpgDbContext db, IStrategusMap strategusMap, Constants constants, IPartyTransferOfferValidationService validationService) : IMediatorRequestHandler<UpdatePartyOrdersCommand>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IStrategusMap _strategusMap = strategusMap;
        private readonly Constants _constants = constants;
        private readonly IPartyTransferOfferValidationService _validationService = validationService;

        public async ValueTask<Result> Handle(UpdatePartyOrdersCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                        .AsSplitQuery()
                        .Include(p => p.User!)
                            .ThenInclude(u => u.ClanMembership!.Clan)
                        .Include(p => p.Orders)
                        .Include(p => p.Items)
                            .ThenInclude(pi => pi.Item)
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

            // remove temporary entities associated with orders
            _db.PartyTransferOffers.RemoveRange(await _db.PartyTransferOffers
                .Include(to => to.Items)
                .Where(to => to.PartyId == party.Id && to.Status == PartyTransferOfferStatus.Intent)
                .ToListAsync(cancellationToken));
            _db.BattleFighterApplications.RemoveRange(await _db.BattleFighterApplications
                .Where(bfa => bfa.PartyId == party.Id && bfa.Status == BattleFighterApplicationStatus.Intent)
                .ToListAsync(cancellationToken));

            // Pre-load all referenced entities to avoid N+1 queries
            int[] targetPartyIds = [.. req.Orders
                .Where(o => o is { Type: PartyOrderType.FollowParty or PartyOrderType.AttackParty or PartyOrderType.TransferOfferParty, TargetedPartyId: > 0 })
                .Select(o => o.TargetedPartyId)
                .Distinct()];

            var targetPartiesDict = await _db.Parties
                .Include(p => p.User)
                .Where(p => targetPartyIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            int[] targetSettlementIds = [.. req.Orders
                .Where(o => o is { Type: PartyOrderType.MoveToSettlement or PartyOrderType.AttackSettlement, TargetedSettlementId: > 0 })
                .Select(o => o.TargetedSettlementId)
                .Distinct()];

            var targetSettlementsDict = await _db.Settlements
                .Include(s => s.Owner!.User)
                .Where(s => targetSettlementIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, cancellationToken);

            int[] targetBattleIds = [.. req.Orders
                .Where(o => o is { Type: PartyOrderType.JoinBattle, TargetedBattleId: > 0 })
                .Select(o => o.TargetedBattleId)
                .Distinct()];

            var targetBattlesDict = await _db.Battles
                .Where(b => targetBattleIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, cancellationToken);

            List<PartyOrder> partyOrders = [];

            foreach (var (order, idx) in req.Orders.Select((order, idx) => (order, idx)))
            {
                switch (order.Type)
                {
                    case PartyOrderType.FollowParty:
                    case PartyOrderType.AttackParty:
                    case PartyOrderType.TransferOfferParty:
                        {
                            if (!targetPartiesDict.TryGetValue(order.TargetedPartyId, out var targetParty))
                            {
                                return new(CommonErrors.PartyNotFound(order.TargetedPartyId));
                            }

                            if (!party.Position.IsWithinDistance(targetParty.Position, _strategusMap.ViewDistance))
                            {
                                return new(CommonErrors.PartyNotInSight(order.TargetedPartyId));
                            }

                            if (order.Type == PartyOrderType.TransferOfferParty && order.TransferOfferPartyIntent != null)
                            {
                                // Validate party has sufficient resources
                                var validationError = _validationService.ValidatePartyResources(party, order.TransferOfferPartyIntent.Gold, order.TransferOfferPartyIntent.Troops, order.TransferOfferPartyIntent.Items, _constants);
                                if (validationError != null)
                                {
                                    return new(validationError);
                                }

                                _db.PartyTransferOffers.Add(new PartyTransferOffer
                                {
                                    Party = party,
                                    TargetParty = targetParty,
                                    Status = PartyTransferOfferStatus.Intent,
                                    Gold = order.TransferOfferPartyIntent.Gold,
                                    Troops = order.TransferOfferPartyIntent.Troops,
                                    Items = [.. order.TransferOfferPartyIntent.Items.Select(i => new PartyTransferOfferItem
                                    {
                                        ItemId = i.ItemId,
                                        Count = i.Count,
                                    })],
                                });
                            }

                            break;
                        }

                    case PartyOrderType.MoveToSettlement:
                    case PartyOrderType.AttackSettlement:
                        {
                            if (!targetSettlementsDict.TryGetValue(order.TargetedSettlementId, out var targetSettlement))
                            {
                                return new(CommonErrors.SettlementNotFound(order.TargetedSettlementId));
                            }

                            break;
                        }

                    case PartyOrderType.JoinBattle:
                        {
                            if (!targetBattlesDict.TryGetValue(order.TargetedBattleId, out var targetBattle))
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
            return Result.NoErrors;
        }
    }
}
