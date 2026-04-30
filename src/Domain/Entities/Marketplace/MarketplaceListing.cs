using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Marketplace;

/// <summary>
/// Represents a listing created by a user to sell or buy items and/or gold/heirloom points.
/// A listing must contain exactly 2 assets: one with <see cref="MarketplaceListingAssetSide.Offered"/> and another with <see cref="MarketplaceListingAssetSide.Requested"/>.
/// When a listing is created, a fixed listing fee is deducted from the <see cref="Seller"/>'s balance, and resources (<see cref="MarketplaceListingAsset.Gold"/> and <see cref="MarketplaceListingAsset.HeirloomPoints"/>) from the <see cref="MarketplaceListingAssetSide.Offered"/> side are reserved.
/// Additionally, <see cref="GoldFee"/> is reserved if <see cref="MarketplaceListingAsset.Gold"/> exists on either side of <see cref="MarketplaceListingAssetSide"/>.
/// When a listing is deleted (including automatic expiration or listing invalidation), all reserved resources are returned to the <see cref="Seller"/>.
/// Upon successful completion, resources from <see cref="MarketplaceListingAssetSide.Offered"/> are transferred to the buyer, while resources from <see cref="MarketplaceListingAssetSide.Requested"/> are deducted from the buyer and transferred to the <see cref="Seller"/>.
///
/// Listings are invalidated if the <see cref="Seller"/> or buyer no longer owns the <see cref="MarketplaceListingAsset.ItemId"/>.
/// In this case, all listings involving that <see cref="MarketplaceListingAsset.UserItem"/> are deleted and reserved resources are returned to <see cref="Seller"/>.
/// This can occur if the <see cref="Seller"/> or buyer accepted another listing involving the same <see cref="MarketplaceListingAsset.UserItem"/>.
///
/// A <see cref="Seller"/> cannot accept their own listing.
/// <see cref="MarketplaceListingAsset.UserItem"/> that is broken, held in clan storage, or is personal cannot be offered.
/// After a listing is created, if it contains a <see cref="MarketplaceListingAsset.UserItem"/>, it can still be equipped. All other actions such as upgrading, selling, or transferring to clan storage are prohibited.
/// </summary>
public class MarketplaceListing : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Used for optimistic concurrency.</summary>
    public uint Version { get; set; }

    public int SellerId { get; set; }

    /// <summary>
    /// The date when the listing expires. Expired listings are automatically deleted and reserved resources
    /// (<see cref="MarketplaceListingAsset.Gold"/>, <see cref="MarketplaceListingAsset.HeirloomPoints"/>, and <see cref="GoldFee"/>)
    /// are returned to the <see cref="Seller"/>. Set at listing creation.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// There must be exactly 2 sides: one with <see cref="MarketplaceListingAssetSide.Offered"/>, another with <see cref="MarketplaceListingAssetSide.Requested"/>.
    /// Only <see cref="MarketplaceListingAssetSide.Offered"/> can have <see cref="MarketplaceListingAsset.UserItemId"/>, and only <see cref="MarketplaceListingAssetSide.Requested"/> can have <see cref="MarketplaceListingAsset.ItemId"/>.
    /// Also, <see cref="MarketplaceListingAsset.Gold"/> or <see cref="MarketplaceListingAsset.HeirloomPoints"/> cannot be specified on both sides simultaneously. In other words, a listing where the seller offers 100 gold in exchange for 50 gold makes no sense.
    /// </summary>
    public List<MarketplaceListingAsset> Assets { get; set; } = [];

    /// <summary>
    /// Additional gold fee charged to the <see cref="Seller"/> if the listing contains gold on either side of <see cref="MarketplaceListingAsset"/>.
    /// Calculated at the time of listing creation and does not change after.
    /// </summary>
    public int GoldFee { get; set; }

    /// <summary>See <see cref="SellerId"/>.</summary>
    public User? Seller { get; set; }
}
