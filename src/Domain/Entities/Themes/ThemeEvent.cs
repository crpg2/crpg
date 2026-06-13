using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Themes;

public class ThemeEvent : AuditableEntity
{
    public ThemeEvent(
            string name,
            float goldMultiplier,
            float expMultiplier,
            DateTimeOffset activeFromUtc,
            DateTimeOffset? activeUntilUtc,
            List<ThemeEquipmentSlot> requiredEquipmentSlotsMatchingTheme,
            int minumumRequiredEquipmentSlotsMatchingTheme,
            Theme theme)
    {
        Name = name;
        GoldMultiplier = goldMultiplier;
        ExpMultiplier = expMultiplier;
        ActiveFromUtc = activeFromUtc;
        ActiveUntilUtc = activeUntilUtc;
        RequiredEquipmentSlotsMatchingTheme = requiredEquipmentSlotsMatchingTheme;
        MinumumRequiredEquipmentSlotsMatchingTheme = minumumRequiredEquipmentSlotsMatchingTheme;
        EventTheme = theme;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // this contructor exists only because Ef core requires it.
    private ThemeEvent()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public int Id { get; set; }

    /// <summary>
    /// The name of the event.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The gold multiplier to apply to the rewards earned with this theme.
    /// </summary>
    public float GoldMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// The experience multiplier to apply to the rewards earned with this theme.
    /// </summary>
    public float ExpMultiplier { get; set; } = 1.0f;

    /// <summary>
    /// The time from which the theme is active.
    /// </summary>
    public DateTimeOffset ActiveFromUtc { get; set; }

    /// <summary>
    /// The time where the theme stops being active. If null, the theme is active indefinitely.
    /// </summary>
    public DateTimeOffset? ActiveUntilUtc { get; set; }

    /// <summary>
    /// The equipment slots that must contain items matching the events theme for the player to be eligible for the event rewards.
    /// </summary>
    public List<ThemeEquipmentSlot> RequiredEquipmentSlotsMatchingTheme { get; set; } = new();

    /// <summary>
    /// The minimum amount of equipment slots that must contain items matching the events theme for the player to be eligible for the event rewards.
    /// </summary>
    public int MinumumRequiredEquipmentSlotsMatchingTheme { get; set; }

    /// <summary>
    /// The theme of the event.
    /// </summary>
    public Theme EventTheme { get; set; }
}
