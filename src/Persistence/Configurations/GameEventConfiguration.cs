using Crpg.Domain.Entities.GameEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace Crpg.Persistence.Configurations;

public class GameEventConfiguration : IEntityTypeConfiguration<GameEvent>
{
    public void Configure(EntityTypeBuilder<GameEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired(false);

        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.UserId, e.Type, e.CreatedAt });

        // Store dictionary as JSON in PostgreSQL
        builder.Property(e => e.EventData)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<GameEventField, string>>(v))
            .HasColumnType("jsonb");
    }
}
