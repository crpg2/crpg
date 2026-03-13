using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record UserItemPresetSlotViewModel : IMapFrom<UserItemPresetSlot>
{
    public ItemSlot Slot { get; init; }
    public string? ItemId { get; init; }
}
