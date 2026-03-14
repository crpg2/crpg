using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class UserItemPresetConfiguration : IEntityTypeConfiguration<UserItemPreset>
{
    public void Configure(EntityTypeBuilder<UserItemPreset> builder)
    {
        builder.Property(p => p.Name)
            .HasMaxLength(300);

        builder.HasOne(p => p.User)
            .WithMany(u => u.ItemPresets)
            .HasForeignKey(p => p.UserId)
            .IsRequired();
    }
}
