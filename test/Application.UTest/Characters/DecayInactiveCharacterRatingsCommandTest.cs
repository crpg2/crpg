using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Servers;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class DecayInactiveCharacterRatingsCommandTest : TestBase
{
    private static readonly Constants Constants = new() { RatingDecayDays = 7 };

    [Test]
    public async Task ShouldOnlyProcessCharactersWithEligibleStatistics()
    {
        var now = DateTime.UtcNow;

        // Eligible: RatingUpdatedAt is older than decay period.
        Character eligibleCharacter = new();
        eligibleCharacter.Statistics.Add(new CharacterStatistics
        {
            GameMode = GameMode.CRPGBattle,
            RatingUpdatedAt = now.AddDays(-8),
        });

        // Ineligible: RatingUpdatedAt is null.
        Character nullRatingCharacter = new();
        nullRatingCharacter.Statistics.Add(new CharacterStatistics
        {
            GameMode = GameMode.CRPGBattle,
            RatingUpdatedAt = null,
        });

        // Ineligible: RatingUpdatedAt is within the decay period.
        Character recentCharacter = new();
        recentCharacter.Statistics.Add(new CharacterStatistics
        {
            GameMode = GameMode.CRPGBattle,
            RatingUpdatedAt = now.AddDays(-3),
        });

        ArrangeDb.Characters.AddRange(eligibleCharacter, nullRatingCharacter, recentCharacter);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        characterServiceMock.Setup(s => s.DecayRatings(It.IsAny<Character>(), It.IsAny<DateTime>())).Returns(1);

        var handler = new DecayInactiveCharacterRatingsCommand.Handler(ActDb, characterServiceMock.Object, Constants);
        var result = await handler.Handle(new DecayInactiveCharacterRatingsCommand(), CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        characterServiceMock.Verify(s => s.DecayRatings(It.IsAny<Character>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public async Task ShouldProcessAllEligibleCharactersAcrossBatches()
    {
        var now = DateTime.UtcNow;

        // Create more characters than the batch size (500) to verify batching.
        var characters = Enumerable.Range(0, 550).Select(_ =>
        {
            var c = new Character();
            c.Statistics.Add(new CharacterStatistics
            {
                GameMode = GameMode.CRPGBattle,
                RatingUpdatedAt = now.AddDays(-8),
            });
            return c;
        }).ToList();

        ArrangeDb.Characters.AddRange(characters);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        // Simulate advancing RatingUpdatedAt past the threshold so processed characters leave the filter.
        characterServiceMock
            .Setup(s => s.DecayRatings(It.IsAny<Character>(), It.IsAny<DateTime>()))
            .Callback((Character c, DateTime _) =>
            {
                foreach (var stat in c.Statistics)
                {
                    stat.RatingUpdatedAt = now.AddDays(-1);
                }
            })
            .Returns(1);

        var handler = new DecayInactiveCharacterRatingsCommand.Handler(ActDb, characterServiceMock.Object, Constants);
        var result = await handler.Handle(new DecayInactiveCharacterRatingsCommand(), CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        characterServiceMock.Verify(s => s.DecayRatings(It.IsAny<Character>(), It.IsAny<DateTime>()), Times.Exactly(550));
    }
}
