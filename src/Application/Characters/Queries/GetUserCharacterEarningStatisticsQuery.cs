using AutoMapper;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetUserCharacterEarningStatisticsQuery : IMediatorRequest<IList<ActivityLogViewModel>>
{
    public DateTime From { get; init; }
    public DateTime? To { get; init; }
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserCharacterEarningStatisticsQuery, IList<ActivityLogViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime)
        {
            _db = db;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async ValueTask<Result<IList<ActivityLogViewModel>>> Handle(GetUserCharacterEarningStatisticsQuery req,
            CancellationToken cancellationToken)
        {
            var activityLogs = await _db.ActivityLogs
                .Include(l => l.Metadata)
                .Where(l =>
                    l.UserId == req.UserId
                    && l.Type == ActivityLogType.CharacterEarned
                    && l.CreatedAt >= req.From && l.CreatedAt <= req.To.GetValueOrDefault(_dateTime.UtcNow)
                    && Convert.ToInt32(l.Metadata.First(m => m.Key == "characterId").Value) == req.CharacterId)
                .ToArrayAsync(cancellationToken);

            return new(_mapper.Map<IList<ActivityLogViewModel>>(activityLogs));
        }
    }
}
