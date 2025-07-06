using System.Text.Json.Serialization;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record EquippedItemIdViewModel
{
    public ItemSlot Slot { get; set; }

    [JsonRequired]
    public int? UserItemId { get; set; }
}
