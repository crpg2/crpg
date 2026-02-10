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

    internal class Handler : IMediatorRequestHandler<GetSettlementByIdQuery, SettlementPublicViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<SettlementPublicViewModel>> Handle(GetSettlementByIdQuery req, CancellationToken cancellationToken)
        {
            var settlement = await _db.Settlements
                .Include(s => s.Owner!.User!.ClanMembership!.Clan)
                .ProjectTo<SettlementPublicViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(s => s.Id == req.SettlementId, cancellationToken);

            return settlement == null
                ? new(CommonErrors.UserNotFound(req.SettlementId)) // TODO: FIXME:L SettlementNotFound
                : new(settlement);
        }
    }
}
