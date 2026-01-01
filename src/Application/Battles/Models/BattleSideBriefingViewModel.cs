using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleSideBriefingViewModel : IMapFrom<BattleSideBriefing>
{
    public int Id { get; init; }
    public string Note { get; set; } = string.Empty;
}
