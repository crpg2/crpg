using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries;

public record GetUserByIdQuery : IMediatorRequest<UserPrivateViewModel>
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserByIdQuery, UserPrivateViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<UserPrivateViewModel>> Handle(GetUserByIdQuery req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .ProjectTo<UserPrivateViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            return user == null
                ? new(CommonErrors.UserNotFound(req.UserId))
                : new(user);
        }
    }
}
