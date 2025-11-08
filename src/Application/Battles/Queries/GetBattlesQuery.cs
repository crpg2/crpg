using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattlesQuery : IMediatorRequest<IList<BattleDetailedViewModel>>
{
    public Region Region { get; init; }
    public BattleType? Type { get; init; }
    public IList<BattlePhase> Phases { get; init; } = Array.Empty<BattlePhase>();

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

    internal class Handler : IMediatorRequestHandler<GetBattlesQuery, IList<BattleDetailedViewModel>>
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

        public async Task<Result<IList<BattleDetailedViewModel>>> Handle(GetBattlesQuery req, CancellationToken cancellationToken)
        {
            var battles = await _db.Battles
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
                .Where(b =>
                    b.Region == req.Region &&
                    req.Phases.Contains(b.Phase) &&
                    (req.Type == null || (req.Type == BattleType.Siege
                            ? b.Fighters.Any(f => f.Side == BattleSide.Defender && f.Commander && f.Settlement != null)
                            : b.Fighters.Any(f => f.Side == BattleSide.Defender && f.Commander && f.Settlement == null))))
                .OrderBy(b => b.ScheduledFor)
                .ToArrayAsync(cancellationToken);

            var battlesVm = battles.Select(b => _battleService.MapToBattleDetailedViewModel(_mapper, b)).ToArray();

            return new(battlesVm);
        }
    }
}
