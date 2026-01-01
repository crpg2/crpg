using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleQuery : IMediatorRequest<BattleDetailedViewModel>
{
    public int BattleId { get; init; }

    [JsonIgnore]
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetBattleQuery, BattleDetailedViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IBattleService _battleService;

        public Handler(ICrpgDbContext db, IMapper mapper, IBattleService battleService)
        {
            _db = db;
            _mapper = mapper;
            _battleService = battleService;
        }

        public async Task<Result<BattleDetailedViewModel>> Handle(GetBattleQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                // TODO: FIXME: optimize
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Party!.User)
                        .ThenInclude(u => u!.ClanMembership)
                            .ThenInclude(c => c!.Clan)
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Settlement)
                        .ThenInclude(s => s!.Owner)
                            .ThenInclude(o => o!.User)
                                .ThenInclude(u => u!.ClanMembership)
                                    .ThenInclude(c => c!.Clan)
                .Include(b => b.MercenaryApplications)
                    .ThenInclude(a => a.Character)
                .Include(b => b.SideBriefings)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            // Battles in preparation shouldn't be visible to anyone but only to parties in sight on the map.
            if (battle.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            return new(_battleService.MapBattleToDetailedViewModel(_mapper, battle, req.UserId));
        }
    }
}
