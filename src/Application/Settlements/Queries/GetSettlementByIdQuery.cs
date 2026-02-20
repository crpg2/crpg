using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries;

public record GetSettlementByIdQuery : IMediatorRequest<SettlementPublicViewModel>
{
    public int SettlementId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetSettlementByIdQuery, SettlementPublicViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<SettlementPublicViewModel>> Handle(GetSettlementByIdQuery req, CancellationToken cancellationToken)
        {
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
