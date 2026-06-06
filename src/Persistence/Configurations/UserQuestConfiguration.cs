using Crpg.Domain.Entities.Quests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class UserQuestConfiguration : IEntityTypeConfiguration<UserQuest>
{
    public void Configure(EntityTypeBuilder<UserQuest> builder)
    {
        builder.HasOne(uq => uq.User)
            .WithMany()
            .HasForeignKey(uq => uq.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uq => uq.QuestDefinition)
            .WithMany()
            .HasForeignKey(uq => uq.QuestDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(uq => uq.Version).IsRowVersion();
        builder.HasIndex(uq => uq.UserId);
    }
}
