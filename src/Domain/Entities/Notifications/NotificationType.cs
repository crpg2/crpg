namespace Crpg.Domain.Entities.Notifications;

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
    BattleMercenaryApplicationAccepted,
    BattleMercenaryApplicationDeclined,
    BattleParticipantKickedToExParticipant,
}
