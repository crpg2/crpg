using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Notifications;

namespace Crpg.Application.Common.Services;

internal interface IUserNotificationService
{
    UserNotification CreateItemReturnedToUserNotification(int userId, string itemId, int refundedHeirloomPoints, int refundedGold);
    UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int clanId);
    UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int clanId, int candidateClanMemberUserId);
    UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int clanId);
    UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int clanId);
    UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole);
    UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int clanId, int clanMemberUserId);
    UserNotification CreateClanMemberKickedToExMemberNotification(int userId, int clanId);
    UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId, int clanId, string itemId, int borowerUserId);
    UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId, int clanId, string itemId, int lenderUserId);
    UserNotification CreateUserRewardedToUserNotification(int userId, int gold, int heirloomPoints, string itemId);
    UserNotification CreateCharacterRewardedToUserNotification(int userId, int characterId, int experience);
}

internal class UserNotificationService : IUserNotificationService
{
    public UserNotification CreateItemReturnedToUserNotification(int userId, string itemId, int refundedHeirloomPoints, int refundedGold)
    {
        return CreateNotification(NotificationType.ItemReturned, userId, new UserNotificationMetadata[]
            {
                new("itemId", itemId),
                new("refundedHeirloomPoints", refundedHeirloomPoints.ToString()),
                new("refundedGold", refundedGold.ToString()),
            });
    }

    public UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole)
    {
        return CreateNotification(NotificationType.ClanMemberRoleChangedToUser, userId, new UserNotificationMetadata[]
        {
            new("clanId", clanId.ToString()),
            new("actorUserId", actorUserId.ToString()),
            new("oldClanMemberRole", oldClanMemberRole.ToString()),
            new("newClanMemberRole", newClanMemberRole.ToString()),
        });
    }

    public UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int clanId, int clanMemberUserId)
    {
        return CreateNotification(NotificationType.ClanMemberLeavedToLeader, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
                new("userId", clanMemberUserId.ToString()),
            });
    }

    public UserNotification CreateClanMemberKickedToExMemberNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanMemberKickedToExMember, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToUser, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int clanId, int candidateClanMemberUserId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToOfficers, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
                new("userId", candidateClanMemberUserId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationAcceptedToUser, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationDeclinedToUser, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId, int clanId, string itemId, int borowerUserId)
    {
        return CreateNotification(NotificationType.ClanArmoryBorrowItemToLender, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
                new("itemId", itemId),
                new("userId", borowerUserId.ToString()),
            });
    }

    public UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId, int clanId, string itemId, int lenderUserId)
    {
        return CreateNotification(NotificationType.ClanArmoryRemoveItemToBorrower, userId, new UserNotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
                new("itemId", itemId),
                new("userId", lenderUserId.ToString()),
            });
    }

    public UserNotification CreateUserRewardedToUserNotification(int userId, int gold, int heirloomPoints, string itemId)
    {
        return CreateNotification(NotificationType.UserRewardedToUser, userId, new UserNotificationMetadata[]
            {
                new("gold", gold.ToString()),
                new("heirloomPoints", heirloomPoints.ToString()),
                new("itemId", itemId),
            });
    }

    public UserNotification CreateCharacterRewardedToUserNotification(int userId, int characterId, int experience)
    {
        return CreateNotification(NotificationType.CharacterRewardedToUser, userId, new UserNotificationMetadata[]
            {
                new("characterId", characterId.ToString()),
                new("experience", experience.ToString()),
            });
    }

    private UserNotification CreateNotification(NotificationType type, int userId, params UserNotificationMetadata[] metadata)
    {
        return new UserNotification
        {
            Type = type,
            UserId = userId,
            Metadata = metadata.ToList(),
        };
    }
}
