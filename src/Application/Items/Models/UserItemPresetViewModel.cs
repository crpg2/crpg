using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record UserItemPresetViewModel : IMapFrom<UserItemPreset>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public IList<UserItemPresetSlotViewModel> Slots { get; init; } = Array.Empty<UserItemPresetSlotViewModel>();
}
