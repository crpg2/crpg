using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Themes;

namespace Crpg.Application.Themes.Models;

public class ThemeEventViewModel : IMapFrom<ThemeEvent>
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the event.
    /// </summary>
    public string Name { get; set; } = default!;

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
    public ThemeViewModel EventTheme { get; set; } = default!;

    public List<string> EligibleItemIds { get; set; } = new();

    public void Mapping(Profile profile) => profile.CreateMap<ThemeEvent, ThemeEventViewModel>()
        .ForMember(dest => dest.EligibleItemIds, opt => opt.Ignore());
}
