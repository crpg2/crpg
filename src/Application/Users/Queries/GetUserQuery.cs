using AutoMapper;
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

    internal class Handler : IMediatorRequestHandler<GetUserQuery, UserViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public Handler(ICrpgDbContext db, IMapper mapper, IUserService userService)
        {
            _db = db;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<Result<UserViewModel>> Handle(GetUserQuery req, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            bool isRecent = await _userService.CheckIsRecentUser(_db, user);

            var userVm = _mapper.Map<UserViewModel>(user);
            userVm.IsRecent = isRecent;

            return new(userVm);
        }
    }
}
