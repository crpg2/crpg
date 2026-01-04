using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class BattleMercenaryApplicationConfiguration : IEntityTypeConfiguration<BattleMercenaryApplication>
{
    public void Configure(EntityTypeBuilder<BattleMercenaryApplication> builder)
    {
        builder
            .HasIndex(b => new { b.BattleId, b.Side })
            .IsUnique();
    }
}
