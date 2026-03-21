using System.Net;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Commands;
using Crpg.Application.Parties.Models;
using Crpg.Application.Parties.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class PartiesController : BaseController
{
    /// <summary>
    /// Get an update of campaign for the current user.
    /// </summary>
    /// <returns>Current campaign party, visible parties and settlements, etc.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="404">User was not registered to campaign.</response>
    [HttpGet("self/update")]
    public Task<ActionResult<Result<CampaignUpdate>>> GetCampaignUpdate()
    {
        return ResultToActionAsync(Mediator.Send(new GetCampaignUpdateQuery
        {
            PartyId = CurrentUser.User!.Id,
        }));
    }

    /// <summary>
    /// Register user to campaign.
    /// </summary>
    /// <returns>The new campaign party.</returns>
    /// <response code="201">Registered.</response>
    /// <response code="400">Already registered.</response>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public Task<ActionResult<Result<PartyViewModel>>> RegisterParty([FromBody] CreatePartyCommand req)
    {
        req.UserId = CurrentUser.User!.Id;
        return ResultToCreatedAtActionAsync(nameof(GetCampaignUpdate), null, null, Mediator.Send(req));
    }

    // /// <summary>
    // /// Update campaign party status.
    // /// </summary>
    // /// <returns>The updated campaign party.</returns>
    // /// <response code="200">Updated.</response>
    // [HttpPut("self/status")]
    // public Task<ActionResult<Result<PartyViewModel>>> UpdatePartyStatus([FromBody] UpdatePartyStatusCommand req)
    // {
    //     req.PartyId = CurrentUser.User!.Id;
    //     return ResultToActionAsync(Mediator.Send(req));
    // }

    /// <summary>
    /// Update campaign party orders.
    /// </summary>
    /// <returns>The updated campaign state.</returns>
    /// <response code="200">Updated.</response>
    [HttpPut("self/orders")]
    public async Task<ActionResult<Result<CampaignUpdate>>> UpdatePartyOrders([FromBody] UpdatePartyOrdersCommand req)
    {
        req.PartyId = CurrentUser.User!.Id;
        var result = await Mediator.Send(req);
        if (result.Errors != null && result.Errors.Count > 0)
        {
            return ResultToAction(new Result<CampaignUpdate>(result.Errors));
        }

        return await ResultToActionAsync(Mediator.Send(new GetCampaignUpdateQuery
        {
            PartyId = CurrentUser.User!.Id,
        }));
    }

    /// <summary>
    /// Get self party items by id.
    /// </summary>
    /// <returns>The item stacks.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("self/items")]
    public Task<ActionResult<Result<IList<ItemStackViewModel>>>> GetPartyItems()
    {
        return ResultToActionAsync(Mediator.Send(new GetPartyItemsQuery
        {
            PartyId = CurrentUser.User!.Id,
        }));
    }

    /// <summary>
    /// Buy items from a settlement.
    /// </summary>
    /// <returns>The bought items.</returns>
    /// <response code="200">Bought.</response>
    /// <response code="400">Too far from the settlement, item not available, ...</response>
    [HttpPost("self/items")]
    public Task<ActionResult<Result<ItemStackViewModel>>> BuySettlementItem([FromBody] BuySettlementItemCommand req)
    {
        req.PartyId = CurrentUser.User!.Id;
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Get party items by id.
    /// </summary>
    /// <returns>The item stacks.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("{partyId}/items")]
    public Task<ActionResult<Result<IList<ItemStackViewModel>>>> GetPartyItems([FromRoute] int partyId)
    {
        return ResultToActionAsync(Mediator.Send(new GetPartyItemsQuery
        {
            PartyId = partyId,
        }));
    }

    /// <summary>
    /// Respond to a party transfer offer.
    /// </summary>
    /// <returns>The transfer offer.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="404">Transfer offer not found.</response>
    /// <response code="400">Invalid response (accepted more than offered, party doesn't have resources, etc).</response>
    [HttpPut("self/transfer-offers/{transferOfferId}")]
    public Task<ActionResult<Result<PartyTransferOfferViewModel>>> RespondToTransferOffer(
        [FromRoute] int transferOfferId,
        [FromBody] RespondToPartyTransferOfferCommand req)
    {
        var command = req with
        {
            PartyId = CurrentUser.User!.Id,
            TransferOfferId = transferOfferId,
        };
        return ResultToActionAsync(Mediator.Send(command));
    }
}
