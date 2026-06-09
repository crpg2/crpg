using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class MarketplaceListingConfiguration : IEntityTypeConfiguration<MarketplaceListing>
{
    public void Configure(EntityTypeBuilder<MarketplaceListing> builder)
    {
        builder.Property(o => o.Version).IsRowVersion();

        builder.HasIndex(o => o.CreatedAt); // search without filters
        builder.HasIndex(o => o.ExpiresAt);
        builder.HasIndex(o => new { o.SellerId, o.CreatedAt }); // "Show my listings" mode

        builder.HasOne(o => o.Seller)
            .WithMany(u => u.MarketplaceListings)
            .HasForeignKey(o => o.SellerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasMany(o => o.Assets)
            .WithOne(a => a.MarketplaceListing)
            .HasForeignKey(a => a.MarketplaceListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
