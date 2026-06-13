namespace Crpg.Module.Api.Models.Themes;

internal class ThemeEvent
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public float GoldMultiplier { get; set; } = 1.0f;

    public float ExpMultiplier { get; set; } = 1.0f;

    public DateTimeOffset ActiveFromUtc { get; set; }

    public DateTimeOffset? ActiveUntilUtc { get; set; }

    public List<ThemeEquipmentSlot> RequiredEquipmentSlotsMatchingTheme { get; set; } = new();

    public int MinumumRequiredEquipmentSlotsMatchingTheme { get; set; }

    public Theme EventTheme { get; set; } = default!;
}
