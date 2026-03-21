using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Battles.Models;

public record GameBattleViewModel : IMapFrom<Battle>
{
    public int Id { get; init; }
    public Region Region { get; init; }
    public IList<GameBattleFighterViewModel> Fighters { get; init; } = [];
    public IList<GameBattleParticipantViewModel> Participants { get; init; } = [];
    public IList<BattleSideBriefingViewModel> SideBriefings { get; init; } = [];
}
