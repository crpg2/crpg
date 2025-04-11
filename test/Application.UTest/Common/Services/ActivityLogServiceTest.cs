using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.ActivityLogs;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ActivityLogServiceTest
{
    private ActivityLogService? _service;

    [SetUp]
    public void SetUp()
    {
        _service = new ActivityLogService();
    }

    [Test]
    public void ExtractEntitiesFromMetadata_Should_NotDuplicate_Ids()
    {
        var activityLogs = new[]
        {
            new ActivityLog
            {
                Metadata = new List<ActivityLogMetadata>
                {
                    new("clanId", "1"),
                    new("userId", "2"),
                    new("characterId", "3"),
                },
            },
            new ActivityLog
            {
                Metadata = new List<ActivityLogMetadata>
                {
                    new("clanId", "1"),
                    new("actorUserId", "2"),
                    new("characterId", "3"),
                },
            },
        };

        var result = _service!.ExtractEntitiesFromMetadata(activityLogs);

        Assert.That(result.ClansIds, Is.EquivalentTo(new[] { 1 }));
        Assert.That(result.UsersIds, Is.EquivalentTo(new[] { 2 }));
        Assert.That(result.CharactersIds, Is.EquivalentTo(new[] { 3 }));
    }
}
