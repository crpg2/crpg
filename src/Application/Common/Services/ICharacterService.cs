using System.Diagnostics;
using Crpg.Application.Common.Results;
using Crpg.Common.Helpers;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Servers;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Common logic for characters.
/// </summary>
internal interface ICharacterService
{
    void SetDefaultValuesForCharacter(Character character);

    void SetValuesForNewUserStartingCharacter(Character character);

    /// <summary>
    /// Reset character characteristics.
    /// </summary>
    /// <param name="character">Character to reset.</param>
    /// <param name="respecialization">If the stats points should be redistributed.</param>
    void ResetCharacterCharacteristics(Character character, bool respecialization = false);

    void UpdateRating(Character character, GameMode gameMode, float value, float deviation, float volatility, bool isGameUserUpdate = false);

    void ResetAllRatings(Character character);

    void ResetRating(Character character, GameMode gameMode);

    void ResetStatistics(Character character);

    Error? Retire(Character character);

    void GiveExperience(Character character, int experience, bool useExperienceMultiplier);
}

/// <inheritdoc />
internal class CharacterService : ICharacterService
{
    private readonly IExperienceTable _experienceTable;
    private readonly ICompetitiveRatingModel _competitiveRatingModel;
    private readonly Constants _constants;

    public CharacterService(
        IExperienceTable experienceTable,
        ICompetitiveRatingModel competitiveRatingModel,
        Constants constants)
    {
        _experienceTable = experienceTable;
        _competitiveRatingModel = competitiveRatingModel;
        _constants = constants;
    }

    public void SetValuesForNewUserStartingCharacter(Character character)
    {
        character.Generation = _constants.DefaultGeneration;
        character.Level = _constants.NewUserStartingCharacterLevel;
        character.Experience = _experienceTable.GetExperienceForLevel(character.Level);
        character.Class = CharacterClass.Infantry;
        ResetStatistics(character);
        ResetAllRatings(character);
    }

    public void SetDefaultValuesForCharacter(Character character)
    {
        character.Generation = _constants.DefaultGeneration;
        character.Level = _constants.MinimumLevel;
        character.Experience = _experienceTable.GetExperienceForLevel(character.Level);
        character.ForTournament = false;
        ResetStatistics(character);
        ResetCharacterCharacteristics(character);
        ResetAllRatings(character);
    }

    /// <inheritdoc />
    public void ResetCharacterCharacteristics(Character character, bool respecialization = false)
    {
        int CalculateAttributePoints(int level)
        {
            int points = 0;
            for (int i = 1; i < level; i++)
            {
                if (i < _constants.HighLevelCutoff)
                {
                    points += _constants.AttributePointsPerLevel;
                }
            }

            return points;
        }

        character.Characteristics = new CharacterCharacteristics
        {
            Attributes = new CharacterAttributes
            {
                Points = _constants.DefaultAttributePoints + (respecialization ? CalculateAttributePoints(character.Level) : 0),
                Strength = _constants.DefaultStrength,
                Agility = _constants.DefaultAgility,
            },
            Skills = new CharacterSkills
            {
                Points = _constants.DefaultSkillPoints + (respecialization ? (character.Level - 1) * _constants.SkillPointsPerLevel : 0),
            },
            WeaponProficiencies = new CharacterWeaponProficiencies
            {
                Points = WeaponProficiencyPointsForLevel(respecialization ? character.Level : 1),
            },
        };
        character.Class = CharacterClass.Peasant;
    }

    public void ResetStatistics(Character character)
    {
        character.Statistics = new List<CharacterStatistics>();

        foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
        {
            character.Statistics.Add(new CharacterStatistics
            {
                GameMode = gameMode,
                Kills = 0,
                Deaths = 0,
                Assists = 0,
                PlayTime = TimeSpan.Zero,
            });
        }
    }

    public void UpdateRating(Character character, GameMode gameMode, float value, float deviation, float volatility, bool isGameUserUpdate = false)
    {
        if (character.Level == 1 && isGameUserUpdate)
        {
            return;
        }

        var statistic = character.Statistics.FirstOrDefault(s => s.GameMode == gameMode);
        if (statistic != null)
        {
            statistic.Rating = new CharacterRating
            {
                Value = value,
                Deviation = deviation,
                Volatility = volatility,
            };

            statistic.Rating.CompetitiveValue = _competitiveRatingModel.ComputeCompetitiveRating(statistic.Rating);
        }
    }

    public void ResetAllRatings(Character character)
    {
        foreach (GameMode gameMode in Enum.GetValues(typeof(GameMode)))
        {
            ResetRating(character, gameMode);
        }
    }

    public void ResetRating(Character character, GameMode gameMode)
    {
        UpdateRating(character, gameMode, _constants.DefaultRating, _constants.DefaultRatingDeviation,
        _constants.DefaultRatingVolatility);
    }

    public Error? Retire(Character character)
    {
        if (character.Level < _constants.MinimumRetirementLevel)
        {
            return CommonErrors.CharacterLevelRequirementNotMet(_constants.MinimumRetirementLevel, character.Level);
        }

        int heirloomPoints = (int)Math.Pow(2, character.Level - _constants.MinimumRetirementLevel); // to update if level above 31 do not follow the x2 pattern anymore

        character.User!.HeirloomPoints += heirloomPoints;
        character.User.ExperienceMultiplier = Math.Min(
            character.User.ExperienceMultiplier + _constants.ExperienceMultiplierByGeneration,
            _constants.MaxExperienceMultiplierForGeneration);

        character.Generation += 1;
        character.Level = _constants.MinimumLevel;
        character.Experience = 0;
        character.EquippedItems.Clear();
        ResetCharacterCharacteristics(character, respecialization: false);
        return null;
    }

    public void GiveExperience(Character character, int experience, bool useExperienceMultiplier)
    {
        Debug.Assert(experience >= 0, "Given experience should be positive");

        if (character.ForTournament)
        {
            return;
        }

        character.Experience += useExperienceMultiplier
            ? (int)(character.User!.ExperienceMultiplier * experience)
            : experience;
        int newLevel = _experienceTable.GetLevelForExperience(character.Experience);
        int levelDiff = newLevel - character.Level;
        if (levelDiff != 0) // if character leveled up
        {
            for (int i = character.Level; i < newLevel; i++)
            {
                if (i < _constants.HighLevelCutoff) // reward attribute points for lower levels
                {
                    character.Characteristics.Attributes.Points += _constants.AttributePointsPerLevel;
                }
            }

            character.Characteristics.Skills.Points += levelDiff * _constants.SkillPointsPerLevel;
            character.Characteristics.WeaponProficiencies.Points += WeaponProficiencyPointsForLevel(newLevel) - WeaponProficiencyPointsForLevel(character.Level);
            character.Level = newLevel;
        }
    }

    private int WeaponProficiencyPointsForLevel(int lvl) =>
        (int)MathHelper.ApplyPolynomialFunction(lvl, _constants.WeaponProficiencyPointsForLevelCoefs);
}
