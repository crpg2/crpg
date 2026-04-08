using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Servers;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class DecayInactiveCharacterRatingsCommandTest : TestBase
{
    [Test]
    public async Task ShouldCallDecayRatingsForEachCharacterAndSave()
    {
        Character character1 = new();
        character1.Statistics.Add(new CharacterStatistics { GameMode = GameMode.CRPGBattle });
        Character character2 = new();
        character2.Statistics.Add(new CharacterStatistics { GameMode = GameMode.CRPGBattle });
        ArrangeDb.Characters.AddRange(character1, character2);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        characterServiceMock.Setup(s => s.DecayRatings(It.IsAny<Character>(), It.IsAny<DateTime>())).Returns(1);

        var handler = new DecayInactiveCharacterRatingsCommand.Handler(ActDb, characterServiceMock.Object);
        var result = await handler.Handle(new DecayInactiveCharacterRatingsCommand(), CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        characterServiceMock.Verify(s => s.DecayRatings(It.IsAny<Character>(), It.IsAny<DateTime>()), Times.Exactly(2));
    }
}
