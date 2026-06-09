using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class ItemsController : BaseController
{
    /// <summary>
    /// Gets all enabled items of rank 0.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet]
    [ResponseCache(Duration = 60 * 60 * 1)] // 1 hours
    public Task<ActionResult<Result<IList<ItemViewModel>>>> GetItemsList() =>
        ResultToActionAsync(Mediator.Send(new GetItemsQuery()));

    /// <summary>
    /// Get items sharing the same BaseId.
    /// </summary>
    /// <param name="baseId">Item BaseId.</param>
    /// <returns>The items sharing the same BaseId.</returns>
    /// <response code="200">Ok.</response>
    /// response code="400">Bad Request.</response>
    [HttpGet("upgrades/{baseId}")]
    public Task<ActionResult<Result<IList<ItemViewModel>>>> GetItemUpgrades([FromRoute] string baseId)
    {
        return ResultToActionAsync(Mediator.Send(new GetItemUpgradesQuery
        {
            BaseId = baseId,
        }));
    }

    /// <summary>
    /// Enable/Disable item.
    /// </summary>
    /// <param name="baseId">Item BaseId.</param>
    /// <param name="req">Enabling value.</param>
    /// <response code="204">Updated.</response>
    /// <response code="400">Bad Request.</response>
    [Authorize(Policy = ModeratorPolicy)]
    [HttpPut("{baseId}/enable")]
    public Task<ActionResult> EnableItem([FromRoute] string baseId, [FromBody] EnableItemCommand req)
    {
        req = req with { BaseItemId = baseId, UserId = CurrentUser.User!.Id };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Refund item.
    /// </summary>
    /// <param name="id">Item id.</param>
    /// <param name="req">Value.</param>
    /// <response code="200">OK.</response>
    /// <response code="400">Bad Request.</response>
    [Authorize(Policy = AdminPolicy)]
    [HttpPost("{id}/refund")]
    public Task<ActionResult> RefundItem([FromRoute] string id, [FromBody] RefundItemCommand req)
    {
        req = req with { ItemId = id, UserId = CurrentUser.User!.Id };
        return ResultToActionAsync(Mediator.Send(req));
    }
}
