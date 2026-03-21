using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleSideBriefingViewModel : IMapFrom<BattleSideBriefing>
{
    public BattleSide Side { get; set; }
    public string Note { get; set; } = string.Empty;
}
