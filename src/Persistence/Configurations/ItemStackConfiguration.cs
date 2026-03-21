using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ItemStackConfiguration : IEntityTypeConfiguration<ItemStack>
{
    public void Configure(EntityTypeBuilder<ItemStack> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.PartyId, e.ItemId })
            .IsUnique()
            .HasFilter("party_id IS NOT NULL");

        builder.HasIndex(e => new { e.SettlementId, e.ItemId })
            .IsUnique()
            .HasFilter("settlement_id IS NOT NULL");

        builder.HasIndex(e => new { e.PartyTransferOfferId, e.ItemId })
            .IsUnique()
            .HasFilter("party_transfer_offer_id IS NOT NULL");

        builder.HasOne(e => e.Party)
            .WithMany(p => p.Items)
            .HasForeignKey(e => e.PartyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Settlement)
            .WithMany(s => s.Items)
            .HasForeignKey(e => e.SettlementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PartyTransferOffer)
            .WithMany(o => o.Items)
            .HasForeignKey(e => e.PartyTransferOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_item_stacks_single_owner",
            "(party_id IS NOT NULL)::int + (settlement_id IS NOT NULL)::int + (party_transfer_offer_id IS NOT NULL)::int = 1"));
    }
}
