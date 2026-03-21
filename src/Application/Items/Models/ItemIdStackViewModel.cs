using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record ItemIdStackViewModel : IMapFrom<ItemStack>
{
    public string ItemId { get; init; } = string.Empty;
    public int Count { get; init; }
}
