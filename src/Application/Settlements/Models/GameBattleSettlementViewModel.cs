using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Application.Settlements.Models;

public record GameBattleSettlementViewModel : IMapFrom<Settlement>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public SettlementType Type { get; init; }
    public Region Region { get; init; }
    public string Scene { get; init; } = string.Empty;
    public int Troops { get; init; }
    public IList<ItemIdStackViewModel> Items { get; init; } = [];
}
