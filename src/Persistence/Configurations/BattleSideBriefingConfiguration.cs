using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class BattleSideBriefingConfiguration : IEntityTypeConfiguration<BattleSideBriefing>
{
    public void Configure(EntityTypeBuilder<BattleSideBriefing> builder)
    {
        builder
            .HasIndex(b => new { b.BattleId, b.Side })
            .IsUnique();
    }
}
