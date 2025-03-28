using AutoMapper;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Battles.Models;

public record BattleDetailedViewModel : IMapFrom<Battle>
{
    public int Id { get; init; }
    public Region Region { get; set; }
    public Point Position { get; set; } = default!;
    public BattlePhase Phase { get; set; }
    public BattleFighterViewModel Attacker { get; init; } = default!;
    public int AttackerTotalTroops { get; init; }
    public BattleFighterViewModel? Defender { get; init; }
    public int DefenderTotalTroops { get; init; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Battle, BattleDetailedViewModel>()
            .ForMember(dest => dest.Attacker,
                opt => opt.MapFrom(src =>
                    src.Fighters.First(f => f.Side == BattleSide.Attacker && f.Commander)))
            .ForMember(dest => dest.Defender,
                opt => opt.MapFrom(src =>
                    src.Fighters.First(f => f.Side == BattleSide.Defender && f.Commander)))
            .ForMember(dest => dest.AttackerTotalTroops,
                opt => opt.MapFrom(src =>
                    src.Fighters
                        .Where(f => f.Side == BattleSide.Attacker)
                        .Sum(f => (int)Math.Floor(f.Party!.Troops))))
            .ForMember(dest => dest.DefenderTotalTroops,
                opt => opt.MapFrom(src =>
                    src.Fighters
                        .Where(f => f.Side == BattleSide.Defender)
                        .Sum(f =>
                            Math.Floor(f.Party != null ? f.Party.Troops : 0) +
                            (f.Settlement != null ? f.Settlement.Troops : 0))));
    }
}
