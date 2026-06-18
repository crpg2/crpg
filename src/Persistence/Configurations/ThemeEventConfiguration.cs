using Crpg.Domain.Entities.Themes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ThemeEventConfiguration : IEntityTypeConfiguration<ThemeEvent>
{
    public void Configure(EntityTypeBuilder<ThemeEvent> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => x.Name);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ActiveFromUtc).IsRequired();
        builder.Property(x => x.MinimumThemedItemsEquipped).IsRequired();

        builder.HasOne(x => x.EventTheme);
    }
}
