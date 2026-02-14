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

        public async ValueTask<Result<IList<ItemStack>>> Handle(GetPartyItemsQuery req, CancellationToken cancellationToken)
        {
            /*
            / TODO: FIXME: SPEC
             Add checks to prevent vulnerabilities.
            Only the following can receive inventory:
            - The party itself
            - Clan leaders/commanders
            - The battle commander when viewing battleFighterApplication
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
