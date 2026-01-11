using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Terrains;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattlesQuery : IMediatorRequest<IList<BattleDetailedViewModel>>
{
    public Region Region { get; init; }
    public BattleType? Type { get; init; }
    public IList<BattlePhase> Phases { get; init; } = Array.Empty<BattlePhase>();

    [JsonIgnore]
    public int UserId { get; init; }

    public class Validator : AbstractValidator<GetBattlesQuery>
    {
        public Validator()
        {
            RuleFor(q => q.Region).IsInEnum();
            RuleFor(q => q.Phases).ForEach(p =>
            {
                p.IsInEnum().NotEqual(BattlePhase.Preparation);
            });
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IBattleService battleService) : IMediatorRequestHandler<GetBattlesQuery, IList<BattleDetailedViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IBattleService _battleService = battleService;

        public async Task<Result<IList<BattleDetailedViewModel>>> Handle(GetBattlesQuery req, CancellationToken cancellationToken)
        {
            var battles = await _db.Battles
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
                .Where(b => b.Region == req.Region && req.Phases.Contains(b.Phase))
                .Select(b => new
                {
                    Battle = b,
                    NearestSettlement = _db.Settlements.OrderBy(s => s.Position.Distance(b.Position)).FirstOrDefault(),
                    Terrain = _db.Terrains.FirstOrDefault(t => t.Boundary.Covers(b.Position)) ?? new() { Type = TerrainType.Plain, },
                })
                // .OrderByDescending(b => b.ScheduledFor) // TODO: FIXME:
                .ToArrayAsync(cancellationToken);

            return new(battles.Select(b =>
                _battleService.MapBattleToDetailedViewModel(
                    _mapper,
                    b.Battle,
                    req.UserId,
                    b.NearestSettlement!,
                    b.Terrain))
            .ToArray());
        }
    }
}
