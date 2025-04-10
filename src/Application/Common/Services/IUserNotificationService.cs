using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Notifications;

namespace Crpg.Application.Common.Services;

internal interface IUserNotificationService
{
    UserNotification CreateItemReturnedToUserNotification(int userId, string itemId, int refundedHeirloomPoints, int refundedGold);
    UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int clanId);
    UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int clanId);
    UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int clanId);
    UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int clanId);
    UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole);
    UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int clanId);
    UserNotification CreateClanMemberKickedToExMemberNotification(int userId);
    UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId);
    UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId);
    UserNotification CreateUserRewardedToUserNotification(int userId, int gold, int heirloomPoints, string itemId);
    UserNotification CreateCharacterRewardedToUserNotification(int userId);
}

internal class UserNotificationService : IUserNotificationService
{
    public UserNotification CreateItemReturnedToUserNotification(int userId, string itemId, int refundedHeirloomPoints, int refundedGold)
    {
        return CreateNotification(NotificationType.ItemReturned, userId, new NotificationMetadata[]
            {
                new("itemId", itemId),
                new("refundedHeirloomPoints", refundedHeirloomPoints.ToString()),
                new("refundedGold", refundedGold.ToString()),
            });
    }

    public UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole)
    {
        return CreateNotification(NotificationType.ClanMemberRoleChangedToUser, userId, new NotificationMetadata[]
        {
            new("clanId", clanId.ToString()),
            new("actorUserId", actorUserId.ToString()),
            new("oldClanMemberRole", oldClanMemberRole.ToString()),
            new("newClanMemberRole", newClanMemberRole.ToString()),
        });
    }

    public UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanMemberLeavedToLeader, userId, new NotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanMemberKickedToExMemberNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanMemberKickedToExMember, userId);
    }

    public UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToUser, userId, new NotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToOfficers, userId, new NotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationAcceptedToUser, userId, new NotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationDeclinedToUser, userId, new NotificationMetadata[]
            {
                new("clanId", clanId.ToString()),
            });
    }

    public UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanArmoryBorrowItemToLender, userId);
    }

    public UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanArmoryRemoveItemToBorrower, userId);
    }

    public UserNotification CreateUserRewardedToUserNotification(int userId, int gold, int heirloomPoints, string itemId)
    {
        return CreateNotification(NotificationType.UserRewardedToUser, userId, new NotificationMetadata[]
            {
                new("gold", gold.ToString()),
                new("heirloomPoints", heirloomPoints.ToString()),
                new("itemId", itemId),
            });
    }

    public UserNotification CreateCharacterRewardedToUserNotification(int userId)
    {
        return CreateNotification(NotificationType.CharacterRewardedToUser, userId);
    }

    private UserNotification CreateNotification(NotificationType type, int userId, params NotificationMetadata[] metadata)
    {
        return new UserNotification
        {
            Type = type,
            UserId = userId,
            Metadata = metadata.ToList(),
        };
    }
}
