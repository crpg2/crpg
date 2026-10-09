using System.Diagnostics;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Common.Helpers;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Servers;
using Microsoft.EntityFrameworkCore;

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

    Task<Result> UpdateItems(ICrpgDbContext db, Character character, IList<EquippedItemIdViewModel> items, CancellationToken cancellationToken);
}

/// <inheritdoc />
internal class CharacterService : ICharacterService
{
    private static readonly ItemSlot[] WeaponSlots =
    {
            ItemSlot.Weapon0,
            ItemSlot.Weapon1,
            ItemSlot.Weapon2,
            ItemSlot.Weapon3,
    };

    private static readonly Dictionary<ItemType, ItemSlot[]> ItemSlotsByType = new()
    {
        [ItemType.HeadArmor] = new[] { ItemSlot.Head },
        [ItemType.ShoulderArmor] = new[] { ItemSlot.Shoulder },
        [ItemType.BodyArmor] = new[] { ItemSlot.Body },
        [ItemType.HandArmor] = new[] { ItemSlot.Hand },
        [ItemType.LegArmor] = new[] { ItemSlot.Leg },
        [ItemType.MountHarness] = new[] { ItemSlot.MountHarness },
        [ItemType.Mount] = new[] { ItemSlot.Mount },
        [ItemType.Shield] = WeaponSlots,
        [ItemType.Bow] = WeaponSlots,
        [ItemType.Crossbow] = WeaponSlots,
        [ItemType.OneHandedWeapon] = WeaponSlots,
        [ItemType.TwoHandedWeapon] = WeaponSlots,
        [ItemType.Polearm] = WeaponSlots,
        [ItemType.Thrown] = WeaponSlots,
        [ItemType.Arrows] = WeaponSlots,
        [ItemType.Bolts] = WeaponSlots,
        [ItemType.Pistol] = WeaponSlots,
        [ItemType.Musket] = WeaponSlots,
        [ItemType.Bullets] = WeaponSlots,
        [ItemType.Banner] = new[] { ItemSlot.WeaponExtra },
    };

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

    public async Task<Result> UpdateItems(ICrpgDbContext db, Character character, IList<EquippedItemIdViewModel> items, CancellationToken cancellationToken)
    {
        int[] newUserItemIds = items
                .Where(ei => ei.UserItemId != null)
                .Select(ei => ei.UserItemId!.Value)
                .ToArray();

        Dictionary<int, UserItem> userItemsById = await db.UserItems
                        .Include(ui => ui.Item)
                        .Include(ui => ui.ClanArmoryItem)
                        .Include(ui => ui.PersonalItem)
                        .Where(ui =>
                            (ui.ClanArmoryBorrowedItem!.BorrowerUserId == character.UserId || (ui.UserId == character.UserId && ui.ClanArmoryItem == null))
                            && newUserItemIds.Contains(ui.Id))
                        .ToDictionaryAsync(ui => ui.Id, cancellationToken);

        Dictionary<ItemSlot, EquippedItem> oldItemsBySlot = character.EquippedItems.ToDictionary(c => c.Slot);

        foreach (EquippedItemIdViewModel newEquippedItem in items)
        {
            EquippedItem? equippedItem;
            if (newEquippedItem.UserItemId == null) // If null, remove item in the slot.
            {
                if (oldItemsBySlot.TryGetValue(newEquippedItem.Slot, out equippedItem))
                {
                    character.EquippedItems.Remove(equippedItem);
                }

                continue;
            }

            if (!userItemsById.TryGetValue(newEquippedItem.UserItemId.Value, out UserItem? userItem))
            {
                return new(CommonErrors.UserItemNotFound(newEquippedItem.UserItemId.Value));
            }

            if (!userItem.Item!.Enabled && userItem.PersonalItem == null)
            {
                return new(CommonErrors.ItemDisabled(userItem.ItemId));
            }

            if (userItem.IsBroken)
            {
                return new(CommonErrors.ItemBroken(userItem.ItemId));
            }

            if ((userItem.Item!.Flags & (ItemFlags.DropOnAnyAction | ItemFlags.DropOnWeaponChange)) != 0)
            {
                if (newEquippedItem.Slot != ItemSlot.WeaponExtra)
                {
                    return new(CommonErrors.ItemBadSlot(userItem.ItemId, newEquippedItem.Slot));
                }
            }
            else if (!ItemSlotsByType[userItem.Item!.Type].Contains(newEquippedItem.Slot))
            {
                return new(CommonErrors.ItemBadSlot(userItem.ItemId, newEquippedItem.Slot));
            }

            if (oldItemsBySlot.TryGetValue(newEquippedItem.Slot, out equippedItem))
            {
                // Character already has an item in this slot. Replace it.
                equippedItem.UserItem = userItem;
            }
            else
            {
                // Character has no item in this slot. Create it.
                equippedItem = new EquippedItem
                {
                    CharacterId = character.Id,
                    Slot = newEquippedItem.Slot,
                    UserItem = userItem,
                };

                character.EquippedItems.Add(equippedItem);
            }
        }

        return Result.NoErrors;
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
