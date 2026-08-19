using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class ItemsController(IOutputCacheStore outputCacheStore) : BaseController
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

    /// <summary>
    /// Replaces the themes assigned to an item family (every rank variant sharing the BaseId).
    /// </summary>
    /// <param name="baseId">Item BaseId.</param>
    /// <param name="req">The themes to assign.</param>
    /// <response code="200">Ok.</response>
    /// <response code="404">Item or theme not found.</response>
    [Authorize(Policy = AdminPolicy)]
    [HttpPut("{baseId}/themes")]
    public async Task<ActionResult<Result<ItemViewModel>>> SetItemThemes([FromRoute] string baseId, [FromBody] SetItemThemesCommand req)
    {
        req = req with { BaseId = baseId };
        var result = await ResultToActionAsync(Mediator.Send(req));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    /// <summary>
    /// Adds a set of themes to several item families at once (each BaseId covers all its rank variants),
    /// preserving themes they already have.
    /// </summary>
    /// <param name="req">The item BaseIds and themes to tag.</param>
    /// <response code="204">Updated.</response>
    /// <response code="404">An item or theme was not found.</response>
    [Authorize(Policy = AdminPolicy)]
    [HttpPut("themes")]
    public async Task<ActionResult> AddThemesToItems([FromBody] AddThemesToItemsCommand req)
    {
        var result = await ResultToActionAsync(Mediator.Send(req));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    /// <summary>
    /// Removes a set of themes from several item families at once (each BaseId covers all its rank variants),
    /// preserving any other themes they have.
    /// </summary>
    /// <param name="req">The item BaseIds and themes to untag.</param>
    /// <response code="204">Updated.</response>
    /// <response code="404">An item or theme was not found.</response>
    [Authorize(Policy = AdminPolicy)]
    [HttpDelete("themes")]
    public async Task<ActionResult> RemoveThemesFromItems([FromBody] RemoveThemesFromItemsCommand req)
    {
        var result = await ResultToActionAsync(Mediator.Send(req));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    private Task EvictActiveThemeEventsCacheAsync() => outputCacheStore.EvictByTagAsync(ActiveThemeEventsCacheTag, CancellationToken.None).AsTask();
}
