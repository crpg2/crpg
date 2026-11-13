using Crpg.Application.Themes.Models;
using Crpg.Domain.Entities.Themes;

namespace Crpg.Application.Themes.Queries;

/// <summary>
/// Pure mapping from a <see cref="ThemeEvent"/> to its <see cref="ThemeEventViewModel"/>. The eligible item ids are
/// resolved by the caller (it's a database concern) and passed in here.
/// </summary>
internal static class ThemeEventMapper
{
    public static ThemeEventViewModel ToViewModel(ThemeEvent themeEvent, IReadOnlyDictionary<int, List<string>> itemIdsByTheme)
    {
        itemIdsByTheme.TryGetValue(themeEvent.EventTheme.Id, out List<string>? itemIds);

        return new ThemeEventViewModel
        {
            Id = themeEvent.Id,
            Name = themeEvent.Name,
            GoldMultiplier = themeEvent.GoldMultiplier,
            ExpMultiplier = themeEvent.ExpMultiplier,
            ActiveFromUtc = themeEvent.ActiveFromUtc,
            ActiveUntilUtc = themeEvent.ActiveUntilUtc,
            RequiredEquipmentSlotsMatchingTheme = themeEvent.RequiredEquipmentSlotsMatchingTheme,
            MinimumThemedItemsEquipped = themeEvent.MinimumThemedItemsEquipped,
            EventTheme = new ThemeViewModel { Id = themeEvent.EventTheme.Id, Name = themeEvent.EventTheme.Name },
            EligibleItemIds = itemIds ?? new List<string>(),
        };
    }
}
