// FIXME: UNIT
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries;

public record GetUsersByNameQuery : IMediatorRequest<UserPrivateViewModel[]>
{
    public string Name { get; init; } = string.Empty;

    internal class Handler : IMediatorRequestHandler<GetUsersByNameQuery, UserPrivateViewModel[]>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<UserPrivateViewModel[]>> Handle(GetUsersByNameQuery req, CancellationToken cancellationToken)
        {
            return new(await _db.Users
                .Where(u => u.Name.ToLower().Contains(req.Name.ToLower()))
                .Take(50)
                .ProjectTo<UserPrivateViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken));
        }
    }
}
