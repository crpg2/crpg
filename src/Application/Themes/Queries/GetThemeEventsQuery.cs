using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Crpg.Domain.Entities.Themes;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Queries;

public class GetThemeEventsQuery : IMediatorRequest<IList<ThemeEventViewModel>>
{
    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<GetThemeEventsQuery, IList<ThemeEventViewModel>>
    {
        public async ValueTask<Result<IList<ThemeEventViewModel>>> Handle(GetThemeEventsQuery req, CancellationToken cancellationToken)
        {
            var themeEvents = await db.ThemeEvents.Include(x => x.EventTheme).AsNoTracking().ToListAsync(cancellationToken);

            var themeIds = themeEvents.Select(te => te.EventTheme.Id).Distinct().ToList();

            var eligibleItemsByTheme = await db.Items
                    .Where(item => item.Enabled && item.Themes.Any(itemTheme => themeIds.Contains(itemTheme.Id)))
                    .Select(item => new { item.Id, ThemeIds = item.Themes.Select(t => t.Id) })
                    .ToListAsync(cancellationToken);

            var itemIdsByTheme = eligibleItemsByTheme
                .SelectMany(itemAndThemeIds => itemAndThemeIds.ThemeIds, (item, themeId) => new { item.Id, ThemeId = themeId })
                .GroupBy(x => x.ThemeId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var viewModels = themeEvents.Select(themeEvent => MapToViewModel(themeEvent, itemIdsByTheme)).ToList();

            return new(viewModels);
        }
    }

    private static ThemeEventViewModel MapToViewModel(ThemeEvent themeEvent, Dictionary<int, List<string>> itemIdsByTheme)
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
            EventTheme = new ThemeViewModel { Id = themeEvent.EventTheme.Id,  Name = themeEvent.EventTheme.Name },
            EligibleItemIds = itemIds ?? new List<string>(),
        };
    }
}
