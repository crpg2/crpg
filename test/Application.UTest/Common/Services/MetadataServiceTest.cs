using Crpg.Application.Common.Services;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class MetadataServiceTest
{
    [Test]
    public void ExtractEntitiesFromMetadata_Should_NotDuplicate_Ids()
    {
        var metadata = new List<KeyValuePair<string, string>>
        {
            new("clanId", "1"),
            new("clanId", "1"),
            new("userId", "2"),
            new("actorUserId", "2"),
            new("targetUserId", "2"),
            new("characterId", "3"),
            new("characterId", "3"),
        };

        var result = new MetadataService().ExtractEntitiesFromMetadata(metadata);

        Assert.That(result.ClansIds, Is.EquivalentTo(new[] { 1 }));
        Assert.That(result.UsersIds, Is.EquivalentTo(new[] { 2 }));
        Assert.That(result.CharactersIds, Is.EquivalentTo(new[] { 3 }));
    }
}
