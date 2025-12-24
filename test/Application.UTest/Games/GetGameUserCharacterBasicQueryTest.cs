using Crpg.Application.Characters.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Queries;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games;

public class GetGameUserCharacterBasicQueryTest : TestBase
{
    [Test]
    public async Task WhenCharacterDoesntExist()
    {
        GetGameUserCharacterBasicQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetGameUserCharacterBasicQuery
        {
            CharacterId = 1,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task WhenCharacterExists()
    {
        Character dbCharacter = new()
        {
            Name = "toto",
            UserId = 2,
        };
        ArrangeDb.Characters.Add(dbCharacter);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCharacterBasicQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetGameUserCharacterBasicQuery
        {
            CharacterId = dbCharacter.Id,
            UserId = 2,
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task WhenCharacterExistsButNotOwned()
    {
        Character dbCharacter = new()
        {
            Name = "toto",
            UserId = 2,
        };
        ArrangeDb.Characters.Add(dbCharacter);
        await ArrangeDb.SaveChangesAsync();

        GetGameUserCharacterBasicQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetGameUserCharacterBasicQuery
        {
            CharacterId = dbCharacter.Id,
            UserId = 1,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}
