using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Queries;

public class GetThemeEventsQuery : IMediatorRequest<IList<ThemeEventViewModel>>
{
    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetThemeEventsQuery, IList<ThemeEventViewModel>>
    {
        public async ValueTask<Result<IList<ThemeEventViewModel>>> Handle(GetThemeEventsQuery req, CancellationToken cancellationToken)
        {
            var themes = await db.ThemeEvents.AsNoTracking().ToListAsync(cancellationToken);
            var viewModels = mapper.Map<IList<ThemeEventViewModel>>(themes);

            return new(viewModels);
        }
    }
}
