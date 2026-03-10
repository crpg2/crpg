using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleItemsQuery : IMediatorRequest<IList<BattleFighterInventoryViewModel>>
{
    public int PartyId { get; init; }

    public int BattleId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetBattleItemsQuery, IList<BattleFighterInventoryViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<IList<BattleFighterInventoryViewModel>>> Handle(GetBattleItemsQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles.FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            BattleSide? battleSide = await _db.BattleFighters
                .Where(f => f.BattleId == req.BattleId && f.PartyId == req.PartyId)
                .Select(f => (BattleSide?)f.Side)
                .FirstOrDefaultAsync(cancellationToken);
            if (battleSide == null)
            {
                return new(CommonErrors.PartyNotAFighter(req.PartyId, req.BattleId));
            }

            var fighters = await _db.BattleFighters
                .AsSplitQuery()
                .Where(f => f.BattleId == req.BattleId && f.Side == battleSide)
                .Include(f => f.Party)
                    .ThenInclude(p => p!.Items)
                        .ThenInclude(i => i.Item)
                .Include(f => f.Party)
                    .ThenInclude(p => p!.User!.ClanMembership!.Clan)
                .Include(f => f.Settlement)
                    .ThenInclude(s => s!.Items)
                        .ThenInclude(i => i.Item)
                .Include(f => f.Settlement)
                    .ThenInclude(s => s!.Owner!.User!.ClanMembership!.Clan)
                .Include(f => f.Settlement)
                .ToListAsync(cancellationToken);

            var result = fighters.Select(f => new BattleFighterInventoryViewModel
            {
                FighterId = f.Id,
                Party = _mapper.Map<PartyPublicViewModel>(f.Party),
                Settlement = _mapper.Map<SettlementPublicViewModel>(f.Settlement),
                Items =
                    f.Party != null
                        ? [.. f.Party.Items.Select(pi => new ItemStack { Count = pi.Count, Item = _mapper.Map<ItemViewModel>(pi.Item!), })]
                        : [.. f.Settlement!.Items.Select(si => new ItemStack { Count = si.Count, Item = _mapper.Map<ItemViewModel>(si.Item!), })],
            }).ToList();

            return new(result);
        }
    }
}
