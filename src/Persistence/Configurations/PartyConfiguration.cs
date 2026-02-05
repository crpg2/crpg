using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> builder)
    {
        builder.HasOne(h => h.User!)
            .WithOne(u => u.Party!)
            .HasForeignKey<Party>(u => u.Id);

        builder.HasMany(p => p.Orders)
               .WithOne(o => o.Party)
               .HasForeignKey(o => o.PartyId);
    }
}
