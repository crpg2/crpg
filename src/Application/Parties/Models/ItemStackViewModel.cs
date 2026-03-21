using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// View model for an <see cref="ItemStack"/>.
/// </summary>
public record ItemStackViewModel : IMapFrom<ItemStack>
{
    public ItemViewModel Item { get; init; } = default!;
    public int Count { get; init; }
}
