namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.GameUserItemViewModel.
/// </summary>
internal class CrpgUserItem
{
    public int Id { get; set; }
    public string ItemId { get; set; } = null!;
    public int Rank { get; set; }
    public bool IsBroken { get; set; }
}
