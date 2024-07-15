using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.ActivityLogs;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ActivityLogServiceTest
{
    [Test]
    public void BaseCase()
    {
        ActivityLogService activityLogService = new();
        ActivityLog[] activityLogs =
        {
            new()
            {
                Metadata =
                {
                    new("characterId", "2"),
                    new("actorUserId", "2"),
                    new("userId", "3"),
                    new("clanId", "4"),
                },
            },
        };

        var res = activityLogService.ExtractEntitiesFromMetadata(activityLogs);
        Assert.That(res.CharactersIds.Count, Is.EqualTo(1));
        Assert.That(res.UsersIds.Count, Is.EqualTo(2));
        Assert.That(res.ClansIds.Count, Is.EqualTo(1));
    }
}
