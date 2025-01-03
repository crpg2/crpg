using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Servers;

namespace Crpg.Application.Common.Services;

internal interface IActivityLogService
{
    ActivityLog CreateUserCreatedLog(int userId);
    ActivityLog CreateUserDeletedLog(int userId);
    ActivityLog CreateUserRenamedLog(int userId, string oldName, string newName);
    ActivityLog CreateUserRewardedLog(int userId, int actorUserId, int gold, int heirloomPoints, string itemId);
    ActivityLog CreateItemBoughtLog(int userId, string itemId, int price);
    ActivityLog CreateItemSoldLog(int userId, string itemId, int price);
    ActivityLog CreateItemBrokeLog(int userId, string itemId);
    ActivityLog CreateItemReforgedLog(int userId, string itemId, int heirloomPoints, int price);
    ActivityLog CreateItemRepairedLog(int userId, string itemId, int price);
    ActivityLog CreateItemUpgradedLog(int userId, string itemId, int heirloomPoints);
    ActivityLog CreateCharacterCreatedLog(int userId, int characterId);
    ActivityLog CreateCharacterDeletedLog(int userId, int characterId, int generation, int level);
    ActivityLog CreateCharacterRatingResetLog(int userId, int characterId);
    ActivityLog CreateCharacterRespecializedLog(int userId, int characterId, int price);
    ActivityLog CreateCharacterRetiredLog(int userId, int characterId, int level);
    ActivityLog CreateCharacterRewardedLog(int userId, int actorUserId, int characterId, int experience);
    ActivityLog CreateAddItemToClanArmory(int userId, int clanId, int userItemId);
    ActivityLog CreateRemoveItemFromClanArmory(int userId, int clanId, int userItemId);
    ActivityLog CreateBorrowItemFromClanArmory(int userId, int clanId, int userItemId);
    ActivityLog CreateReturnItemToClanArmory(int userId, int clanId, int userItemId);
    ActivityLog CreateCharacterEarnedLog(int userId, int characterId, GameMode gameMode, int experience, int gold, double timeEffort);
}

internal class ActivityLogService : IActivityLogService
{
    public ActivityLog CreateUserCreatedLog(int userId)
    {
        return CreateLog(ActivityLogType.UserCreated, userId);
    }

    public ActivityLog CreateUserDeletedLog(int userId)
    {
        return CreateLog(ActivityLogType.UserDeleted, userId);
    }

    public ActivityLog CreateUserRenamedLog(int userId, string oldName, string newName)
    {
        return CreateLog(ActivityLogType.UserRenamed, userId, new ActivityLogMetadata[]
        {
            new("oldName", oldName),
            new("newName", newName),
        });
    }

    public ActivityLog CreateUserRewardedLog(int userId, int actorUserId, int gold, int heirloomPoints, string itemId)
    {
        return CreateLog(ActivityLogType.UserRewarded, userId, new ActivityLogMetadata[]
        {
            new("actorUserId", actorUserId.ToString()),
            new("gold", gold.ToString()),
            new("heirloomPoints", heirloomPoints.ToString()),
            new("itemId", itemId),
        });
    }

    public ActivityLog CreateItemBoughtLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemBought, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateItemSoldLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemSold, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateItemBrokeLog(int userId, string itemId)
    {
        return CreateLog(ActivityLogType.ItemBroke, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
        });
    }

    public ActivityLog CreateItemReforgedLog(int userId, string itemId, int heirloomPoints, int price)
    {
        return CreateLog(ActivityLogType.ItemReforged, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("heirloomPoints", heirloomPoints.ToString()),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateItemRepairedLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemRepaired, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateItemUpgradedLog(int userId, string itemId, int heirloomPoints)
    {
        return CreateLog(ActivityLogType.ItemUpgraded, userId, new ActivityLogMetadata[]
        {
            new("itemId", itemId),
            new("heirloomPoints", heirloomPoints.ToString()),
        });
    }

    public ActivityLog CreateCharacterCreatedLog(int userId, int characterId)
    {
        return CreateLog(ActivityLogType.CharacterCreated, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
        });
    }

    public ActivityLog CreateCharacterDeletedLog(int userId, int characterId, int generation, int level)
    {
        return CreateLog(ActivityLogType.CharacterDeleted, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("generation", generation.ToString()),
            new("level", level.ToString()),
        });
    }

    public ActivityLog CreateCharacterRatingResetLog(int userId, int characterId)
    {
        return CreateLog(ActivityLogType.CharacterRatingReset, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
        });
    }

    public ActivityLog CreateCharacterRespecializedLog(int userId, int characterId, int price)
    {
        return CreateLog(ActivityLogType.CharacterRespecialized, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("price", price.ToString()),
        });
    }

    public ActivityLog CreateCharacterRetiredLog(int userId, int characterId, int level)
    {
        return CreateLog(ActivityLogType.CharacterRetired, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("level", level.ToString()),
        });
    }

    public ActivityLog CreateCharacterRewardedLog(int userId, int actorUserId, int characterId, int experience)
    {
        return CreateLog(ActivityLogType.CharacterRewarded, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("experience", experience.ToString()),
            new("actorUserId", actorUserId.ToString()),
        });
    }

    public ActivityLog CreateAddItemToClanArmory(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryAddItem, userId, new ActivityLogMetadata[]
        {
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        });
    }

    public ActivityLog CreateRemoveItemFromClanArmory(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryRemoveItem, userId, new ActivityLogMetadata[]
        {
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        });
    }

    public ActivityLog CreateBorrowItemFromClanArmory(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryBorrowItem, userId, new ActivityLogMetadata[]
        {
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        });
    }

    public ActivityLog CreateReturnItemToClanArmory(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryReturnItem, userId, new ActivityLogMetadata[]
        {
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        });
    }

    public ActivityLog CreateCharacterEarnedLog(int userId, int characterId, GameMode gameMode, int experience, int gold, double timeEffort)
    {
        return CreateLog(ActivityLogType.CharacterEarned, userId, new ActivityLogMetadata[]
        {
            new("characterId", characterId.ToString()),
            new("gameMode", gameMode.ToString()),
            new("experience", experience.ToString()),
            new("gold", gold.ToString()),
            new("timeEffort", timeEffort.ToString()),
        });
    }

    private ActivityLog CreateLog(ActivityLogType type, int userId, params ActivityLogMetadata[] metadata)
    {
        return new ActivityLog
        {
            Type = type,
            UserId = userId,
            Metadata = metadata.ToList(),
        };
    }
}
