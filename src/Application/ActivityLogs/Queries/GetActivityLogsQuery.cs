using AutoMapper;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.ActivityLogs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.ActivityLogs.Queries;

public record GetActivityLogsQuery : IMediatorRequest<ActivityLogWithDictViewModel>
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public int[] UserIds { get; init; } = Array.Empty<int>();
    public ActivityLogType[] Types { get; init; } = Array.Empty<ActivityLogType>();

    public class Validator : AbstractValidator<GetActivityLogsQuery>
    {
        public Validator()
        {
            RuleFor(l => l.From).LessThan(l => l.To);
            RuleForEach(l => l.Types).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<GetActivityLogsQuery, ActivityLogWithDictViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<Result<ActivityLogWithDictViewModel>> Handle(GetActivityLogsQuery req,
            CancellationToken cancellationToken)
        {
            var activityLogs = await _db.ActivityLogs
                .Include(l => l.Metadata)
                .Where(l =>
                    l.CreatedAt >= req.From
                    && l.CreatedAt <= req.To
                    && (req.UserIds.Length == 0 || req.UserIds.Contains(l.UserId))
                    && (req.Types.Length == 0 || req.Types.Contains(l.Type)))
                .OrderByDescending(l => l.CreatedAt)
                .Take(1000)
                .ToArrayAsync(cancellationToken);

            var entitiesFromMetadata = _activityLogService.ExtractEntitiesFromMetadata(activityLogs);
            var clans = await _db.Clans.Where(c => entitiesFromMetadata.ClansIds.Contains(c.Id)).ToArrayAsync();
            var users = await _db.Users.Where(u => entitiesFromMetadata.UsersIds.Contains(u.Id)).ToArrayAsync();
            var characters = await _db.Characters.Where(c => entitiesFromMetadata.CharactersIds.Contains(c.Id)).ToArrayAsync();

            return new(new ActivityLogWithDictViewModel()
            {
                ActivityLogs = _mapper.Map<IList<ActivityLogViewModel>>(activityLogs),
                Dict = new()
                {
                    Clans = _mapper.Map<IList<ClanPublicViewModel>>(clans),
                    Users = _mapper.Map<IList<UserPublicViewModel>>(users),
                    Characters = _mapper.Map<IList<CharacterPublicViewModel>>(characters),
                },
            });
        }
    }
}
