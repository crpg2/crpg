using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Files;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games;
public class GameConvertCharacterCharacterisiticsCommandTest : TestBase
{
    [Test]
    public async Task AttributesToSkillsSucceeds()
    {
        // Arrange
        var user = new User();
        ActDb.Users.Add(user);

        var character = new Character
        {
            User = user,
            Characteristics =
            {
                Attributes =
                {
                    Points = 3,
                },
                Skills =
                {
                    Points = 0,
                },
            },
        };

        ActDb.Characters.Add(character);
        await ActDb.SaveChangesAsync();

        var handler = new GameConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper);
        var cmd = new GameConvertCharacterCharacteristicsCommand
        {
            UserId = user.Id,
            CharacterId = character.Id,
            Conversion = CharacterCharacteristicConversion.AttributesToSkills,
        };

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Attributes.Points, Is.EqualTo(2));
        Assert.That(result.Data!.Skills.Points, Is.EqualTo(2));
    }

    [Test]
    public async Task AttributesToSkillsFailsWhenNotEnoughPoints()
    {
        // Arrange
        var user = new User();
        ActDb.Users.Add(user);

        var character = new Character
        {
            User = user,
            Characteristics =
            {
                Attributes =
                {
                    Points = 0,
                },
                Skills =
                {
                    Points = 1,
                },
            },
        };

        ActDb.Characters.Add(character);
        await ActDb.SaveChangesAsync();

        var handler = new GameConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper);
        var cmd = new GameConvertCharacterCharacteristicsCommand
        {
            UserId = user.Id,
            CharacterId = character.Id,
            Conversion = CharacterCharacteristicConversion.AttributesToSkills,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughAttributePoints));
    }

    [Test]
    public async Task SkillsToAttributesSucceeds()
    {
        // Arrange
        var user = new User();
        ActDb.Users.Add(user);

        var character = new Character
        {
            User = user,
            Characteristics =
            {
                Attributes =
                {
                    Points = 1,
                },
                Skills =
                {
                    Points = 4,
                },
            },
        };

        ActDb.Characters.Add(character);
        await ActDb.SaveChangesAsync();

        var handler = new GameConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper);
        var cmd = new GameConvertCharacterCharacteristicsCommand
        {
            UserId = user.Id,
            CharacterId = character.Id,
            Conversion = CharacterCharacteristicConversion.SkillsToAttributes,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Attributes.Points, Is.EqualTo(2));
        Assert.That(result.Data!.Skills.Points, Is.EqualTo(2));
    }

    [Test]
    public async Task SkillsToAttributesFailsWhenNotEnoughPoints()
    {
        // Arrange
        var user = new User();
        ActDb.Users.Add(user);

        var character = new Character
        {
            User = user,
            Characteristics =
            {
                Attributes =
                {
                    Points = 3,
                },
                Skills =
                {
                    Points = 1,
                },
            },
        };

        ActDb.Characters.Add(character);
        await ActDb.SaveChangesAsync();

        var handler = new GameConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper);
        var cmd = new GameConvertCharacterCharacteristicsCommand
        {
            UserId = user.Id,
            CharacterId = character.Id,
            Conversion = CharacterCharacteristicConversion.SkillsToAttributes,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughSkillPoints));
    }

    [Test]
    public async Task Fails_WhenCharacterNotFound()
    {
        // Arrange
        var user = new User();
        ActDb.Users.Add(user);

        var handler = new GameConvertCharacterCharacteristicsCommand.Handler(ActDb, Mapper);

        var cmd = new GameConvertCharacterCharacteristicsCommand
        {
            UserId = user.Id,
            CharacterId = 9999, // nonexistent
            Conversion = CharacterCharacteristicConversion.AttributesToSkills,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}
