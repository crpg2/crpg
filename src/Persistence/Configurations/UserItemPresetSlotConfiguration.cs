using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class UserItemPresetSlotConfiguration : IEntityTypeConfiguration<UserItemPresetSlot>
{
    public void Configure(EntityTypeBuilder<UserItemPresetSlot> builder)
    {
        builder.HasKey(s => new { s.UserItemPresetId, s.Slot });

        builder.HasOne(s => s.UserItemPreset)
            .WithMany(p => p.Slots)
            .HasForeignKey(s => s.UserItemPresetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Item)
            .WithMany()
            .HasForeignKey(s => s.ItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
