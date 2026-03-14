using System.Text.Json.Serialization;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record UserItemPresetSlotInputModel
{
    public ItemSlot Slot { get; init; }
    [JsonRequired]
    public string? ItemId { get; init; }
}
