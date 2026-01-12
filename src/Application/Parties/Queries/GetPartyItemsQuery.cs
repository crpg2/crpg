using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Parties.Queries;

public record GetPartyItemsQuery : IMediatorRequest<IList<ItemStack>>
{
    public int PartyId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetPartyItemsQuery, IList<ItemStack>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<IList<ItemStack>>> Handle(GetPartyItemsQuery req, CancellationToken cancellationToken)
        {
            /*
            / TODO: FIXME: добавить проверок, чтобы не было уязвимости
            Инвентарь могут получать только:
                - сам отряд
                - лидер/командиры клана
                - командир битвы, когда он смотрит battleFighterApplication
            */

            var party = await _db.Parties
                .AsSplitQuery()
                .Include(p => p.Items!)
                    .ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);

            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            return new(_mapper.Map<IList<ItemStack>>(party.Items));
        }
    }
}
