using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands;

public record UpdateSettlementCommand : IMediatorRequest<SettlementPublicViewModel>
{
    [JsonIgnore]
    public int PartyId { get; init; }
    [JsonIgnore]
    public int SettlementId { get; init; }
    public int Troops { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, Constants constants) : IMediatorRequestHandler<UpdateSettlementCommand, SettlementPublicViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateSettlementCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly Constants _constants = constants;

        public async ValueTask<Result<SettlementPublicViewModel>> Handle(UpdateSettlementCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
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

            var settlement = await _db.Settlements
                .FirstOrDefaultAsync(h => h.Id == req.SettlementId, cancellationToken);
            if (settlement == null)
            {
                return new(CommonErrors.SettlementNotFound(req.SettlementId));
            }

            int troopsDelta = req.Troops - settlement.Troops;
            if (troopsDelta >= 0) // Party troops -> settlement troops.
            {
                if (party.Troops < troopsDelta + _constants.StrategusMinPartyTroops)
                {
                    return new(CommonErrors.PartyNotEnoughTroops(party.Id));
                }
            }
            else // Settlement troops -> party troops.
            {
                if (settlement.OwnerId != party.Id)
                {
                    return new(CommonErrors.PartyNotSettlementOwner(party.Id, settlement.Id));
                }

                if (settlement.Troops < Math.Abs(troopsDelta) + _constants.StrategusMinPartyTroops)
                {
                    return new(CommonErrors.SettlementNotEnoughTroops(settlement.Id));
                }
            }

            settlement.Troops += troopsDelta;
            party.Troops -= troopsDelta;

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Party '{0}' {1} settlement '{2}'", req.PartyId,
                troopsDelta >= 0 ? "gave troops to" : "took troops from", settlement.Id);
            return new(_mapper.Map<SettlementPublicViewModel>(settlement));
        }
    }
}
