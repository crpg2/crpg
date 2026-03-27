using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands;

public record UpdateSettlementItemsCommand : IMediatorRequest<ItemStackViewModel[]>
{
    [JsonIgnore]
    public int PartyId { get; init; }
    [JsonIgnore]
    public int SettlementId { get; init; }
    public ItemStackUpdate[] Items { get; init; } = [];

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<UpdateSettlementItemsCommand, ItemStackViewModel[]>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateSettlementItemsCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<ItemStackViewModel[]>> Handle(UpdateSettlementItemsCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .Include(p => p.CurrentSettlement)
                .FirstOrDefaultAsync(p => p.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if ((party.Status != PartyStatus.IdleInSettlement
                 && party.Status != PartyStatus.RecruitingInSettlement)
                || party.CurrentSettlementId != req.SettlementId)
            {
                return new(CommonErrors.PartyNotInASettlement(party.Id));
            }

            // Get all item IDs from the request (skip zero changes)
            string[] itemIds = [.. req.Items
                .Where(i => i.Count != 0)
                .Select(i => i.ItemId)
                .Distinct()];

            if (itemIds.Length == 0)
            {
                return new(Array.Empty<ItemStackViewModel>());
            }

            // Load all items from database
            var items = await _db.Items
                .Where(i => itemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, cancellationToken);

            foreach (var itemUpdate in req.Items)
            {
                if (itemUpdate.Count != 0 && !items.ContainsKey(itemUpdate.ItemId))
                {
                    return new(CommonErrors.ItemNotFound(itemUpdate.ItemId));
                }
            }

            var partyItems = await _db.ItemStacks
                .Where(pi => pi.PartyId == req.PartyId)
                .ToDictionaryAsync(pi => pi.ItemId, cancellationToken);

            var settlementItems = await _db.ItemStacks
                .Where(si => si.SettlementId == req.SettlementId)
                .ToDictionaryAsync(si => si.ItemId, cancellationToken);

            var resultSettlementItems = new List<ItemStack>();

            foreach (var itemUpdate in req.Items)
            {
                if (itemUpdate.Count == 0)
                {
                    continue;
                }

                var item = items[itemUpdate.ItemId];
                var partyItem = partyItems.GetValueOrDefault(itemUpdate.ItemId);
                var settlementItem = settlementItems.GetValueOrDefault(itemUpdate.ItemId);

                if (itemUpdate.Count > 0) // PARTY -> SETTLEMENT
                {
                    if (partyItem == null || partyItem.Count < itemUpdate.Count)
                    {
                        return new(CommonErrors.PartyNotEnoughItems(itemUpdate.ItemId, itemUpdate.Count, partyItem?.Count ?? 0));
                    }

                    // Update settlement item
                    if (settlementItem == null)
                    {
                        settlementItem = new ItemStack
                        {
                            Item = item,
                            Count = itemUpdate.Count,
                            SettlementId = req.SettlementId,
                        };
                        _db.ItemStacks.Add(settlementItem);
                        settlementItems[itemUpdate.ItemId] = settlementItem;
                        resultSettlementItems.Add(settlementItem);
                    }
                    else
                    {
                        settlementItem.Count += itemUpdate.Count;
                        resultSettlementItems.Add(settlementItem);
                    }

                    // Update party item
                    partyItem!.Count -= itemUpdate.Count;
                    if (partyItem.Count == 0)
                    {
                        _db.ItemStacks.Remove(partyItem);
                        partyItems.Remove(itemUpdate.ItemId);
                    }
                }
                else // SETTLEMENT -> PARTY
                {
                    if (party.CurrentSettlement!.OwnerId != party.Id)
                    {
                        return new(CommonErrors.PartyNotSettlementOwner(party.Id, party.CurrentSettlementId.Value));
                    }

                    if (settlementItem == null || settlementItem.Count < -itemUpdate.Count)
                    {
                        return new(CommonErrors.SettlementNotEnoughItems(itemUpdate.ItemId, -itemUpdate.Count, settlementItem?.Count ?? 0));
                    }

                    // Update settlement item
                    settlementItem!.Count += itemUpdate.Count; // Count is negative, so this subtracts
                    if (settlementItem.Count == 0)
                    {
                        _db.ItemStacks.Remove(settlementItem);
                        settlementItems.Remove(itemUpdate.ItemId);
                    }
                    else
                    {
                        resultSettlementItems.Add(settlementItem);
                    }

                    // Update party item
                    if (partyItem == null)
                    {
                        partyItem = new ItemStack
                        {
                            Item = item,
                            Count = -itemUpdate.Count, // Make it positive
                            PartyId = req.PartyId,
                        };
                        _db.ItemStacks.Add(partyItem);
                        partyItems[itemUpdate.ItemId] = partyItem;
                    }
                    else
                    {
                        partyItem.Count += -itemUpdate.Count; // Add positive count
                    }
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            int totalGiven = req.Items.Where(i => i.Count > 0).Sum(i => i.Count);
            int totalTaken = req.Items.Where(i => i.Count < 0).Sum(i => -i.Count);

            if (totalGiven > 0 || totalTaken > 0)
            {
                Logger.LogInformation(
                    "Party '{PartyId}' transferred items to/from settlement '{SettlementId}': {ItemCount} items changed, {GivenCount} given, {TakenCount} taken",
                    req.PartyId, req.SettlementId, req.Items.Length, totalGiven, totalTaken);
            }

            return new(_mapper.Map<ItemStackViewModel[]>(resultSettlementItems));
        }
    }
}
