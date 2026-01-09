using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Terrains;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleQuery : IMediatorRequest<BattleDetailedViewModel>
{
    public int BattleId { get; init; }

    [JsonIgnore]
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IBattleService battleService) : IMediatorRequestHandler<GetBattleQuery, BattleDetailedViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IBattleService _battleService = battleService;

        public async Task<Result<BattleDetailedViewModel>> Handle(GetBattleQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Party!.User!.ClanMembership!.Clan)
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Settlement!.Owner!.User!.ClanMembership!.Clan)
                .Include(b => b.MercenaryApplications)
                    .ThenInclude(a => a.Character)
                .Include(b => b.Participants)
                    .ThenInclude(p => p.Character)
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

            var nearestSettlement = await _db.Settlements
                .OrderBy(s => s.Position.Distance(battle.Position))
                .FirstAsync(cancellationToken);

            var battleTerrain = await _db.Terrains
                .FirstOrDefaultAsync(t => t.Boundary.Covers(battle.Position), cancellationToken) ?? new() { Type = TerrainType.Plain, };

            return new(_battleService.MapBattleToDetailedViewModel(_mapper, battle, req.UserId, nearestSettlement, battleTerrain));
        }
    }
}
