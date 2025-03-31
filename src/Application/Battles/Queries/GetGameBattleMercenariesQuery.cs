using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetGameBattleMercenariesQuery : IMediatorRequest<IList<BattleMercenaryViewModel>>
{
    public int BattleId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetGameBattleMercenariesQuery, IList<BattleMercenaryViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterClassResolver _characterClassResolver;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterClassResolver characterClassResolver)
        {
            _db = db;
            _mapper = mapper;
            _characterClassResolver = characterClassResolver;
        }

        public async Task<Result<IList<BattleMercenaryViewModel>>> Handle(GetGameBattleMercenariesQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters)
                .Include(b => b.Mercenaries).ThenInclude(m => m.Character!.User)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            // Mercenaries can only apply during the Hiring phase so return an error for preceding phases.
            if (battle.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            // Return the mercenaries from the same side as the user during the hiring phase.
            var mercenaries = battle.Mercenaries
                .Where(m => battle.Phase != BattlePhase.Hiring)
                .Select(m => new BattleMercenaryViewModel
                {
                    Id = m.Id,
                    User = _mapper.Map<UserPublicViewModel>(m.Character!.User),
                    Character = new CharacterPublicViewModel
                    {
                        Id = m.Character.Id,
                        Level = m.Character.Level,
                        Class = m.Character.Class,
                    },
                    Side = m.Side,
                }).ToArray();

            return new(mercenaries);
        }
    }
}
