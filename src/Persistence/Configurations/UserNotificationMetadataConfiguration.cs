using Crpg.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class UserNotificationMetadataConfiguration : IEntityTypeConfiguration<UserNotificationMetadata>
{
    public void Configure(EntityTypeBuilder<UserNotificationMetadata> builder)
    {
        builder.HasKey(m => new { m.UserNotificationId, m.Key });
    }
}
