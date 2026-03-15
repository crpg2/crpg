using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class MarketplaceOfferConfiguration : IEntityTypeConfiguration<MarketplaceOffer>
{
    public void Configure(EntityTypeBuilder<MarketplaceOffer> builder)
    {
        builder.Property(o => o.Version).IsRowVersion();

        builder.HasIndex(o => o.CreatedAt); // search without filters
        builder.HasIndex(o => o.ExpiresAt);
        builder.HasIndex(o => new { o.SellerId, o.CreatedAt }); // "Show my offers" mode

        builder.HasOne(o => o.Seller)
            .WithMany(u => u.Offers)
            .HasForeignKey(o => o.SellerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(o => o.Assets)
            .WithOne(a => a.MarketplaceOffer)
            .HasForeignKey(a => a.MarketplaceOfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
