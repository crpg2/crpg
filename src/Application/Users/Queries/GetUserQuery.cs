using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Users.Queries;

public record GetUserQuery : IMediatorRequest<UserViewModel>
{
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IUserService userService) : IMediatorRequestHandler<GetUserQuery, UserViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IUserService _userService = userService;

        public async ValueTask<Result<UserViewModel>> Handle(GetUserQuery req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .AsSplitQuery()
                .Include(u => u.Offers)
                    .ThenInclude(o => o.Assets)
                .ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            user.IsRecent = await _userService.CheckIsRecentUser(_db, user.Id, cancellationToken);

            return new(user);
        }
    }
}
