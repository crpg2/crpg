using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

/// <summary>
/// Returns the live battle for the calling game server instance, or claims a new scheduled one. If a battle is already
/// live for this instance, it is returned (resume). Otherwise, the next scheduled battle past its time for the region
/// is claimed, associated to the instance, switched to <see cref="BattlePhase.Live"/> and returned.
/// </summary>
public record StartScheduledBattleCommand : IMediatorRequest<GameBattleViewModel>
{
    public Region Region { get; init; }
    public string Instance { get; init; } = string.Empty;

    internal class Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime)
        : IMediatorRequestHandler<StartScheduledBattleCommand, GameBattleViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<StartScheduledBattleCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IDateTime _dateTime = dateTime;

        public async ValueTask<Result<GameBattleViewModel>> Handle(StartScheduledBattleCommand req, CancellationToken cancellationToken)
        {
            var battleQuery = _db.Battles.AsSplitQuery()
                .Include(b => b.Fighters).ThenInclude(f => f.Party).ThenInclude(p => p!.User).ThenInclude(u => u!.Characters)
                .Include(b => b.Fighters).ThenInclude(f => f.Party).ThenInclude(p => p!.User).ThenInclude(u => u!.ClanMembership)
                .Include(b => b.Fighters).ThenInclude(f => f.Party).ThenInclude(p => p!.Items)
                .Include(b => b.Fighters).ThenInclude(f => f.Settlement).ThenInclude(s => s!.Items)
                .Include(b => b.Participants).ThenInclude(p => p.Character!.Statistics)
                .Include(b => b.Participants).ThenInclude(p => p.Character!.User!)
                .Include(b => b.SideBriefings);

            // Check if this instance already has a live battle.
            var battle = await battleQuery
                .FirstOrDefaultAsync(b => b.Phase == BattlePhase.Live && b.GameServer == req.Instance, cancellationToken);

            if (battle != null)
            {
                Logger.LogInformation("Battle '{0}' resumed by instance '{1}'", battle.Id, req.Instance);
                await LoadActiveRestrictions(battle, cancellationToken);
                return new(_mapper.Map<GameBattleViewModel>(battle));
            }

            // Claim the next scheduled battle for the region.
            battle = await battleQuery
                .FirstOrDefaultAsync(b => b.Phase == BattlePhase.Scheduled
                                          && b.ScheduledFor != null
                                          && b.ScheduledFor <= _dateTime.UtcNow
                                          && b.Region == req.Region, cancellationToken);

            if (battle == null)
            {
                return new((GameBattleViewModel)null!);
            }

            battle.Phase = BattlePhase.Live;
            battle.GameServer = req.Instance;
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Battle '{0}' started by instance '{1}'", battle.Id, req.Instance);
            await LoadActiveRestrictions(battle, cancellationToken);
            return new(_mapper.Map<GameBattleViewModel>(battle));
        }

        /// <summary>
        /// Load only the latest active restriction per type for each participant's user,
        /// matching the behavior of <see cref="Games.Commands.GetGameUserCommand"/>.
        /// </summary>
        private async Task LoadActiveRestrictions(Battle battle, CancellationToken cancellationToken)
        {
            foreach (var participant in battle.Participants)
            {
                await _db.Entry(participant.Character!.User!)
                    .Collection(u => u.Restrictions)
                    .Query()
                    .GroupBy(r => r.Type)
                    .Select(g => g.OrderByDescending(r => r.CreatedAt).FirstOrDefault()!)
                    .LoadAsync(cancellationToken);
            }
        }
    }
}
