using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Clans;
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
    ActivityLog CreateItemReturnedLog(int userId, string itemId, int refundedHeirloomPoints, int refundedGold);
    ActivityLog CreateItemUpgradedLog(int userId, string itemId, int heirloomPoints);
    ActivityLog CreateCharacterCreatedLog(int userId, int characterId);
    ActivityLog CreateCharacterDeletedLog(int userId, int characterId, int generation, int level);
    ActivityLog CreateCharacterRatingResetLog(int userId, int characterId);
    ActivityLog CreateCharacterRespecializedLog(int userId, int characterId, int price);
    ActivityLog CreateCharacterRetiredLog(int userId, int characterId, int level);
    ActivityLog CreateCharacterRewardedLog(int userId, int actorUserId, int characterId, int experience);
    ActivityLog CreateCharacterEarnedLog(int userId, int characterId, GameMode gameMode, int experience, int gold, double timeEffort);
    ActivityLog CreateClanCreatedLog(int userId, int clanId);
    ActivityLog CreateClanDeletedLog(int userId, int clanId);
    ActivityLog CreateClanApplicationCreatedLog(int userId, int clanId);
    ActivityLog CreateClanApplicationDeclinedLog(int userId, int clanId);
    ActivityLog CreateClanApplicationAcceptedLog(int userId, int clanId);
    ActivityLog CreateClanMemberRoleChangeLog(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole);
    ActivityLog CreateClanMemberLeavedLog(int userId, int clanId);
    ActivityLog CreateClanMemberKickedLog(int userId, int clanId, int actorUserId);
    ActivityLog CreateAddItemToClanArmoryLog(int userId, int clanId, int userItemId);
    ActivityLog CreateRemoveItemFromClanArmoryLog(int userId, int clanId, int userItemId);
    ActivityLog CreateBorrowItemFromClanArmoryLog(int userId, int clanId, int userItemId);
    ActivityLog CreateReturnItemToClanArmoryLog(int userId, int clanId, int userItemId);
    ActivityLog CreateApplyAsMercenaryToBattleLog(int battleId, BattleSide side, int userId, int characterId);
    ActivityLog CreateRespondToBattleMercenaryApplicationLog(int battleId, int applicationId, int userId, bool accept);
    ActivityLog CreateBattleParticipantLeavedLog(int battleId, int userId);
    ActivityLog CreateBattleParticipantKickedLog(int battleId, int userId, int actorUserId);
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
        return CreateLog(ActivityLogType.UserRenamed, userId, [
            new("oldName", oldName),
            new("newName", newName),
        ]);
    }

    public ActivityLog CreateUserRewardedLog(int userId, int actorUserId, int gold, int heirloomPoints, string itemId)
    {
        return CreateLog(ActivityLogType.UserRewarded, userId, [
            new("actorUserId", actorUserId.ToString()),
            new("gold", gold.ToString()),
            new("heirloomPoints", heirloomPoints.ToString()),
            new("itemId", itemId),
        ]);
    }

    public ActivityLog CreateItemBoughtLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemBought, userId, [
            new("itemId", itemId),
            new("price", price.ToString()),
        ]);
    }

    public ActivityLog CreateItemSoldLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemSold, userId, [
            new("itemId", itemId),
            new("price", price.ToString()),
        ]);
    }

    public ActivityLog CreateItemBrokeLog(int userId, string itemId)
    {
        return CreateLog(ActivityLogType.ItemBroke, userId, [
            new("itemId", itemId),
        ]);
    }

    public ActivityLog CreateItemReforgedLog(int userId, string itemId, int heirloomPoints, int price)
    {
        return CreateLog(ActivityLogType.ItemReforged, userId, [
            new("itemId", itemId),
            new("heirloomPoints", heirloomPoints.ToString()),
            new("price", price.ToString()),
        ]);
    }

    public ActivityLog CreateItemRepairedLog(int userId, string itemId, int price)
    {
        return CreateLog(ActivityLogType.ItemRepaired, userId, [
            new("itemId", itemId),
            new("price", price.ToString()),
        ]);
    }

    public ActivityLog CreateItemUpgradedLog(int userId, string itemId, int heirloomPoints)
    {
        return CreateLog(ActivityLogType.ItemUpgraded, userId, [
            new("itemId", itemId),
            new("heirloomPoints", heirloomPoints.ToString()),
        ]);
    }

    public ActivityLog CreateItemReturnedLog(int userId, string itemId, int refundedHeirloomPoints, int refundedGold)
    {
        return CreateLog(ActivityLogType.ItemReturned, userId, [
            new("itemId", itemId),
            new("refundedHeirloomPoints", refundedHeirloomPoints.ToString()),
            new("refundedGold", refundedGold.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterCreatedLog(int userId, int characterId)
    {
        return CreateLog(ActivityLogType.CharacterCreated, userId, [
            new("characterId", characterId.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterDeletedLog(int userId, int characterId, int generation, int level)
    {
        return CreateLog(ActivityLogType.CharacterDeleted, userId, [
            new("characterId", characterId.ToString()),
            new("generation", generation.ToString()),
            new("level", level.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterRatingResetLog(int userId, int characterId)
    {
        return CreateLog(ActivityLogType.CharacterRatingReset, userId, [
            new("characterId", characterId.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterRespecializedLog(int userId, int characterId, int price)
    {
        return CreateLog(ActivityLogType.CharacterRespecialized, userId, [
            new("characterId", characterId.ToString()),
            new("price", price.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterRetiredLog(int userId, int characterId, int level)
    {
        return CreateLog(ActivityLogType.CharacterRetired, userId, [
            new("characterId", characterId.ToString()),
            new("level", level.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterRewardedLog(int userId, int actorUserId, int characterId, int experience)
    {
        return CreateLog(ActivityLogType.CharacterRewarded, userId, [
            new("characterId", characterId.ToString()),
            new("experience", experience.ToString()),
            new("actorUserId", actorUserId.ToString()),
        ]);
    }

    public ActivityLog CreateCharacterEarnedLog(int userId, int characterId, GameMode gameMode, int experience, int gold, double timeEffort)
    {
        return CreateLog(ActivityLogType.CharacterEarned, userId, [
            new("characterId", characterId.ToString()),
            new("gameMode", gameMode.ToString()),
            new("experience", experience.ToString()),
            new("gold", gold.ToString()),
            new("timeEffort", timeEffort.ToString()),
        ]);
    }

    public ActivityLog CreateClanCreatedLog(int userId, int clanId)
    {
        return CreateLog(ActivityLogType.ClanCreated, userId, [
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateClanDeletedLog(int userId, int clanId)
    {
        return CreateLog(ActivityLogType.ClanDeleted, userId, [
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateClanMemberRoleChangeLog(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole)
    {
        return CreateLog(ActivityLogType.ClanMemberRoleEdited, userId, [
            new("clanId", clanId.ToString()),
            new("actorUserId", actorUserId.ToString()),
            new("oldClanMemberRole", oldClanMemberRole.ToString()),
            new("newClanMemberRole", newClanMemberRole.ToString()),
        ]);
    }

    public ActivityLog CreateClanMemberKickedLog(int userId, int clanId, int actorUserId)
    {
        return CreateLog(ActivityLogType.ClanMemberKicked, userId, [
            new("clanId", clanId.ToString()),
            new("actorUserId", actorUserId.ToString()),
        ]);
    }

    public ActivityLog CreateClanMemberLeavedLog(int userId, int clanId)
    {
        return CreateLog(ActivityLogType.ClanMemberLeaved, userId, [
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateClanApplicationCreatedLog(int userId, int clanId)
    {
        return CreateLog(ActivityLogType.ClanApplicationCreated, userId, [
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateClanApplicationDeclinedLog(int userId, int clanId)
    {
        return CreateLog(ActivityLogType.ClanApplicationDeclined, userId, [
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateClanApplicationAcceptedLog(int userId, int clanId)
    {
        return CreateLog(ActivityLogType.ClanApplicationAccepted, userId, [
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateAddItemToClanArmoryLog(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryAddItem, userId, [
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateRemoveItemFromClanArmoryLog(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryRemoveItem, userId, [
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateBorrowItemFromClanArmoryLog(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryBorrowItem, userId, [
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateReturnItemToClanArmoryLog(int userId, int clanId, int userItemId)
    {
        return CreateLog(ActivityLogType.ClanArmoryReturnItem, userId, [
            new("userItemId", userId.ToString()),
            new("clanId", clanId.ToString()),
        ]);
    }

    public ActivityLog CreateApplyAsMercenaryToBattleLog(int battleId, BattleSide side, int userId, int characterId)
    {
        return CreateLog(ActivityLogType.BattleApplyAsMercenary, userId, [
            new("battleId", battleId.ToString()),
            new("side", side.ToString()),
            new("userId", userId.ToString()),
            new("characterId", characterId.ToString()),
      ]);
    }

    public ActivityLog CreateRespondToBattleMercenaryApplicationLog(int battleId, int applicationId, int userId, bool accept)
    {
        return CreateLog(accept ? ActivityLogType.BattleMercenaryApplicationAccepted : ActivityLogType.BattleMercenaryApplicationDeclined, userId, [
            new("battleId", battleId.ToString()),
            new("applicationId", applicationId.ToString()),
            new("userId", userId.ToString()),
            new("status", accept ? "accepted" : "declined"),
      ]);
    }

    public ActivityLog CreateBattleParticipantLeavedLog(int battleId, int userId)
    {
        return CreateLog(ActivityLogType.BattleParticipantLeaved, userId, [
            new("battleId", battleId.ToString()),
            new("userId", userId.ToString()),
      ]);
    }

    public ActivityLog CreateBattleParticipantKickedLog(int battleId, int userId, int actorUserId)
    {
        return CreateLog(ActivityLogType.BattleParticipantKicked, userId, [
            new("battleId", battleId.ToString()),
            new("userId", userId.ToString()),
            new("actorUserId", actorUserId.ToString()),
      ]);
    }

    private static ActivityLog CreateLog(ActivityLogType type, int userId, params ActivityLogMetadata[] metadata)
    {
        return new ActivityLog
        {
            Type = type,
            UserId = userId,
            Metadata = metadata.ToList(),
        };
    }
}
