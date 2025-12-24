using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.ActivityLogs;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api;

internal interface ICrpgClient : IDisposable
{
    Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId, CrpgRegion region,
        CancellationToken cancellationToken = default);
    Task<CrpgResult<CrpgUser>> GetTournamentUserAsync(Platform platform, string platformUserId,
        CancellationToken cancellationToken = default);
    Task CreateActivityLogsAsync(IList<CrpgActivityLog> activityLogs, CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default);

    Task<CrpgResult<IList<CrpgClanArmoryItem>>> GetClanArmoryAsync(int clanId, int userId,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgClanArmoryItem>> ClanArmoryAddItemAsync(int clanId, CrpgGameClanArmoryAddItemRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<object>> RemoveClanArmoryItemAsync(int clanId, int userItemId, int userId,
       CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgClanArmoryBorrowedItem>> ClanArmoryBorrowItemAsync(int clanId, int userItemId,
        CrpgGameBorrowClanArmoryItemRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgClanArmoryBorrowedItem>> ClanArmoryReturnItemAsync(int clanId, int userItemId,
        CrpgGameBorrowClanArmoryItemRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgRestriction>> RestrictUserAsync(CrpgRestrictionRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<IList<CrpgUserItemExtended>>> GetUserItemsAsync(int userId,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgCharacter>> GetUserCharacterBasicAsync(int userId, int characterId,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<IList<CrpgEquippedItemExtended>>> GetCharacterEquippedItemsAsync(int userId, int characterId,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<IList<CrpgEquippedItemId>>> UpdateCharacterEquippedItemsAsync(int userId, int characterId,
         CrpgGameCharacterItemsUpdateRequest req,
         CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgCharacterCharacteristics>> UpdateCharacterCharacteristicsAsync(int userId, int characterId,
        CrpgGameCharacterCharacteristicsUpdateRequest req,
        CancellationToken cancellationToken = default);

    Task<CrpgResult<CrpgCharacterCharacteristics>> ConvertCharacterCharacteristicsAsync(int userId, int characterId,
        CrpgGameCharacteristicConversionRequest req,
        CancellationToken cancellationToken = default);
}
