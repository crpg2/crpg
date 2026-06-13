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
}
