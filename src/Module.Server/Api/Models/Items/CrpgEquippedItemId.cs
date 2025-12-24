namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.EquippedItemIdViewModel.
/// </summary>
internal class CrpgEquippedItemId
{
    public CrpgItemSlot Slot { get; set; }
    public int? UserItemId { get; set; }
}
