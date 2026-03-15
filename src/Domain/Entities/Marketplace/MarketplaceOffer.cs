using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Marketplace;

/// <summary>
/// Represents an offer created by a user to sell or buy items and/or gold/heirloom points.
/// An offer must contain exactly 2 assets: one with <see cref="MarketplaceOfferAssetSide.Offered"/> and another with <see cref="MarketplaceOfferAssetSide.Requested"/>.
/// When an offer is created, a fixed listing fee is deducted from the <see cref="Seller"/>'s balance, and resources (<see cref="MarketplaceOfferAsset.Gold"/> and <see cref="MarketplaceOfferAsset.HeirloomPoints"/>) from the <see cref="MarketplaceOfferAssetSide.Offered"/> side are reserved.
/// Additionally, <see cref="GoldFee"/> is reserved if <see cref="MarketplaceOfferAsset.Gold"/> exists on either side of <see cref="MarketplaceOfferAssetSide"/>.
/// When an offer is deleted (including automatic expiration or offer invalidation), all reserved resources are returned to the <see cref="Seller"/>.
/// Upon successful completion, resources from <see cref="MarketplaceOfferAssetSide.Offered"/> are transferred to the buyer, while resources from <see cref="MarketplaceOfferAssetSide.Requested"/> are deducted from the buyer and transferred to the <see cref="Seller"/>.
///
/// Offers are invalidated if the <see cref="Seller"/> or buyer no longer owns the <see cref="MarketplaceOfferAsset.ItemId"/>.
/// In this case, all offers involving that <see cref="MarketplaceOfferAsset.UserItem"/> are deleted and reserved resources are returned to <see cref="Seller"/>.
/// This can occur if the <see cref="Seller"/> or buyer accepted another offer involving the same <see cref="MarketplaceOfferAsset.UserItem"/>.
///
/// A <see cref="Seller"/> cannot accept their own offer.
/// <see cref="MarketplaceOfferAsset.UserItem"/> that is broken, held in clan storage, or is personal cannot be offered.
/// After an offer is created, if it contains a <see cref="MarketplaceOfferAsset.UserItem"/>, it can still be equipped. All other actions such as upgrading, selling, or transferring to clan storage are prohibited.
/// </summary>
public class MarketplaceOffer : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Used for optimistic concurrency.</summary>
    public uint Version { get; set; }

    public int SellerId { get; set; }

    /// <summary>
    /// The date when the offer expires. Expired offers are automatically deleted and reserved resources
    /// (<see cref="MarketplaceOfferAsset.Gold"/>, <see cref="MarketplaceOfferAsset.HeirloomPoints"/>, and <see cref="GoldFee"/>)
    /// are returned to the <see cref="Seller"/>. Set at offer creation.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// There must be exactly 2 sides: one with <see cref="MarketplaceOfferAssetSide.Offered"/>, another with <see cref="MarketplaceOfferAssetSide.Requested"/>.
    /// Only <see cref="MarketplaceOfferAssetSide.Offered"/> can have <see cref="MarketplaceOfferAsset.UserItemId"/>, and only <see cref="MarketplaceOfferAssetSide.Requested"/> can have <see cref="MarketplaceOfferAsset.ItemId"/>.
    /// Also, <see cref="MarketplaceOfferAsset.Gold"/> or <see cref="MarketplaceOfferAsset.HeirloomPoints"/> cannot be specified on both sides simultaneously. In other words, an offer where the seller offers 100 gold in exchange for 50 gold makes no sense.
    /// </summary>
    public List<MarketplaceOfferAsset> Assets { get; set; } = [];

    /// <summary>
    /// Additional gold fee charged to the <see cref="Seller"/> if the offer contains gold on either side of <see cref="MarketplaceOfferAsset"/>.
    /// Calculated at the time of offer creation and does not change after.
    /// </summary>
    public int GoldFee { get; set; }

    /// <summary>See <see cref="SellerId"/>.</summary>
    public User? Seller { get; set; }
}
