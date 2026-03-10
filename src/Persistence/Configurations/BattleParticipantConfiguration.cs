using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class BattleParticipantConfiguration : IEntityTypeConfiguration<BattleParticipant>
{
    public void Configure(EntityTypeBuilder<BattleParticipant> builder)
    {
    }
}
