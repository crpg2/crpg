using Crpg.Domain.Entities.Notifications;

namespace Crpg.Application.Common.Services;

internal interface IUserNotificationService
{
    UserNotification CreateItemReturnedToUserNotification(int userId, string itemId, int refundedHeirloomPoints, int refundedGold);
    UserNotification CreateClanApplicationCreatedToUserNotification(int userId);
    UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId);
    UserNotification CreateClanApplicationAcceptedToUserNotification(int userId);
    UserNotification CreateClanApplicationDeclinedToUserNotification(int userId);
    UserNotification CreateClanMemberRoleChangedToUserNotification(int userId);
    UserNotification CreateClanMemberLeavedToLeaderNotification(int userId);
    UserNotification CreateClanMemberKickedToExMemberNotification(int userId);
    UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId);
    UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId);
    UserNotification CreateUserRewardedToUserNotification(int userId);
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

    public UserNotification CreateClanMemberRoleChangedToUserNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanMemberRoleChangedToUser, userId);
    }

    public UserNotification CreateClanMemberLeavedToLeaderNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanMemberLeavedToLeader, userId);
    }

    public UserNotification CreateClanMemberKickedToExMemberNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanMemberKickedToExMember, userId);
    }

    public UserNotification CreateClanApplicationCreatedToUserNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToUser, userId);
    }

    public UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToOfficers, userId);
    }

    public UserNotification CreateClanApplicationAcceptedToUserNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanApplicationAcceptedToUser, userId);
    }

    public UserNotification CreateClanApplicationDeclinedToUserNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanApplicationDeclinedToUser, userId);
    }

    public UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanArmoryBorrowItemToLender, userId);
    }

    public UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId)
    {
        return CreateNotification(NotificationType.ClanArmoryRemoveItemToBorrower, userId);
    }

    public UserNotification CreateUserRewardedToUserNotification(int userId)
    {
        return CreateNotification(NotificationType.UserRewardedToUser, userId);
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
