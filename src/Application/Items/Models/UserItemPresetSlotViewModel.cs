using System.Text.Json.Serialization;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record UserItemPresetSlotViewModel : IMapFrom<UserItemPresetSlot>
{
    public ItemSlot Slot { get; init; }

    [JsonRequired]
    public ItemViewModel? Item { get; init; } = null;
}
