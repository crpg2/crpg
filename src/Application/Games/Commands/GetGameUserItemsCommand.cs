using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands;

/// <summary>
/// Get all items owned by a user. Used by the game server to populate the in-game inventory.
/// </summary>
public record GetGameUserItemsCommand : IMediatorRequest<IList<GameUserItemViewModel>>
{
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = default!;

    public class Validator : AbstractValidator<GetGameUserItemsCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Platform).IsInEnum();
            RuleFor(c => c.PlatformUserId).NotEmpty();
        }
    }

    internal class Handler : IMediatorRequestHandler<GetGameUserItemsCommand, IList<GameUserItemViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<IList<GameUserItemViewModel>>> Handle(GetGameUserItemsCommand req,
            CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId,
                    cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.Platform, req.PlatformUserId));
            }

            var items = await _db.UserItems
                .Where(ui => ui.UserId == user.Id)
                .Include(ui => ui.Item)
                .ProjectTo<GameUserItemViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new Result<IList<GameUserItemViewModel>>(items);
        }
    }
}
