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
    BattleDetailedViewModel MapBattleToDetailedViewModel(IMapper mapper, Battle battle, int userId);
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

    public BattleDetailedViewModel MapBattleToDetailedViewModel(IMapper mapper, Battle battle, int userId)
    {
        var userApplications = battle.MercenaryApplications
            .Where(a => a.Character!.UserId == userId)
            .ToList();

        BattleMercenaryApplicationStatus? attackerStatus = userApplications
            .Where(a => a.Side == BattleSide.Attacker)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => (BattleMercenaryApplicationStatus?)a.Status)
            .FirstOrDefault();

        BattleMercenaryApplicationStatus? defenderStatus = userApplications
            .Where(a => a.Side == BattleSide.Defender)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => (BattleMercenaryApplicationStatus?)a.Status)
            .FirstOrDefault();

        string? attackerBriefing = battle.SideBriefings
            .Where(b => b.Side == BattleSide.Attacker)
            .Select(b => b.Note)
            .FirstOrDefault();

        string? defenderBriefing = battle.SideBriefings
            .Where(b => b.Side == BattleSide.Defender)
            .Select(b => b.Note)
            .FirstOrDefault();

        return new BattleDetailedViewModel
        {
            Id = battle.Id,
            Region = battle.Region,
            Position = battle.Position,
            Phase = battle.Phase,
            Type = GetBattleType(battle),
            CreatedAt = battle.CreatedAt,
            ScheduledFor = battle.ScheduledFor,

            Attacker = mapper.Map<BattleFighterViewModel>(GetAttackerCommander(battle)),
            AttackerTotalTroops = CalculateAttackerTotalTroops(battle),
            AttackerApplicationStatus = attackerStatus,
            AttackerBriefing = attackerBriefing ?? string.Empty,

            Defender = mapper.Map<BattleFighterViewModel>(GetDefenderCommander(battle)),
            DefenderTotalTroops = CalculateDefenderTotalTroops(battle),
            DefenderApplicationStatus = defenderStatus,
            DefenderBriefing = defenderBriefing ?? string.Empty,
        };
    }
}
