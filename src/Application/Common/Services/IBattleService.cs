using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Terrains.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;

namespace Crpg.Application.Common.Services;

internal interface IBattleService
{
    BattleDetailedViewModel MapBattleToDetailedViewModel(IMapper mapper, Battle battle, int userId, Settlement? nearestSettlement, Terrain terrain);
    int CalculateTotalParticipantSlots(Battle battle, BattleSide side);
}

internal class BattleService : IBattleService
{
    /// <summary>
    /// Calculates the total participant slots for a battle side.
    /// See <see cref="BattleParticipantUniformDistributionModel.DistributeParticipants"/>.
    /// </summary>
    public int CalculateTotalParticipantSlots(Battle battle, BattleSide side)
    {
        var fighters = battle.Fighters.Where(f => f.Side == side);
        return fighters.Sum(f => f.ParticipantSlots) + fighters.Count();
    }

    public BattleDetailedViewModel MapBattleToDetailedViewModel(
        IMapper mapper,
        Battle battle,
        int userId,
        Settlement? nearestSettlement,
        Terrain terrain)
    {
        var userApplications = battle.MercenaryApplications
            .Where(a => a.Character!.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        var userParticipant = battle.Participants
            .FirstOrDefault(m => m.Character!.UserId == userId);

        return new BattleDetailedViewModel
        {
            Id = battle.Id,
            Region = battle.Region,
            Position = battle.Position,
            NearestSettlement = nearestSettlement != null ? mapper.Map<SettlementPublicViewModel>(nearestSettlement) : null,
            Terrain = mapper.Map<TerrainViewModel>(terrain),
            Phase = battle.Phase,
            Type = GetBattleType(battle),
            CreatedAt = battle.CreatedAt,
            ScheduledFor = battle.ScheduledFor,
            Attacker = BuildSideViewModel(
                mapper,
                battle,
                BattleSide.Attacker,
                userApplications,
                userParticipant),
            Defender = BuildSideViewModel(
                mapper,
                battle,
                BattleSide.Defender,
                userApplications,
                userParticipant),
        };
    }

    private static BattleFighter? GetCommander(Battle battle, BattleSide side)
    {
        return battle.Fighters.FirstOrDefault(f => f.Side == side && f.Commander);
    }

    private static Settlement? GetDefenderSettlement(Battle battle)
    {
        return battle.Fighters.FirstOrDefault(f => f.Side == BattleSide.Defender && f.Settlement != null)?.Settlement;
    }

    private static BattleType GetBattleType(Battle battle)
    {
        var settlementFighter = battle.Fighters.FirstOrDefault(f => f.Settlement != null);
        return settlementFighter != null ? BattleType.Siege : BattleType.Battle;
    }

    private static bool ShouldShowMercenaryApplication(BattleMercenaryApplication application, BattleParticipant? participant)
    {
        return participant != null || application.Status != BattleMercenaryApplicationStatus.Accepted;
    }

    private static BattleMercenaryApplication? GetUserMercenaryApplication(IEnumerable<BattleMercenaryApplication> applications, BattleSide side)
    {
        return applications.FirstOrDefault(a => a.Side == side);
    }

    private static int CalculateTotalTroops(Battle battle, BattleSide side)
    {
        if (side == BattleSide.Attacker)
        {
            return battle.Fighters
                .Where(f => f.Side == BattleSide.Attacker)
                .Sum(f => (int)Math.Floor(f.Party?.Troops ?? 0));
        }

        return battle.Fighters
            .Where(f => f.Side == BattleSide.Defender)
            .Sum(f =>
                (int)Math.Floor(f.Party?.Troops ?? 0) +
                (f.Settlement?.Troops ?? 0));
    }

    private BattleSideDetailedViewModel BuildSideViewModel(
    IMapper mapper,
    Battle battle,
    BattleSide side,
    IEnumerable<BattleMercenaryApplication> userApplications,
    BattleParticipant? userParticipant)
    {
        var mercenaryApplication = GetUserMercenaryApplication(userApplications, side);
        var briefing = battle.SideBriefings.FirstOrDefault(b => b.Side == side);
        var defenderSettlement = side == BattleSide.Defender ? GetDefenderSettlement(battle) : null;

        return new BattleSideDetailedViewModel
        {
            Commander = mapper.Map<BattleFighterViewModel>(GetCommander(battle, side)),
            Settlement = defenderSettlement != null ? mapper.Map<SettlementPublicViewModel>(defenderSettlement) : null,
            TotalTroops = CalculateTotalTroops(battle, side),
            TotalParticipantSlots = CalculateTotalParticipantSlots(battle, side),
            MercenaryApplication = mercenaryApplication != null && ShouldShowMercenaryApplication(mercenaryApplication, userParticipant)
                ? mapper.Map<BattleMercenaryApplicationViewModel>(mercenaryApplication)
                : null,
            Briefing = briefing != null
                ? mapper.Map<BattleSideBriefingViewModel>(briefing)
                : new BattleSideBriefingViewModel(),
        };
    }
}
