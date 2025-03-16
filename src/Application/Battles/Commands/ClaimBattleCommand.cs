using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record ClaimBattleCommand : IMediatorRequest<BattleViewModel>
{
    public int BattleId { get; init; }
    public string Instance { get; init; } = string.Empty;
    internal class Handler : IMediatorRequestHandler<ClaimBattleCommand, BattleViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ClaimBattleCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<BattleViewModel>> Handle(ClaimBattleCommand req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);

            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Instance != null)
            {
                return new(CommonErrors.BattleAlreadyClaimed(req.BattleId, battle.Instance));
            }

            battle.Instance = req.Instance;

            Logger.LogInformation("Battle '{0}' claimed by instance '{1}'",
                battle.Id, req.Instance);

            await _db.SaveChangesAsync(cancellationToken);
            return new(_mapper.Map<BattleViewModel>(battle));
        }
    }
}
