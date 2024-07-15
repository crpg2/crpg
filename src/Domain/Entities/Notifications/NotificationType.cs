namespace Crpg.Domain.Entities.Notification;

public enum NotificationType
{
    UserRewardedToUser,
    CharacterRewardedToUser,
    ItemReturned,
    ClanApplicationCreatedToUser,
    ClanApplicationCreatedToOfficers,
    ClanApplicationAcceptedToUser,
    ClanApplicationDeclinedToUser,
    ClanMemberRoleChangedToUser,
    ClanMemberLeavedToLeader,
    ClanMemberKickedToExMember,
    ClanArmoryBorrowItemToLender,
    ClanArmoryRemoveItemToBorrower,
}
