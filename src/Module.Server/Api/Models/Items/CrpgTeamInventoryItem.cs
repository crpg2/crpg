namespace Crpg.Module.Api.Models.Items;

internal class CrpgTeamInventoryItem
{
    public string Id { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public bool Restricted { get; set; } = false;
}
