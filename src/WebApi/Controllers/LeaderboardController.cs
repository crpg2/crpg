using Crpg.Application.Characters.Models;
using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Servers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class LeaderboardController : BaseController
{
    /// <summary>
    /// Get top character competitive ratings.
    /// </summary>
    /// <returns>The top character competitive ratings.</returns>
    /// <response code="200">Ok.</response>
    [HttpGet("leaderboard")]
    [ResponseCache(Duration = 1 * 60 * 1)] // 1 minutes
    public Task<ActionResult<Result<IList<CharacterPublicCompetitiveViewModel>>>> GetLeaderboard(
        [FromQuery] Region? region,
        [FromQuery] CharacterClass? characterClass,
        [FromQuery] GameMode? gameMode)
    {
        return ResultToActionAsync(Mediator.Send(new GetLeaderboardQuery
        {
            Region = region,
            CharacterClass = characterClass,
            GameMode = gameMode,
        }));
    }
}
