using Crpg.Domain.Entities.Quests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace Crpg.Persistence.Configurations;

public class QuestDefinitionConfiguration : IEntityTypeConfiguration<QuestDefinition>
{
    public void Configure(EntityTypeBuilder<QuestDefinition> builder)
    {
        builder.Property(qd => qd.EventFiltersJson)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<string, string>[]>(v))
            .HasColumnType("jsonb");

        builder.Property(qd => qd.IsActive)
            .HasDefaultValue(true);
    }
}
