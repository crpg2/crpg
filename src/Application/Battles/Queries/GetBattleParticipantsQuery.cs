using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleParticipantsQuery : IMediatorRequest<IList<BattleParticipantViewModel>>
{
    public int BattleId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetBattleParticipantsQuery, IList<BattleParticipantViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<IList<BattleParticipantViewModel>>> Handle(GetBattleParticipantsQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles.FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var battleParticipants = await _db.BattleParticipants
                .AsSplitQuery()
                .Where(p => p.BattleId == req.BattleId)
                .ProjectTo<BattleParticipantViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            return new(battleParticipants);
        }
    }
}
