using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record UserItemPresetSlotInputModel
{
    public ItemSlot Slot { get; init; }
    public string? ItemId { get; init; }
}
