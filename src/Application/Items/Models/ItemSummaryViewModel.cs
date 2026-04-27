using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record ItemSummaryViewModel : IMapFrom<Item>
{
    public string Id { get; init; } = string.Empty;
    public string BaseId { get; init; } = string.Empty;
    public int Rank { get; init; }
    public string Name { get; init; } = string.Empty;
}
