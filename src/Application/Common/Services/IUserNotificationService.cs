using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Marketplace;
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
    UserNotification CreateBattleMercenaryApplicationRespondedNotification(int userId, int battleId, bool status);
    UserNotification CreateBattleParticipantKickedToExParticipantNotification(int userId, int battleId);
    UserNotification CreateMarketplaceOfferExpiredNotification(int userId, int offerId, int goldFee, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested);
    UserNotification CreateMarketplaceOfferAcceptedToSellerNotification(int userId, int buyerId, int offerId, int goldFee, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested);
    UserNotification CreateMarketplaceOfferAcceptedToBuyerNotification(int userId, int sellerId, int offerId, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested);
    UserNotification CreateMarketplaceOfferInvalidatedNotification(int userId, int offerId, int goldFee, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested);
}

internal class UserNotificationService(IMetadataService metadataService) : IUserNotificationService
{
    private readonly IMetadataService _metadataService = metadataService;

    public UserNotification CreateItemReturnedToUserNotification(int userId, string itemId, int refundedHeirloomPoints, int refundedGold)
    {
        return CreateNotification(NotificationType.ItemReturned, userId, [
                new("itemId", itemId),
                new("refundedHeirloomPoints", refundedHeirloomPoints.ToString()),
                new("refundedGold", refundedGold.ToString()),
            ]);
    }

    public UserNotification CreateClanMemberRoleChangedToUserNotification(int userId, int clanId, int actorUserId, ClanMemberRole oldClanMemberRole, ClanMemberRole newClanMemberRole)
    {
        return CreateNotification(NotificationType.ClanMemberRoleChangedToUser, userId, [
            new("clanId", clanId.ToString()),
            new("actorUserId", actorUserId.ToString()),
            new("oldClanMemberRole", oldClanMemberRole.ToString()),
            new("newClanMemberRole", newClanMemberRole.ToString()),
        ]);
    }

    public UserNotification CreateClanMemberLeavedToLeaderNotification(int userId, int clanId, int clanMemberUserId)
    {
        return CreateNotification(NotificationType.ClanMemberLeavedToLeader, userId, [
                new("clanId", clanId.ToString()),
                new("userId", clanMemberUserId.ToString()),
            ]);
    }

