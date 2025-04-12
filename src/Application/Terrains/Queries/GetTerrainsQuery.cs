using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Terrains.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries;

public record GetTerrainsQuery : IMediatorRequest<IList<TerrainViewModel>>
{
    internal class Handler : IMediatorRequestHandler<GetTerrainsQuery, IList<TerrainViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<TerrainViewModel>>> Handle(GetTerrainsQuery req, CancellationToken cancellationToken)
        {
            var terrains = await _db.Terrains
                .ProjectTo<TerrainViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            return new(terrains);
        }
    }
}
