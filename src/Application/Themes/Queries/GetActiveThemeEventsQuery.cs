using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Crpg.Domain.Entities.Themes;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Queries;

public class GetActiveThemeEventsQuery : IMediatorRequest<IList<ThemeEventViewModel>>
{
    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<GetActiveThemeEventsQuery, IList<ThemeEventViewModel>>
    {
        public async ValueTask<Result<IList<ThemeEventViewModel>>> Handle(GetActiveThemeEventsQuery req, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var activeThemeEvents = await db.ThemeEvents
                .Include(x => x.EventTheme)
                .Where(te => te.ActiveFromUtc <= now && (te.ActiveUntilUtc == null || te.ActiveUntilUtc > now))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var themeIds = activeThemeEvents.Select(te => te.EventTheme.Id).Distinct().ToList();

            var eligibleItemsByTheme = await db.Items
                    .Where(item => item.Enabled && item.Themes.Any(itemTheme => themeIds.Contains(itemTheme.Id)))
                    .Select(item => new { item.Id, ThemeIds = item.Themes.Select(t => t.Id) })
                    .ToListAsync(cancellationToken);

            var itemIdsByTheme = eligibleItemsByTheme
                .SelectMany(itemAndThemeIds => itemAndThemeIds.ThemeIds, (item, themeId) => new { item.Id, ThemeId = themeId })
                .GroupBy(x => x.ThemeId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var viewModels = activeThemeEvents.Select(themeEvent => ThemeEventMapper.ToViewModel(themeEvent, itemIdsByTheme)).ToList();

            return new(viewModels);
        }
    }
}
