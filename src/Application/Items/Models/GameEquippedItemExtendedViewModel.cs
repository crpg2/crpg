using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record GameEquippedItemExtendedViewModel : IMapFrom<EquippedItem>
{
    public ItemSlot Slot { get; init; }
    public GameUserItemExtendedViewModel UserItem { get; init; } = default!;
}
