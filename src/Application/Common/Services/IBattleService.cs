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
    int CalculateAttackerTotalTroops(Battle battle);
    int CalculateDefenderTotalTroops(Battle battle);
    BattleFighter? GetAttackerCommander(Battle battle);
    BattleFighter? GetDefenderCommander(Battle battle);
    BattleType GetBattleType(Battle battle);
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

    // TODO: FIXME: SPEC
    public BattleFighter? GetAttackerCommander(Battle battle)
    {
        return battle.Fighters.First(f => f.Side == BattleSide.Attacker && f.Commander);
    }

    // TODO: FIXME: SPEC
    public BattleFighter? GetDefenderCommander(Battle battle)
    {
        return battle.Fighters.First(f => f.Side == BattleSide.Defender && f.Commander);
    }

    // TODO: FIXME: SPEC
    public BattleType GetBattleType(Battle battle)
    {
        var defenderCommander = GetDefenderCommander(battle);
        return defenderCommander?.Settlement != null ? BattleType.Siege : BattleType.Battle;
    }

    // TODO: FIXME: SPEC
    public int CalculateAttackerTotalTroops(Battle battle)
    {
        return battle.Fighters
            .Where(f => f.Side == BattleSide.Attacker)
            .Sum(f => (int)Math.Floor(f.Party?.Troops ?? 0));
    }

    // TODO: FIXME: SPEC
    public int CalculateDefenderTotalTroops(Battle battle)
    {
        return battle.Fighters
            .Where(f => f.Side == BattleSide.Defender)
            .Sum(f =>
                (int)Math.Floor(f.Party?.Troops ?? 0) +
                (f.Settlement?.Troops ?? 0));
    }

    public BattleDetailedViewModel MapToBattleDetailedViewModel(IMapper mapper, Battle battle)
    {
        return new BattleDetailedViewModel
        {
            Id = battle.Id,
            Region = battle.Region,
            Position = battle.Position,
            Phase = battle.Phase,
            Type = GetBattleType(battle),
            Attacker = mapper.Map<BattleFighterViewModel>(GetAttackerCommander(battle)),
            AttackerTotalTroops = CalculateAttackerTotalTroops(battle),
            Defender = mapper.Map<BattleFighterViewModel>(GetDefenderCommander(battle)),
            DefenderTotalTroops = CalculateDefenderTotalTroops(battle),
            CreatedAt = battle.CreatedAt,
            ScheduledFor = battle.ScheduledFor,
        };
    }
}
