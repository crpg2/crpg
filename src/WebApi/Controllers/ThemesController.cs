using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Commands;
using Crpg.Application.Themes.Models;
using Crpg.Application.Themes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class ThemesController(IOutputCacheStore outputCacheStore) : BaseController
{
    /// <summary>
    ///  Gets all themes.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet]
    public Task<ActionResult<Result<IList<ThemeViewModel>>>> GetThemes() => ResultToActionAsync(Mediator.Send(new GetThemesQuery()));

    /// <summary>
    /// Creates a new theme.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpPost]
    [Authorize(Policy = AdminPolicy)]
    public Task<ActionResult<Result<ThemeViewModel>>> CreateTheme([FromBody] CreateThemeCommand req) => ResultToActionAsync(Mediator.Send(req));

    /// <summary>
    /// Updates a theme.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpPut]
    [Authorize(Policy = AdminPolicy)]
    public async Task<ActionResult<Result<ThemeViewModel>>> UpdateTheme([FromBody] UpdateThemeCommand req)
    {
        var result = await ResultToActionAsync(Mediator.Send(req));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    /// <summary>
    /// Deletes a theme in a cascade, also removing all theme event based on that theme, and untags all items tagged with it.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = AdminPolicy)]
    public async Task<ActionResult> DeleteTheme([FromRoute] int id)
    {
        var result = await ResultToActionAsync(Mediator.Send(new DeleteThemeCommand { Id = id }));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    /// <summary>
    ///  Gets all theme events.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet("events")]
    public Task<ActionResult<Result<IList<ThemeEventViewModel>>>> GetThemeEvents() => ResultToActionAsync(Mediator.Send(new GetThemeEventsQuery()));

    /// <summary>
    ///  Gets all active theme events.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpGet("events/active")]
    [OutputCache(Tags = [ActiveThemeEventsCacheTag], Duration = 60)]
    public Task<ActionResult<Result<IList<ThemeEventViewModel>>>> GetActiveThemeEvents() => ResultToActionAsync(Mediator.Send(new GetActiveThemeEventsQuery()));

    /// <summary>
    /// Creates a new theme event.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpPost("events")]
    [Authorize(Policy = AdminPolicy)]
    public async Task<ActionResult<Result<ThemeEventViewModel>>> CreateThemeEvent([FromBody] CreateThemeEventCommand req)
    {
        var result = await ResultToActionAsync(Mediator.Send(req));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    /// <summary>
    /// Updates a theme event.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpPut("events")]
    [Authorize(Policy = AdminPolicy)]
    public async Task<ActionResult<Result<ThemeEventViewModel>>> UpdateThemeEvent([FromBody] UpdateThemeEventCommand req)
    {
        var result = await ResultToActionAsync(Mediator.Send(req));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    /// <summary>
    /// Deletes a theme event.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpDelete("events/{id}")]
    [Authorize(Policy = AdminPolicy)]
    public async Task<ActionResult> DeleteThemeEvent([FromRoute] int id)
    {
        var result = await ResultToActionAsync(Mediator.Send(new DeleteThemeEventCommand { Id = id }));
        await EvictActiveThemeEventsCacheAsync();
        return result;
    }

    private Task EvictActiveThemeEventsCacheAsync() => outputCacheStore.EvictByTagAsync(ActiveThemeEventsCacheTag, CancellationToken.None).AsTask();
}
