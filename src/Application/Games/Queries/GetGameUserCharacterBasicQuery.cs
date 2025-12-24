using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Queries;

public record GetGameUserCharacterBasicQuery : IMediatorRequest<GameCharacterViewModel>
{
    public int UserId { get; init; }
    public int CharacterId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetGameUserCharacterBasicQuery, GameCharacterViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<GameCharacterViewModel>> Handle(GetGameUserCharacterBasicQuery req, CancellationToken cancellationToken)
        {
            // Load character
            var character = await _db.Characters
                .Include(c => c.Characteristics) // attributes + skills
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId,
                    cancellationToken);

            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            // Map to ViewModel
            var vm = _mapper.Map<GameCharacterViewModel>(character);
            return new(vm);
        }
    }
}
