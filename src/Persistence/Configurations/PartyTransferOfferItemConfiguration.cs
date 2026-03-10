using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class PartyTransferOfferItemConfiguration : IEntityTypeConfiguration<PartyTransferOfferItem>
{
    public void Configure(EntityTypeBuilder<PartyTransferOfferItem> builder)
    {
        builder.HasKey(e => new { e.PartyTransferOfferId, e.ItemId });

        builder.HasOne(e => e.PartyTransferOffer)
            .WithMany(p => p.Items)
            .HasForeignKey(e => e.PartyTransferOfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