    public UserNotification CreateClanMemberKickedToExMemberNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanMemberKickedToExMember, userId, [
                new("clanId", clanId.ToString()),
            ]);
    }

    public UserNotification CreateClanApplicationCreatedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToUser, userId, [
                new("clanId", clanId.ToString()),
            ]);
    }

    public UserNotification CreateClanApplicationCreatedToOfficersNotification(int userId, int clanId, int candidateClanMemberUserId)
    {
        return CreateNotification(NotificationType.ClanApplicationCreatedToOfficers, userId, [
                new("clanId", clanId.ToString()),
                new("userId", candidateClanMemberUserId.ToString()),
            ]);
    }

    public UserNotification CreateClanApplicationAcceptedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationAcceptedToUser, userId, [
                new("clanId", clanId.ToString()),
            ]);
    }

    public UserNotification CreateClanApplicationDeclinedToUserNotification(int userId, int clanId)
    {
        return CreateNotification(NotificationType.ClanApplicationDeclinedToUser, userId, [
                new("clanId", clanId.ToString()),
            ]);
    }

    public UserNotification CreateClanArmoryBorrowItemToLenderNotification(int userId, int clanId, string itemId, int borowerUserId)
    {
        return CreateNotification(NotificationType.ClanArmoryBorrowItemToLender, userId, [
                new("clanId", clanId.ToString()),
                new("itemId", itemId),
                new("userId", borowerUserId.ToString()),
            ]);
    }

    public UserNotification CreateClanArmoryRemoveItemToBorrowerNotification(int userId, int clanId, string itemId, int lenderUserId)
    {
        return CreateNotification(NotificationType.ClanArmoryRemoveItemToBorrower, userId, [
                new("clanId", clanId.ToString()),
                new("itemId", itemId),
                new("userId", lenderUserId.ToString()),
            ]);
    }

    public UserNotification CreateUserRewardedToUserNotification(int userId, int gold, int heirloomPoints, string itemId)
    {
        return CreateNotification(NotificationType.UserRewardedToUser, userId, [
                new("gold", gold.ToString()),
                new("heirloomPoints", heirloomPoints.ToString()),
                new("itemId", itemId),
            ]);
    }

    public UserNotification CreateCharacterRewardedToUserNotification(int userId, int characterId, int experience)
    {
        return CreateNotification(NotificationType.CharacterRewardedToUser, userId, [
                new("characterId", characterId.ToString()),
                new("experience", experience.ToString()),
            ]);
    }

    public UserNotification CreateBattleMercenaryApplicationRespondedNotification(int userId, int battleId, bool status)
    {
        return CreateNotification(status ? NotificationType.BattleMercenaryApplicationAccepted : NotificationType.BattleMercenaryApplicationDeclined, userId, [
                new("battleId", battleId.ToString()),
            ]);
    }

    public UserNotification CreateBattleParticipantKickedToExParticipantNotification(int userId, int battleId)
    {
        return CreateNotification(NotificationType.BattleParticipantKickedToExParticipant, userId, [
                new("battleId", battleId.ToString()),
            ]);
    }

    public UserNotification CreateMarketplaceOfferExpiredNotification(int userId, int offerId, int goldFee, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested)
    {
        List<UserNotificationMetadata> metadata =
        [
            new("offerId", offerId.ToString()),
            new("goldFee", goldFee.ToString()),
            ..CreateMarketplaceOfferMetadata(offered, requested),
        ];

        return CreateNotification(NotificationType.MarketplaceOfferExpired, userId, [.. metadata]);
    }

    public UserNotification CreateMarketplaceOfferAcceptedToSellerNotification(int userId, int buyerId, int offerId, int goldFee, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested)
    {
        List<UserNotificationMetadata> metadata =
        [
            new("buyerId", buyerId.ToString()),
            new("offerId", offerId.ToString()),
            new("goldFee", goldFee.ToString()),
            ..CreateMarketplaceOfferMetadata(offered, requested),
        ];

        return CreateNotification(NotificationType.MarketplaceOfferAcceptedToSeller, userId, [.. metadata]);
    }

    public UserNotification CreateMarketplaceOfferAcceptedToBuyerNotification(int userId, int sellerId, int offerId, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested)
    {
        List<UserNotificationMetadata> metadata =
        [
            new("sellerId", sellerId.ToString()),
            new("offerId", offerId.ToString()),
            ..CreateMarketplaceOfferMetadata(offered, requested),
        ];

        return CreateNotification(NotificationType.MarketplaceOfferAcceptedToBuyer, userId, [.. metadata]);
    }

    public UserNotification CreateMarketplaceOfferInvalidatedNotification(int userId, int offerId, int goldFee, MarketplaceOfferAsset offered, MarketplaceOfferAsset requested)
    {
        List<UserNotificationMetadata> metadata =
        [
            new("offerId", offerId.ToString()),
            new("goldFee", goldFee.ToString()),
            ..CreateMarketplaceOfferMetadata(offered, requested),
        ];

        return CreateNotification(NotificationType.MarketplaceOfferInvalidated, userId, [.. metadata]);
    }

    private List<UserNotificationMetadata> CreateMarketplaceOfferMetadata(MarketplaceOfferAsset offered, MarketplaceOfferAsset requested) =>
        [.. _metadataService.ConvertMarketplaceOfferToMetadata(offered, requested).Select(m => new UserNotificationMetadata(m.Key, m.Value))];

    private static UserNotification CreateNotification(NotificationType type, int userId, params UserNotificationMetadata[] metadata)
    {
        return new UserNotification
        {
            Type = type,
            UserId = userId,
            Metadata = [.. metadata],
        };
    }
}
