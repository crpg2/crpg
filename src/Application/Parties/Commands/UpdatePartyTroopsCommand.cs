using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyTroopsCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler(ICrpgDbContext db, Constants constants) : IMediatorRequestHandler<UpdatePartyTroopsCommand>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly Constants _constants = constants;

        public async ValueTask<Result> Handle(UpdatePartyTroopsCommand req, CancellationToken cancellationToken)
        {
            float deltaTimeHours = (float)req.DeltaTime.TotalHours;
            float recruits = deltaTimeHours * _constants.StrategusTroopRecruitmentPerHour;

            var parties = _db.Parties
                .Where(h => h.Status == PartyStatus.RecruitingInSettlement)
                .AsAsyncEnumerable();

            await foreach (var party in parties.WithCancellation(cancellationToken))
            {
                party.Troops = Math.Min(party.Troops + recruits, _constants.StrategusMaxPartyTroops);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}
