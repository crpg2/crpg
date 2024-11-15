using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class CharacterServiceTest
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MaximumLevel = 38,
        MinimumRetirementLevel = 31,
        NewUserStartingCharacterLevel = 30,
        ExperienceMultiplierByGeneration = 0.03f,
        MaxExperienceMultiplierForGeneration = 1.48f,
        ExperienceForLevelCoefs = new[] { 2f, 0 },
        HighLevelCutoff = 30,
        DefaultAttributePoints = 0,
        AttributePointsPerLevel = 1,
        DefaultSkillPoints = 2,
        SkillPointsPerLevel = 1,
        WeaponProficiencyPointsForLevelCoefs = new[] { 100f, -90f }, // wpp = lvl * 100 - 90
        DefaultStrength = 3,
        DefaultAgility = 3,
        DefaultGeneration = 1,
        DefaultRating = 101,
        DefaultRatingDeviation = 102,
        DefaultRatingVolatility = 103,
    };

    private static readonly ExperienceTable ExperienceTable = new(Constants);

    [Test]
    public void GiveExperienceShouldGiveExperience()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = false,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 3, true);

        Assert.That(character.Level, Is.EqualTo(1));
        Assert.That(character.Experience, Is.EqualTo(8));
    }

    [Test]
    public void GiveExperienceShouldGiveExperienceBypassingMultiplier()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = false,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 3, false);

        Assert.That(character.Level, Is.EqualTo(1));
        Assert.That(character.Experience, Is.EqualTo(5));
    }

    [Test]
    public void GiveExperienceShouldntGiveExperienceIfTournamentCharacter()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = true,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 3, true);

        Assert.That(character.Level, Is.EqualTo(1));
        Assert.That(character.Experience, Is.EqualTo(2));
    }

    [Test]
    public void GiveExperienceShouldMakeCharacterLevelUpIfEnoughExperience()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new()
        {
            Level = 1,
            Experience = 2,
            ForTournament = false,
            User = new User { ExperienceMultiplier = 2f },
        };
        characterService.GiveExperience(character, 6000, true);

        Assert.That(character.Level, Is.EqualTo(2));
        Assert.That(character.Experience, Is.EqualTo(12002));
        Assert.That(character.Characteristics.Attributes.Points, Is.EqualTo(1));
        Assert.That(character.Characteristics.Skills.Points, Is.EqualTo(1));
        Assert.That(character.Characteristics.WeaponProficiencies.Points, Is.EqualTo(100));
    }

    [Test]
    [Theory]
    public void ResetCharacterStatsShouldResetStats(bool respecialization)
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new()
        {
            Level = 5,
            Class = CharacterClass.ShockInfantry,
            Characteristics = new CharacterCharacteristics
            {
                Attributes = new CharacterAttributes
                {
                    Points = 1,
                    Agility = 5,
                    Strength = 7,
                },
                Skills = new CharacterSkills
                {
                    Points = 2,
                    IronFlesh = 3,
                    PowerStrike = 4,
                    PowerDraw = 5,
                    PowerThrow = 6,
                    Athletics = 7,
                    Riding = 8,
                    WeaponMaster = 9,
                    MountedArchery = 10,
                    Shield = 11,
                },
                WeaponProficiencies = new CharacterWeaponProficiencies
                {
                    Points = 1,
                    OneHanded = 2,
                    TwoHanded = 3,
                    Polearm = 4,
                    Bow = 5,
                    Throwing = 6,
                    Crossbow = 7,
                },
            },
        };

        characterService.ResetCharacterCharacteristics(character, respecialization);
        Assert.That(character.Class, Is.EqualTo(CharacterClass.Peasant));
        if (respecialization)
        {
            Assert.That(character.Characteristics.Attributes.Points, Is.EqualTo(4));
            Assert.That(character.Characteristics.Skills.Points, Is.EqualTo(6));
            Assert.That(character.Characteristics.WeaponProficiencies.Points, Is.EqualTo(410));
        }
        else
        {
            Assert.That(character.Characteristics.Attributes.Points, Is.Zero);
            Assert.That(character.Characteristics.Skills.Points, Is.EqualTo(2));
            Assert.That(character.Characteristics.WeaponProficiencies.Points, Is.EqualTo(10));
        }

        Assert.That(character.Characteristics.Attributes.Strength, Is.EqualTo(Constants.DefaultStrength));
        Assert.That(character.Characteristics.Attributes.Agility, Is.EqualTo(Constants.DefaultAgility));
        Assert.That(character.Characteristics.Skills.IronFlesh, Is.Zero);
        Assert.That(character.Characteristics.Skills.PowerStrike, Is.Zero);
        Assert.That(character.Characteristics.Skills.PowerDraw, Is.Zero);
        Assert.That(character.Characteristics.Skills.PowerThrow, Is.Zero);
        Assert.That(character.Characteristics.Skills.Athletics, Is.Zero);
        Assert.That(character.Characteristics.Skills.Riding, Is.Zero);
        Assert.That(character.Characteristics.Skills.WeaponMaster, Is.Zero);
        Assert.That(character.Characteristics.Skills.MountedArchery, Is.Zero);
        Assert.That(character.Characteristics.Skills.Shield, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.OneHanded, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.TwoHanded, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Polearm, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Bow, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Throwing, Is.Zero);
        Assert.That(character.Characteristics.WeaponProficiencies.Crossbow, Is.Zero);
    }

    [Test]
    public void UpdateRatingTest()
    {
        Character character = new()
        {
            Statistics = new List<CharacterStatistics>()
            {
                new()
                {
                    GameMode = GameMode.CRPGBattle,
                    Rating = new CharacterRating
                    {
                        Value = 1,
                        Deviation = 2,
                        Volatility = 3,
                    },
                },
            },
        };

        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        competitiveRatingModelMock
            .Setup(m => m.ComputeCompetitiveRating(It.IsAny<CharacterRating>()))
            .Returns(4);

        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        characterService.UpdateRating(character, GameMode.CRPGBattle, 4, 5, 6);

        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.Value, Is.EqualTo(4));
        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.Deviation, Is.EqualTo(5));
        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.Volatility, Is.EqualTo(6));
        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.CompetitiveValue, Is.EqualTo(4));
    }

    [Test]
    public void ResetRatingTest()
    {
        Character character = new()
        {
            Statistics = new List<CharacterStatistics>()
            {
                new()
                {
                    GameMode = GameMode.CRPGBattle,
                    Rating = new CharacterRating
                    {
                        Value = 1,
                        Deviation = 2,
                        Volatility = 3,
                    },
                },
            },
        };

        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        competitiveRatingModelMock
            .Setup(m => m.ComputeCompetitiveRating(It.IsAny<CharacterRating>()))
            .Returns(4);

        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        characterService.ResetAllRatings(character);

        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.Value, Is.EqualTo(Constants.DefaultRating));
        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.Deviation, Is.EqualTo(Constants.DefaultRatingDeviation));
        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.Volatility, Is.EqualTo(Constants.DefaultRatingVolatility));
        Assert.That(character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle)?.Rating.CompetitiveValue, Is.EqualTo(4));
    }

    [TestCase(31, 1.00f, 1, 1.03f)]
    [TestCase(32, 1.00f, 2, 1.03f)]
    [TestCase(33, 1.00f, 4, 1.03f)]
    [TestCase(34, 1.00f, 8, 1.03f)]
    [TestCase(35, 1.00f, 16, 1.03f)]
    [TestCase(36, 1.00f, 32, 1.03f)]
    [TestCase(31, 1.06f, 1, 1.09f)]
    [TestCase(31, 1.06f, 1, 1.09f)]
    [TestCase(31, 1.45f, 1, 1.48f)]
    [TestCase(31, 1.48f, 1, 1.48f)]
    public void RetireTest(int level, float experienceMultiplier, int expectedPoints, float expectedExperienceMultiplier)
    {
        Character character = new()
        {
            Generation = 0,
            Level = level,
            Experience = 32000,
            EquippedItems =
            {
                new EquippedItem { Slot = ItemSlot.Head },
                new EquippedItem { Slot = ItemSlot.Hand },
            },
            User = new User
            {
                HeirloomPoints = 0,
                ExperienceMultiplier = experienceMultiplier,
            },
        };

        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        var error = characterService.Retire(character);

        Assert.That(error, Is.Null);
        Assert.That(character.Generation, Is.EqualTo(1));
        Assert.That(character.Level, Is.EqualTo(Constants.MinimumLevel));
        Assert.That(character.Experience, Is.EqualTo(0));
        Assert.That(character.User!.HeirloomPoints, Is.EqualTo(expectedPoints));
        Assert.That(character.User.ExperienceMultiplier, Is.EqualTo(expectedExperienceMultiplier).Within(0.001f));
        Assert.That(character.EquippedItems, Is.Empty);
    }

    [Test]
    public void RetireShouldFailIfLevelTooLow()
    {
        Character character = new() { Level = 30 };
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        var error = characterService.Retire(character);

        Assert.That(error, Is.Not.Null);
        Assert.That(error!.Code, Is.EqualTo(ErrorCode.CharacterLevelRequirementNotMet));
    }

    [Test]
    public void SetDefaultValuesShouldSetDefaultValues()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new() { Level = 2, Experience = 2, ForTournament = false };
        characterService.SetDefaultValuesForCharacter(character);

        Assert.That(character.Level, Is.EqualTo(Constants.MinimumLevel));
        Assert.That(character.Experience, Is.EqualTo(0));
        Assert.That(character.ForTournament, Is.False);
        Assert.That(character.Class, Is.EqualTo(CharacterClass.Peasant));
    }

    [Test]
    public void SetValuesForNewUserStartingCharacter()
    {
        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetExperienceForLevel(Constants.NewUserStartingCharacterLevel)).Returns(1234);
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(experienceTableMock.Object, competitiveRatingModelMock.Object, Constants);
        Character character = new() { Level = 0, Experience = 0 };
        characterService.SetValuesForNewUserStartingCharacter(character);

        Assert.That(character.Level, Is.EqualTo(Constants.NewUserStartingCharacterLevel));
        Assert.That(character.Experience, Is.EqualTo(1234));
        Assert.That(character.Class, Is.EqualTo(CharacterClass.Infantry));
    }

    [Test]
    public void ResetCharacterStatistics()
    {
        Mock<ICompetitiveRatingModel> competitiveRatingModelMock = new();
        CharacterService characterService = new(ExperienceTable, competitiveRatingModelMock.Object, Constants);
        Character character = new()
        {
            Level = 5,
            Statistics = new List<CharacterStatistics>
                    {
                        {
                            new CharacterStatistics
                            {
                                Kills = 100,
                                Deaths = 100,
                                Assists = 100,
                                PlayTime = TimeSpan.FromHours(1),
                                GameMode = GameMode.CRPGBattle,
                            }
                        },
                    },
        };
        characterService.ResetStatistics(character);

        CharacterStatistics? charStats = character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle);
        Assert.That(charStats?.Kills, Is.EqualTo(0));
        Assert.That(charStats?.Deaths, Is.EqualTo(0));
        Assert.That(charStats?.Assists, Is.EqualTo(0));
        Assert.That(charStats?.PlayTime, Is.EqualTo(TimeSpan.Zero));
    }
}
