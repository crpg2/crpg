using Crpg.Application.ActivityLogs.Commands;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Clans.Models;
using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Commands;
using Crpg.Application.Games.Commands.Armory;
using Crpg.Application.Games.Models;
using Crpg.Application.Games.Queries;
using Crpg.Application.Items.Models;
using Crpg.Application.Items.Queries;
using Crpg.Application.Restrictions.Commands;
using Crpg.Application.Restrictions.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = GamePolicy)]
public class GamesController : BaseController
{
    /// <summary>
    /// Get or create user.
    /// </summary>
    [HttpGet("users")]
    public Task<ActionResult<Result<GameUserViewModel>>> GetUser(
        [FromQuery] Platform platform, [FromQuery] string platformUserId, [FromQuery] Region region, [FromQuery] string instance) =>
        ResultToActionAsync(Mediator.Send(new GetGameUserCommand
        {
            Platform = platform,
            PlatformUserId = platformUserId,
            Region = region,
            Instance = instance,
        }));

    /// <summary>
    /// Get user items.
    /// </summary>
    [HttpGet("users/{userId}/items")]
    public Task<ActionResult<Result<IList<GameUserItemExtendedViewModel>>>> GetUserItems(
        [FromRoute] int userId) =>
        ResultToActionAsync(Mediator.Send(new GetGameUserItemsQuery
        {
            UserId = userId,
        }));

    /// <summary>
    /// Get character items.
    /// </summary>
    [HttpGet("users/{userId}/characters/{characterId}/items")]
    public Task<ActionResult<Result<IList<GameEquippedItemExtendedViewModel>>>> GetCharacterEquippedItems(
        [FromRoute] int userId, [FromRoute] int characterId) =>
        ResultToActionAsync(Mediator.Send(new GetGameCharacterEquippedItemsQuery
        {
            UserId = userId,
            CharacterId = characterId,
        }));

    /// <summary>
    /// Get character basic (no equipped items or statistics).
    /// </summary>
    [HttpGet("users/{userId}/characters/{characterId}")]
    public Task<ActionResult<Result<GameCharacterViewModel>>> GetUserCharacterBasic(
        [FromRoute] int userId, [FromRoute] int characterId) =>
        ResultToActionAsync(Mediator.Send(new GetGameUserCharacterBasicQuery
        {
            UserId = userId,
            CharacterId = characterId,
        }));

    /// <summary>
    /// Updates character characteristics for the user.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="characterId">Character id.</param>
    /// <param name="cmd">The character characteristics with the updated values.</param>
    /// <returns>The updated character characteristics.</returns>
    /// <response code="200">Updated.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPut("users/{userId}/characters/{characterId}/characteristics")]
    public Task<ActionResult<Result<CharacterCharacteristicsViewModel>>> UpdateGameCharacterCharacteristics(
        [FromRoute] int userId, [FromRoute] int characterId, [FromBody] UpdateGameCharacterCharacteristicsCommand cmd) =>
        ResultToActionAsync(Mediator.Send(new UpdateGameCharacterCharacteristicsCommand
        {
            UserId = userId,
            CharacterId = characterId,
            Characteristics = cmd.Characteristics,
        }));

    /// <summary>
    /// Convert character characteristics for the current user.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="characterId">Character id.</param>
    /// <param name="conversion">Conversion.</param>
    /// <returns>The updated character characteristics.</returns>
    /// <response code="200">Conversion performed.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPut("users/{userId}/characters/{characterId}/characteristics/convert")]
    public Task<ActionResult<Result<CharacterCharacteristicsViewModel>>> GameConvertCharacterCharacteristics(
        [FromRoute] int userId,
        [FromRoute] int characterId,
        [FromBody] CharacterCharacteristicConversion conversion) =>
        ResultToActionAsync(Mediator.Send(new GameConvertCharacterCharacteristicsCommand
        {
            UserId = userId,
            CharacterId = characterId,
            Conversion = conversion,
        }));

    /// <summary>
    /// Update character items.
    /// </summary>
    [HttpPut("users/{userId}/characters/{characterId}/items")]
    public Task<ActionResult<Result<IList<EquippedItemIdViewModel>>>> UpdateCharacterItems(
        [FromRoute] int userId, [FromRoute] int characterId, [FromBody] UpdateGameCharacterItemsCommand cmd) =>
        ResultToActionAsync(Mediator.Send(new UpdateGameCharacterItemsCommand
        {
            UserId = userId,
            CharacterId = characterId,
            Items = cmd.Items,
        }));

    /// <summary>
    /// Get tournament user.
    /// </summary>
    [HttpGet("tournament-users")]
    public Task<ActionResult<Result<GameUserViewModel>>> GetTournamentUser(
        [FromQuery] Platform platform, [FromQuery] string platformUserId) =>
        ResultToActionAsync(Mediator.Send(new GetGameUserTournamentCommand
        {
            Platform = platform,
            PlatformUserId = platformUserId,
        }));

    /// <summary>
    /// Give reward to users and break or repair items.
    /// </summary>
    [HttpPut("users")]
    public Task<ActionResult<Result<UpdateGameUsersResult>>> UpdateUsers([FromBody] UpdateGameUsersCommand cmd) =>
        ResultToActionAsync(Mediator.Send(cmd));

