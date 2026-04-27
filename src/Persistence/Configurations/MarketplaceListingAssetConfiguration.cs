using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class MarketplaceListingAssetConfiguration : IEntityTypeConfiguration<MarketplaceListingAsset>
{
    public void Configure(EntityTypeBuilder<MarketplaceListingAsset> builder)
    {
        builder.HasIndex(a => new { a.MarketplaceListingId, a.Side });
        builder.HasIndex(a => a.UserItemId);

        builder.HasOne(a => a.UserItem)
            .WithMany(u => u.MarketplaceListingAssets)
            .HasForeignKey(a => a.UserItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
