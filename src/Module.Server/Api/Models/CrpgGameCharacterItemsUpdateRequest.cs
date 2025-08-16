using Crpg.Module.Api.Models.Items;

namespace Crpg.Module.Api.Models;

internal class CrpgGameCharacterItemsUpdateRequest
{
    public IList<CrpgEquippedItemId> Items { get; set; } = Array.Empty<CrpgEquippedItemId>();
}