    /// <summary>
    /// Insert activity logs.
    /// </summary>
    /// <param name="activityLogs">The activity logs to insert.</param>
    /// <response code="200">Inserted.</response>
    /// <response code="400">Bad Request.</response>
    [HttpPost("activity-logs")]
    public Task<ActionResult> InsertActivityLogs([FromBody] ActivityLogViewModel[] activityLogs)
    {
        return ResultToActionAsync(Mediator.Send(new CreateActivityLogsCommand
        {
            ActivityLogs = activityLogs,
        }, CancellationToken.None));
    }

    // TODO: this endpoint is a duplicate of /clans/{id} because I could not find a good way for an endpoint to allow
    // both the user and game policies.

    /// <summary>
    /// Gets a clan from its id.
    /// </summary>
    /// <response code="200">Ok.</response>
    /// <response code="404">Clan was not found.</response>
    [HttpGet("clans/{id}")]
    public Task<ActionResult<Result<ClanViewModel>>> GetClan([FromRoute] int id) =>
        ResultToActionAsync(Mediator.Send(new GetClanQuery { ClanId = id }));

    /// <summary>
    /// Gets the armory items.
    /// </summary>
    /// <param name="clanId">Clan id.</param>
    /// <param name="req">User id.</param>
    /// <returns>List of clan armory items.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="404">Clan was not found.</response>
    [HttpGet("clans/{clanId}/armory")]
    public Task<ActionResult<Result<IList<GameClanArmoryItemViewModel>>>> GetClanArmory(
        [FromRoute] int clanId,
        [FromQuery] GetGameClanArmoryQuery req)
    {
        return ResultToActionAsync(Mediator.Send(new GetGameClanArmoryQuery { UserId = req.UserId, ClanId = clanId }));
    }

    /// <summary>
    /// Add an item to the armory.
    /// </summary>
    /// <param name="clanId">Clan id.</param>
    /// <param name="req">Item id.</param>
    /// <returns>Added item.</returns>
    /// <response code="201">Item added to clan armory.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="409">Conflict.</response>
    [HttpPost("clans/{clanId}/armory")]
    public Task<ActionResult<Result<GameClanArmoryItemViewModel>>> AddClanArmoryItem(
        [FromRoute] int clanId,
        [FromBody] GameAddItemToClanArmoryCommand req)
    {
        return ResultToActionAsync(Mediator.Send(new GameAddItemToClanArmoryCommand
        {
            UserId = req.UserId,
            UserItemId = req.UserItemId,
            ClanId = clanId,
        }));
    }

    /// <summary>
    /// Remove an item from the armory.
    /// </summary>
    /// <param name="clanId">Clan id.</param>
    /// <param name="userItemId">Item id.</param>
    /// <param name="req">contains userId.</param>"
    /// <response code="204">Item removed from clan armory.</response>
    /// <response code="400">Bad request.</response>
    [HttpDelete("clans/{clanId}/armory/{userItemId}")]
    public Task<ActionResult> RemoveClanArmoryItem([FromRoute] int clanId, [FromRoute] int userItemId, [FromBody] GameRemoveItemFromClanArmoryCommand req)
    {
        return ResultToActionAsync(Mediator.Send(new GameRemoveItemFromClanArmoryCommand
        {
            UserId = req.UserId,
            UserItemId = userItemId,
            ClanId = clanId,
        }));
    }

    /// <summary>
    /// Borrow an item from the armory.
    /// </summary>
    /// <param name="clanId">Clan id.</param>
    /// <param name="userItemId">Item id.</param>
    /// <param name="req">contains userId.</param>
    /// <returns> Borrowed item.</returns>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpPut("clans/{clanId}/armory/{userItemId}/borrow")]
    public Task<ActionResult<Result<ClanArmoryBorrowedItemViewModel>>> BorrowClanArmoryItem([FromRoute] int clanId, [FromRoute] int userItemId, [FromBody] GameBorrowItemFromClanArmoryCommand req)
    {
        return ResultToActionAsync(Mediator.Send(new GameBorrowItemFromClanArmoryCommand
        {
            UserId = req.UserId,
            UserItemId = userItemId,
            ClanId = clanId,
        }));
    }

    /// <summary>
    /// Return an item to the armory.
    /// </summary>
    /// <param name="clanId">Clan id.</param>
    /// <param name="userItemId">Item id.</param>
    /// <param name="req">contains userId.</param>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request.</response>
    [HttpPut("clans/{clanId}/armory/{userItemId}/return")]
    public Task<ActionResult> ReturnClanArmoryItem([FromRoute] int clanId, [FromRoute] int userItemId, [FromBody] GameReturnItemToClanArmoryCommand req)
    {
        return ResultToActionAsync(Mediator.Send(new GameReturnItemToClanArmoryCommand
        {
            UserId = req.UserId,
            UserItemId = userItemId,
            ClanId = clanId,
        }));
    }

    [HttpPost("restrictions")]
    public Task<ActionResult<Result<RestrictionViewModel>>> RestrictUser([FromBody] RestrictCommand req)
    {
        return ResultToActionAsync(Mediator.Send(req));
    }
}
