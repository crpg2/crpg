namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.GameClanArmoryItemViewModel.
/// </summary>
internal class CrpgClanArmoryItem
{
    public CrpgUserItemExtended? UserItem { get; set; }
    public CrpgClanArmoryBorrowedItem? BorrowedItem { get; set; }
    public DateTime UpdatedAt { get; set; }
}
