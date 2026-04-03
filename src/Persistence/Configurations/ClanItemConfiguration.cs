using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ClanItemConfiguration : IEntityTypeConfiguration<ClanItem>
{
    public void Configure(EntityTypeBuilder<ClanItem> builder)
    {
        builder.HasKey(ci => ci.UserItemId);

        builder.HasOne(ci => ci.UserItem)
            .WithOne(ui => ui.ClanItem)
            .HasForeignKey<ClanItem>(ci => ci.UserItemId)
            .IsRequired();

        builder.HasOne(ci => ci.Clan)
            .WithMany()
            .HasForeignKey(ci => ci.ClanId)
            .IsRequired();
    }
}
