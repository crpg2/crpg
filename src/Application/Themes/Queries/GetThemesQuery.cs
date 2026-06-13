using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Queries;

public class GetThemesQuery : IMediatorRequest<IList<ThemeViewModel>>
{
    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetThemesQuery, IList<ThemeViewModel>>
    {
        public async ValueTask<Result<IList<ThemeViewModel>>> Handle(GetThemesQuery req, CancellationToken cancellationToken)
        {
            var themes = await db.Themes.AsNoTracking().ToListAsync(cancellationToken);
            var viewModels = mapper.Map<IList<ThemeViewModel>>(themes);

            return new(viewModels);
        }
    }
}
