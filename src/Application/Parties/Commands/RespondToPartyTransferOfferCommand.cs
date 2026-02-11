using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record RespondToPartyTransferOfferCommand : IMediatorRequest<PartyTransferOfferViewModel>
{
    [JsonIgnore]
    public int PartyId { get; set; }
    [JsonIgnore]
    public int TransferOfferId { get; set; }
    public bool Accept { get; set; }
    public TransferOffer? Accepted { get; set; }

    public record TransferOffer
    {
        public int Gold { get; set; }
        public float Troops { get; set; }
        public List<TransferOfferItem> Items { get; set; } = [];
    }

    public record TransferOfferItem
    {
        public string ItemId { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<RespondToPartyTransferOfferCommand, PartyTransferOfferViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespondToPartyTransferOfferCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<PartyTransferOfferViewModel>> Handle(RespondToPartyTransferOfferCommand req,
            CancellationToken cancellationToken)
        {
            var respondingParty = await _db.Parties
                .Include(p => p.Items)
                    .ThenInclude(pi => pi.Item)
                .FirstOrDefaultAsync(p => p.Id == req.PartyId, cancellationToken);
            if (respondingParty == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var offer = await _db.PartyTransferOffers
                .Include(o => o.Party)
                    .ThenInclude(p => p!.Items)
                        .ThenInclude(i => i.Item)
                .Include(o => o.TargetParty)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(o => o.Id == req.TransferOfferId, cancellationToken);
            if (offer == null)
            {
                return new(CommonErrors.TransferOfferNotFound(req.TransferOfferId));
            }

            // Validate that the offer is for the responding party
            if (offer.TargetPartyId != req.PartyId)
            {
                return new(CommonErrors.TransferOfferNotAllowed(req.PartyId, req.TransferOfferId));
            }

            // Validate offer status
            if (offer.Status != PartyTransferOfferStatus.Pending)
            {
                return new(CommonErrors.TransferOfferInvalidStatus(req.TransferOfferId, offer.Status));
            }

            var offeringParty = offer.Party;
            if (offeringParty == null)
            {
                return new(new Error(ErrorType.NotFound, ErrorCode.PartyNotFound)
                {
                    Title = "Offering party was not found",
                    Detail = $"Party that made the offer was not found",
                });
            }

            if (req.Accept)
            {
                if (req.Accepted == null)
                {
                    return new(CommonErrors.TransferOfferMissingItems());
                }

                var validationResult = ValidateAcceptedResources(req.Accepted, offer);
                if (validationResult != null)
                {
                    return new(validationResult);
                }

                var offeringPartyValidation = ValidateOfferingPartyResources(offeringParty, offer);
                if (offeringPartyValidation != null)
                {
                    return new(offeringPartyValidation);
                }

                offeringParty.Gold -= req.Accepted.Gold;
                offeringParty.Troops -= req.Accepted.Troops;

                respondingParty.Gold += req.Accepted.Gold;
                respondingParty.Troops += req.Accepted.Troops;

                TransferItems(offeringParty, respondingParty, [.. req.Accepted.Items.Select(i => new PartyTransferOfferItem
                {
                    ItemId = i.ItemId,
                    Count = i.Count,
                })]);

                // offeringParty.Status = PartyStatus.Idle;
            }

            _db.PartyTransferOffers.Remove(offer);

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Party '{0}' {1} transfer offer '{2}'", req.PartyId, req.Accept ? "accepted" : "declined", req.TransferOfferId);

            return new(_mapper.Map<PartyTransferOfferViewModel>(offer));
        }

        private static Error? ValidateAcceptedResources(TransferOffer accepted, PartyTransferOffer offer)
        {
            if (accepted.Gold > offer.Gold)
            {
                return CommonErrors.TransferOfferInvalidAmount($"Cannot accept {accepted.Gold} gold when only {offer.Gold} gold was offered");
            }

            if (accepted.Troops > offer.Troops)
            {
                return CommonErrors.TransferOfferInvalidAmount($"Cannot accept {accepted.Troops} troops when only {offer.Troops} troops were offered");
            }

            foreach (var acceptedItem in accepted.Items)
            {
                var originalItem = offer.Items.FirstOrDefault(i => i.ItemId == acceptedItem.ItemId);
                if (originalItem == null)
                {
                    return CommonErrors.TransferOfferInvalidItem(acceptedItem.ItemId);
                }

                if (acceptedItem.Count > originalItem.Count)
                {
                    return CommonErrors.TransferOfferInvalidAmount($"Cannot accept {acceptedItem.Count} of item '{acceptedItem.ItemId}' when only {originalItem.Count} were offered");
                }
            }

            return null;
        }

        private static Error? ValidateOfferingPartyResources(Party offeringParty, PartyTransferOffer offer)
        {
            if (offeringParty.Gold < offer.Gold)
            {
                return CommonErrors.NotEnoughGold(offer.Gold, offeringParty.Gold);
            }

            if (offeringParty.Troops < offer.Troops)
            {
                return CommonErrors.PartyNotEnoughTroops(offeringParty.Id);
            }

            foreach (var offerItem in offer.Items)
            {
                var partyItem = offeringParty.Items.FirstOrDefault(pi => pi.ItemId == offerItem.ItemId);
                if (partyItem == null || partyItem.Count < offerItem.Count)
                {
                    return CommonErrors.TransferOfferInvalidAmount(
                        $"Offering party doesn't have {offerItem.Count} of item '{offerItem.ItemId}'");
                }
            }

            return null;
        }

        private void TransferItems(Party from, Party to, List<PartyTransferOfferItem> items)
        {
            foreach (var item in items)
            {
                var fromItem = from.Items.FirstOrDefault(pi => pi.ItemId == item.ItemId);
                if (fromItem != null)
                {
                    fromItem.Count -= item.Count;
                    if (fromItem.Count <= 0)
                    {
                        _db.PartyItems.Remove(fromItem);
                    }
                }

                var toItem = to.Items.FirstOrDefault(pi => pi.ItemId == item.ItemId);
                if (toItem != null)
                {
                    toItem.Count += item.Count;
                }
                else
                {
                    var newItem = new PartyItem
                    {
                        PartyId = to.Id,
                        ItemId = item.ItemId,
                        Count = item.Count,
                    };
                    _db.PartyItems.Add(newItem);
                }
            }
        }
    }
}
