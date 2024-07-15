using Crpg.Domain.Entities.Notification;

namespace Crpg.Application.Common.Services;

internal interface IUserNotificationService
{
    UserNotification CreateItemReturnedToUserNotification(int userId, int activityLogId);
    UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int activityLogId);
    UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int activityLogId);
    UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int activityLogId);
    UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int activityLogId);
    UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int activityLogId);
    UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int activityLogId);
    UserNotification CreateClanMemberKickedToExMemberNotification(int userId, int activityLogId);
    UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId, int activityLogId);
    UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId, int activityLogId);
    UserNotification CreateUserRewardedToUserNotification(int userId, int activityLogId);
    UserNotification CreateCharacterRewardedToUserNotification(int userId, int activityLogId);
}

internal class UserNotificationService : IUserNotificationService
{
    public UserNotification CreateItemReturnedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ItemReturned, userId, activityLogId);
    }

    public UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanMemberRoleChangedToUser, userId, activityLogId);
    }

    public UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanMemberLeavedToLeader, userId, activityLogId);
    }

    public UserNotification CreateClanMemberKickedToExMemberNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanMemberKickedToExMember, userId, activityLogId);
    }

    public UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToUser, userId, activityLogId);
    }

    public UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToOfficers, userId, activityLogId);
    }

    public UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanApplicationAcceptedToUser, userId, activityLogId);
    }

    public UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanApplicationDeclinedToUser, userId, activityLogId);
    }

    public UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanArmoryBorrowItemToLender, userId, activityLogId);
    }

    public UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.ClanArmoryRemoveItemToBorrower, userId, activityLogId);
    }

    public UserNotification CreateUserRewardedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.UserRewardedToUser, userId, activityLogId);
    }

    public UserNotification CreateCharacterRewardedToUserNotification(int userId, int activityLogId)
    {
        return CreateNotification(NotificationType.CharacterRewardedToUser, userId, activityLogId);
    }

    private UserNotification CreateNotification(NotificationType type, int userId, int activityLogId)
    {
        return new UserNotification
        {
            Type = type,
            UserId = userId,
            ActivityLogId = activityLogId,
        };
    }
}
