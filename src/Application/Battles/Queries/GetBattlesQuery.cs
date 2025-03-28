using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattlesQuery : IMediatorRequest<IList<BattleDetailedViewModel>>
{
    public Region Region { get; init; }
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

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<BattleDetailedViewModel>>> Handle(GetBattlesQuery req, CancellationToken cancellationToken)
        {
            var battles = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters).ThenInclude(f => f.Party!.User)
                .Include(b => b.Fighters).ThenInclude(f => f.Settlement)
                .Where(b => b.Region == req.Region && req.Phases.Contains(b.Phase))
                .ToArrayAsync(cancellationToken);

            return new(_mapper.Map<IList<BattleDetailedViewModel>>(battles));
        }
    }
}
