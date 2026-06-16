using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Commands;
using Crpg.Application.Themes.Models;
using Crpg.Application.Themes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class ThemesController : BaseController
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
    public Task<ActionResult<Result<ThemeViewModel>>> UpdateTheme([FromBody] UpdateThemeCommand req) => ResultToActionAsync(Mediator.Send(req));

    /// <summary>
    /// Deletes a theme in a cascade, also removing all theme event based on that theme, and untags all items tagged with it.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = AdminPolicy)]
    public Task<ActionResult<Result<ThemeViewModel>>> DeleteTheme([FromRoute] int id) => ResultToActionAsync(Mediator.Send(new DeleteThemeCommand { Id = id }));

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
    public Task<ActionResult<Result<IList<ThemeEventViewModel>>>> GetActiveThemeEvents() => ResultToActionAsync(Mediator.Send(new GetActiveThemeEventsQuery()));

    /// <summary>
    /// Creates a new theme event.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpPost("events")]
    [Authorize(Policy = AdminPolicy)]
    public Task<ActionResult<Result<ThemeEventViewModel>>> CreateThemeEvent([FromBody] CreateThemeEventCommand req) => ResultToActionAsync(Mediator.Send(req));

    /// <summary>
    /// Updates a theme event.
    /// </summary>
    /// <response code="200">Ok.</response>
    [HttpPut("events")]
    [Authorize(Policy = AdminPolicy)]
    public Task<ActionResult<Result<ThemeEventViewModel>>> UpdateThemeEvent([FromBody] UpdateThemeEventCommand req) => ResultToActionAsync(Mediator.Send(req));
}
