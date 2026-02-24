using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Commands;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Settlements.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class SettlementsController : BaseController
{
    /// <summary>
    /// Get Strategus settlements.
    /// </summary>
    [HttpGet]
    public Task<ActionResult<Result<IList<SettlementPublicViewModel>>>> GetSettlements()
        => ResultToActionAsync(Mediator.Send(new GetSettlementsQuery()));

    /// <summary>
    /// Get settlement details by Id.
    /// </summary>
    [HttpGet("{settlementId}")]
    public Task<ActionResult<Result<SettlementPublicViewModel>>> GetSettlementById([FromRoute] int settlementId)
    {
        return ResultToActionAsync(Mediator.Send(new GetSettlementByIdQuery()
        {
            PartyId = CurrentUser.User!.Id,
            SettlementId = settlementId,
        }));
    }

    /// <summary>
    /// Give (position count) or take (negative count) garrison troops from a settlement.
    /// </summary>
    [HttpPut("{settlementId}")]
    public Task<ActionResult<Result<SettlementPublicViewModel>>> UpdateSettlement([FromRoute] int settlementId,
        [FromBody] UpdateSettlementCommand req)
    {
        req = req with { PartyId = CurrentUser.User!.Id, SettlementId = settlementId };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Get garrison items from a settlement.
    /// </summary>
    [HttpGet("{settlementId}/items")]
    public Task<ActionResult<Result<IList<ItemStack>>>> GetSettlementItems([FromRoute] int settlementId)
    {
        return ResultToActionAsync(Mediator.Send(new GetSettlementItemsQuery
        {
            PartyId = CurrentUser.User!.Id,
            SettlementId = settlementId,
        }));
    }

    /// <summary>
    /// Give (position count) or take (negative count) garrison items from a settlement.
    /// </summary>
    [HttpPut("{settlementId}/items")]
    public Task<ActionResult<Result<ItemStack>>> UpdateSettlementItems([FromRoute] int settlementId,
        [FromBody] AddSettlementItemCommand req)
    {
        req = req with { PartyId = CurrentUser.User!.Id, SettlementId = settlementId };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Get strategus settlement shop items.
    /// </summary>
    [HttpGet("{settlementId}/shop/items")]
    public Task<ActionResult<Result<IList<ItemViewModel>>>> GetSettlementShopItems([FromRoute] int settlementId)
        => ResultToActionAsync(Mediator.Send(new GetSettlementShopItemsQuery
        {
            PartyId = CurrentUser.User!.Id,
            SettlementId = settlementId,
        }));
}
