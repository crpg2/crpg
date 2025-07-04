using Crpg.Application.Common.Results;
using Crpg.Application.Settings.Commands;
using Crpg.Application.Settings.Models;
using Crpg.Application.Settings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

public class SettingsController : BaseController
{
    /// <summary>
    /// Get settings.
    /// </summary>
    /// <returns>The settings.</returns>
    /// <response code="200">Ok.</response>
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(Duration = 1 * 60 * 1)] // 1 minute
    public Task<ActionResult<Result<SettingsViewModel>>> GetSettings()
    {
        return ResultToActionAsync(Mediator.Send(new GetSettingsQuery()));
    }

    /// <summary>
    /// Edit setting.
    /// </summary>
    /// <returns>The setting object.</returns>
    /// <response code="201">Created.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPatch]
    [Authorize(Policy = AdminPolicy)]
    public Task<ActionResult<Result<SettingsViewModel>>> EditSetting([FromBody] EditSettingsCommand req)
    {
        return ResultToCreatedAtActionAsync(nameof(GetSettings), null, null, Mediator.Send(req));
    }
}
