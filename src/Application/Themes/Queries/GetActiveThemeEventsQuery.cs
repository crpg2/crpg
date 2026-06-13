using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Queries;

public class GetActiveThemeEventsQuery : IMediatorRequest<IList<ThemeEventViewModel>>
{
    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetActiveThemeEventsQuery, IList<ThemeEventViewModel>>
    {
        public async ValueTask<Result<IList<ThemeEventViewModel>>> Handle(GetActiveThemeEventsQuery req, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var activeThemes = await db.ThemeEvents.Include(x => x.EventTheme).Where(theme => now >= theme.ActiveFromUtc && (theme.ActiveUntilUtc == null || now <= theme.ActiveUntilUtc))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var viewModels = mapper.Map<IList<ThemeEventViewModel>>(activeThemes);

            return new(viewModels);
        }
    }
}
