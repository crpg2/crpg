using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleMercenaryApplicationsQuery : IMediatorRequest<IList<BattleMercenaryApplicationViewModel>>
{
    public int UserId { get; init; }
    public int BattleId { get; init; }
    public IList<BattleMercenaryApplicationStatus> Statuses { get; init; } = Array.Empty<BattleMercenaryApplicationStatus>();

    public class Validator : AbstractValidator<GetBattleMercenaryApplicationsQuery>
    {
        public Validator()
        {
            RuleFor(a => a.Statuses).ForEach(s => s.IsInEnum());
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetBattleMercenaryApplicationsQuery, IList<BattleMercenaryApplicationViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<IList<BattleMercenaryApplicationViewModel>>> Handle(GetBattleMercenaryApplicationsQuery req, CancellationToken cancellationToken)
        {
            var battleInfo = await _db.Battles
                .Where(b => b.Id == req.BattleId)
                .Select(b => new { b.Id, b.Phase, })
                .FirstOrDefaultAsync(cancellationToken);

            if (battleInfo == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            // Mercenaries can only apply during the Hiring phase so return an error for preceding phases.
            if (battleInfo.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battleInfo.Phase));
            }

            BattleSide? fightersSide = await _db.BattleFighters
                .Where(f => f.BattleId == req.BattleId && f.PartyId == req.UserId)
                .Select(f => (BattleSide?)f.Side)
                .FirstOrDefaultAsync(cancellationToken);

            if (fightersSide == null)
            {
                return new(CommonErrors.PartyNotAFighter(req.UserId, req.BattleId));
            }

            var applications = await _db.BattleMercenaryApplications
                .Where(a =>
                    a.BattleId == req.BattleId &&
                    req.Statuses.Contains(a.Status) &&
                    (
                        // If the user is not a fighter of that battle, only return its applications,
                        // else return the mercenary applications from the same side as the user.
                        a.Character!.UserId == req.UserId || a.Side == fightersSide))
                .OrderByDescending(a => a.CreatedAt)
                .ProjectTo<BattleMercenaryApplicationViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            return new(applications);
        }
    }
}
