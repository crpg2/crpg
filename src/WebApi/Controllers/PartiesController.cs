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
    /// Get an update of strategus for the current user.
    /// </summary>
    /// <returns>Current strategus party, visible parties and settlements, etc.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="404">User was not registered to strategus.</response>
    [HttpGet("self/update")]
    public Task<ActionResult<Result<StrategusUpdate>>> GetStrategusUpdate()
    {
        return ResultToActionAsync(Mediator.Send(new GetStrategusUpdateQuery
        {
            PartyId = CurrentUser.User!.Id,
        }));
    }

    /// <summary>
    /// Register user to strategus.
    /// </summary>
    /// <returns>The new strategus party.</returns>
    /// <response code="201">Registered.</response>
    /// <response code="400">Already registered.</response>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public Task<ActionResult<Result<PartyViewModel>>> RegisterParty([FromBody] CreatePartyCommand req)
    {
        req.UserId = CurrentUser.User!.Id;
        return ResultToCreatedAtActionAsync(nameof(GetStrategusUpdate), null, null, Mediator.Send(req));
    }

    // /// <summary>
    // /// Update strategus party status.
    // /// </summary>
    // /// <returns>The updated strategus party.</returns>
    // /// <response code="200">Updated.</response>
    // [HttpPut("self/status")]
    // public Task<ActionResult<Result<PartyViewModel>>> UpdatePartyStatus([FromBody] UpdatePartyStatusCommand req)
    // {
    //     req.PartyId = CurrentUser.User!.Id;
    //     return ResultToActionAsync(Mediator.Send(req));
    // }

    /// <summary>
    /// Update strategus party orders.
    /// </summary>
    /// <returns>The updated strategus state.</returns>
    /// <response code="200">Updated.</response>
    [HttpPut("self/orders")]
    public async Task<ActionResult<Result<StrategusUpdate>>> UpdatePartyOrders([FromBody] UpdatePartyOrdersCommand req)
    {
        req.PartyId = CurrentUser.User!.Id;
        var result = await Mediator.Send(req);
        if (result.Errors != null && result.Errors.Count > 0)
        {
            return ResultToAction(new Result<StrategusUpdate>(result.Errors));
        }

        return await ResultToActionAsync(Mediator.Send(new GetStrategusUpdateQuery
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
    public Task<ActionResult<Result<IList<ItemStack>>>> GetPartyItems()
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
    public Task<ActionResult<Result<ItemStack>>> BuySettlementItem([FromBody] BuySettlementItemCommand req)
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
    public Task<ActionResult<Result<IList<ItemStack>>>> GetPartyItems([FromRoute] int partyId)
    {
        return ResultToActionAsync(Mediator.Send(new GetPartyItemsQuery
        {
            PartyId = partyId,
        }));
    }
}
