using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Parties.Services;

internal interface IPartyTransferOfferValidationService
{
    /// <summary>
    /// Validates that the accepted transfer offer matches the original offer.
    /// </summary>
    Error? ValidateAcceptedResources(PartyTransferOfferUpdate accepted, PartyTransferOffer offer);

    /// <summary>
    /// Validates that the party has sufficient resources for a transfer.
    /// </summary>
    Error? ValidatePartyResources(Party party, int gold, float troops, ItemStackUpdate[] items, Constants constants);
}

internal class PartyTransferOfferValidationService : IPartyTransferOfferValidationService
{
    public Error? ValidateAcceptedResources(PartyTransferOfferUpdate accepted, PartyTransferOffer offer)
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

    public Error? ValidatePartyResources(Party party, int gold, float troops, ItemStackUpdate[] items, Constants constants)
    {
        if (party.Gold < gold)
        {
            return CommonErrors.NotEnoughGold(gold, party.Gold);
        }

        if (party.Troops < troops)
        {
            return CommonErrors.PartyNotEnoughTroops(party.Id);
        }

        if (party.Troops - troops < constants.StrategusMinPartyTroops)
        {
            return CommonErrors.TransferOfferInvalidAmount(
                $"Cannot transfer {troops} troops as party must maintain at least {constants.StrategusMinPartyTroops} troops");
        }

        foreach (var item in items)
        {
            var partyItem = party.Items.FirstOrDefault(pi => pi.ItemId == item.ItemId);
            if (partyItem == null || partyItem.Count < item.Count)
            {
                return CommonErrors.TransferOfferInvalidAmount(
                    $"Party doesn't have {item.Count} of item '{item.ItemId}'");
            }
        }

        return null;
    }
}
