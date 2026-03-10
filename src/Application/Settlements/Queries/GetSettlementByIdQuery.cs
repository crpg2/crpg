using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries;

public record GetSettlementByIdQuery : IMediatorRequest<SettlementPublicViewModel>
{
    public int PartyId { get; init; }
    public int SettlementId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetSettlementByIdQuery, SettlementPublicViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<SettlementPublicViewModel>> Handle(GetSettlementByIdQuery req, CancellationToken cancellationToken)
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

            var settlement = await _db.Settlements
                .Include(s => s.Owner!.User!.ClanMembership!.Clan)
                .ProjectTo<SettlementPublicViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(s => s.Id == req.SettlementId, cancellationToken);

            return settlement == null
                ? new(CommonErrors.SettlementNotFound(req.SettlementId))
                : new(settlement);
        }
    }
}
