using Crpg.Domain.Entities.Quests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class WeeklyQuestAssignmentConfiguration : IEntityTypeConfiguration<WeeklyQuestAssignment>
{
    public void Configure(EntityTypeBuilder<WeeklyQuestAssignment> builder)
    {
        builder.HasKey(wqa => wqa.Id);
        builder.Property(wqa => wqa.Id).ValueGeneratedOnAdd();

        builder.HasOne(wqa => wqa.QuestDefinition)
            .WithMany()
            .HasForeignKey(wqa => wqa.QuestDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wqa => wqa.ExpiresAt);
    }
}
