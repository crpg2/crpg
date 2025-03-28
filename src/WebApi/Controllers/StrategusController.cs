using Crpg.Application.Battles.Commands;
using Crpg.Application.Battles.Models;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = GamePolicy)]
public class StrategusController : BaseController
{
    /// <summary>
    /// Get strategus battles.
    /// </summary>
    [HttpGet("battles")]
    public Task<ActionResult<Result<IList<BattleDetailedViewModel>>>> GetBattles([FromQuery] Region region,
        [FromQuery(Name = "phase[]")] BattlePhase[] phases)
        => ResultToActionAsync(Mediator.Send(new GetBattlesQuery
        {
            Region = region,
            Phases = phases,
        }));

    /// <summary>
    /// Get strategus battle.
    /// </summary>
    [HttpGet("battles/{battleId}")]
    public Task<ActionResult<Result<BattleViewModel>>> GetBattle([FromRoute] int battleId) =>
        ResultToActionAsync(Mediator.Send(new GetBattleQuery
        {
            BattleId = battleId,
        }));

    /// <summary>
    /// Get battle fighters.
    /// </summary>
    /// <returns>The fighters.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("battles/{battleId}/fighters")]
    public Task<ActionResult<Result<IList<BattleFighterViewModel>>>> GetBattleFighters([FromRoute] int battleId)
    {
        return ResultToActionAsync(Mediator.Send(new GetBattleFightersQuery
        {
            BattleId = battleId,
        }));
    }

    /// <summary>
    /// Get battle mercenaries.
    /// </summary>
    /// <returns>The mercenaries.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpGet("battles/{battleId}/mercenaries")]
    public Task<ActionResult<Result<IList<BattleMercenaryViewModel>>>> GetBattleMercenaries([FromRoute] int battleId)
    {
        return ResultToActionAsync(Mediator.Send(new GetBattleMercenariesQuery
        {
            UserId = CurrentUser.User!.Id,
            BattleId = battleId,
        }));
    }

    /// <summary>
    /// Updates the battle.
    /// </summary>
    /// <returns>The updated battle.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpPut("battles/{battleId}")]
    public Task<ActionResult<Result<BattleDetailedViewModel>>> UpdateBattle([FromRoute] int battleId, [FromQuery] string instance, [FromBody] UpdateBattleCommand req)
    {
        req = req with { BattleId = battleId, Instance = instance };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Claims the battle for the game server.
    /// </summary>
    /// <returns>The claimed battle.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpPost("battles/claim")]
    public Task<ActionResult<Result<BattleViewModel>>> ClaimBattle([FromBody] ClaimBattleCommand req)
    {
        return ResultToActionAsync(Mediator.Send(req));
    }
}
