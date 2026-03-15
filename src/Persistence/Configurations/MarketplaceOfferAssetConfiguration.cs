using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class MarketplaceOfferAssetConfiguration : IEntityTypeConfiguration<MarketplaceOfferAsset>
{
    public void Configure(EntityTypeBuilder<MarketplaceOfferAsset> builder)
    {
        builder.HasIndex(a => new { a.MarketplaceOfferId, a.Side });
        builder.HasIndex(a => new { a.Type, a.ItemId });
        builder.HasIndex(a => a.UserItemId);

        builder.HasOne(a => a.UserItem)
            .WithMany()
            .HasForeignKey(a => a.UserItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
