namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.GameEquippedItemExtendedViewModel.
/// </summary>
internal class CrpgEquippedItemExtended
{
    public CrpgItemSlot Slot { get; set; }
    public CrpgUserItemExtended UserItem { get; set; } = default!;
}
