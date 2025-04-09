using Crpg.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class UserNotificationMetadataConfiguration : IEntityTypeConfiguration<NotificationMetadata>
{
    public void Configure(EntityTypeBuilder<NotificationMetadata> builder)
    {
        builder.HasKey(m => new { m.NotificationId, m.Key });
    }
}
