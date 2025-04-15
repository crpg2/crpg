using Crpg.Application.Common.Results;
using Crpg.Application.Terrains.Commands;
using Crpg.Application.Terrains.Models;
using Crpg.Application.Terrains.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = ModeratorPolicy)]
public class TerrainsController : BaseController
{
    /// <summary>
    /// Get Strategus map terrains.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = UserPolicy)]
    public Task<ActionResult<Result<IList<TerrainViewModel>>>> GetTerrains()
        => ResultToActionAsync(Mediator.Send(new GetTerrainsQuery()));

    /// <summary>
    /// Create terrain.
    /// </summary>
    [HttpPost]
    public Task<ActionResult<Result<TerrainViewModel>>> AddTerrain(
        [FromBody] CreateTerrainCommand req) => ResultToActionAsync(Mediator.Send(req));

    /// <summary>
    /// Update terrain.
    /// </summary>
    [HttpPut("{id}")]
    public Task<ActionResult<Result<TerrainViewModel>>> UpdateTerrain([FromRoute] int id,
        [FromBody] UpdateTerrainCommand req)
    {
        req = req with { Id = id };
        return ResultToActionAsync(Mediator.Send(req));
    }

    /// <summary>
    /// Delete terrain by Id.
    /// </summary>
    [HttpDelete("{id}")]
    public Task<ActionResult> DeleteTerrain(int id) => ResultToActionAsync(Mediator.Send(new DeleteTerrainCommand { Id = id, }, CancellationToken.None));
}
