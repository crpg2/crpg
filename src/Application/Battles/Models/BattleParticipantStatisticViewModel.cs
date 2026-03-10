using Crpg.Application.Common.Mappings;

namespace Crpg.Application.Battles.Models;

public record BattleParticipantStatisticViewModel : IMapFrom<BattleParticipantStatistic>
{
    public bool Participated { get; set; }

    public int Kills { get; set; }
    public int Assists { get; set; }
    public int Deaths { get; set; }
}
