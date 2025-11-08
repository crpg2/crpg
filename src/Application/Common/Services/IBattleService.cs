using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Common.Services;

internal interface IBattleService
{
    Task<Result<BattleFighter>> GetBattleFighter(ICrpgDbContext db, int userId, int battleId, CancellationToken cancellationToken);
    Task<Result<BattleMercenary>> GetBattleMercenary(ICrpgDbContext db, int userId, int battleId, CancellationToken cancellationToken);
    Task<Result<BattleMercenaryApplication>> GetBattleMercenaryApplication(ICrpgDbContext db, int userId, int battleId, CancellationToken cancellationToken);
    BattleDetailedViewModel MapToBattleDetailedViewModel(IMapper mapper, Battle battle);
}

internal class BattleService : IBattleService
{
    public async Task<Result<BattleFighter>> GetBattleFighter(ICrpgDbContext db, int userId, int battleId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return new(CommonErrors.UserNotFound(userId));
        }

        var battle = await db.Battles
            .FirstOrDefaultAsync(b => b.Id == battleId, cancellationToken);

        if (battle == null)
        {
            return new(CommonErrors.BattleNotFound(battleId));
        }

        var partyFighter = await db.BattleFighters
            .FirstOrDefaultAsync(u => u.Party!.User!.Id == userId, cancellationToken);
        var settlementFighter = await db.BattleFighters
            .FirstOrDefaultAsync(u => u.Settlement!.Owner!.User!.Id == userId, cancellationToken);
        if (partyFighter == null && settlementFighter == null)
        {
            return new(CommonErrors.FighterNotFound(userId, battleId));
        }

        return partyFighter != null ? new(partyFighter) : new(settlementFighter);
    }

    public async Task<Result<BattleMercenary>> GetBattleMercenary(ICrpgDbContext db, int userId, int battleId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return new(CommonErrors.UserNotFound(userId));
        }

        var battle = await db.Battles
            .FirstOrDefaultAsync(b => b.Id == battleId, cancellationToken);

        if (battle == null)
        {
            return new(CommonErrors.BattleNotFound(battleId));
        }

        var mercenary = await db.BattleMercenaries
            .FirstOrDefaultAsync(u => u.Character!.UserId == userId, cancellationToken);
        if (mercenary == null)
        {
            return new(CommonErrors.MercenaryNotFound(userId));
        }

        return new(mercenary);
    }

    public async Task<Result<BattleMercenaryApplication>> GetBattleMercenaryApplication(ICrpgDbContext db, int userId, int battleId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return new(CommonErrors.UserNotFound(userId));
        }

        var battle = await db.Battles
            .FirstOrDefaultAsync(b => b.Id == battleId, cancellationToken);

        if (battle == null)
        {
            return new(CommonErrors.BattleNotFound(battleId));
        }

        var mercenaryApplication = await db.BattleMercenaryApplications
            .FirstOrDefaultAsync(u => u.Character!.UserId == userId, cancellationToken);
        if (mercenaryApplication == null)
        {
            return new(CommonErrors.ApplicationNotFound(userId));
        }

        return new(mercenaryApplication);
    }

    public BattleDetailedViewModel MapToBattleDetailedViewModel(IMapper mapper, Battle battle)
    {
        var attackerCommander = battle.Fighters.First(f => f.Side == BattleSide.Attacker && f.Commander);
        var defenderCommander = battle.Fighters.First(f => f.Side == BattleSide.Defender && f.Commander);
        var battleType = defenderCommander.Settlement != null ? BattleType.Siege : BattleType.Battle;

        return new BattleDetailedViewModel
        {
            Id = battle.Id,
            Region = battle.Region,
            Position = battle.Position,
            Phase = battle.Phase,
            Type = battleType,
            Attacker = mapper.Map<BattleFighterViewModel>(attackerCommander),
            AttackerTotalTroops = battle.Fighters
                .Where(f => f.Side == BattleSide.Attacker)
                .Sum(f => (int)Math.Floor(f.Party!.Troops)),
            Defender = mapper.Map<BattleFighterViewModel>(defenderCommander),
            DefenderTotalTroops = battle.Fighters
                .Where(f => f.Side == BattleSide.Defender)
                .Sum(f => (int)Math.Floor(f.Party?.Troops ?? 0) + (f.Settlement?.Troops ?? 0)),
            CreatedAt = battle.CreatedAt,
            ScheduledFor = battle.ScheduledFor,
        };
    }
}
