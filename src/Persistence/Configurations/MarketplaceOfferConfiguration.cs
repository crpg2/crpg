using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class MarketplaceOfferConfiguration : IEntityTypeConfiguration<MarketplaceOffer>
{
    public void Configure(EntityTypeBuilder<MarketplaceOffer> builder)
    {
        builder.HasIndex(o => new { o.SellerUserId, o.Status });
        builder.HasIndex(o => new { o.Status, o.ExpiresAt });

        builder.HasOne(o => o.SellerUser)
            .WithMany()
            .HasForeignKey(o => o.SellerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(o => o.BuyerUser)
            .WithMany()
            .HasForeignKey(o => o.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasMany(o => o.Assets)
            .WithOne(a => a.MarketplaceOffer)
            .HasForeignKey(a => a.MarketplaceOfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
