using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands;

public record UpdateSettlementCommand : IMediatorRequest<SettlementPublicViewModel>
{
    public int PartyId { get; init; }
    public int SettlementId { get; init; }
    public int Troops { get; init; }

    // TODO: fix SPEC
    // public class Validator : AbstractValidator<UpdateSettlementCommand>
    // {
    //     public Validator()
    //     {
    //         RuleFor(c => c.Troops).GreaterThanOrEqualTo(0);
    //     }
    // }

    internal class Handler : IMediatorRequestHandler<UpdateSettlementCommand, SettlementPublicViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateSettlementCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<SettlementPublicViewModel>> Handle(UpdateSettlementCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .Include(h => h.CurrentSettlement)
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if ((party.Status != PartyStatus.IdleInSettlement
                 && party.Status != PartyStatus.RecruitingInSettlement)
                || party.CurrentSettlementId != req.SettlementId)
            {
                return new(CommonErrors.PartyNotInASettlement(party.Id));
            }

            int troopsDelta = req.Troops - party.CurrentSettlement!.Troops;
            if (troopsDelta >= 0) // Party troops -> settlement troops.
            {
                if (party.Troops < troopsDelta)
                {
                    return new(CommonErrors.PartyNotEnoughTroops(party.Id));
                }
            }
            else // Settlement troops -> party troops.
            {
                if (party.CurrentSettlement!.OwnerId != party.Id)
                {
                    return new(CommonErrors.PartyNotSettlementOwner(party.Id, party.CurrentSettlementId!.Value));
                }
            }

            party.CurrentSettlement.Troops += troopsDelta;
            party.Troops -= troopsDelta;

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Party '{0}' {1} settlement '{2}'", req.PartyId,
                troopsDelta >= 0 ? "gave troops to" : "took troops from", party.CurrentSettlementId);
            return new(_mapper.Map<SettlementPublicViewModel>(party.CurrentSettlement));
        }
    }
}
