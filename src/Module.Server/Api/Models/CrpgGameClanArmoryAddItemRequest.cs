using Crpg.Module.Api.Models.Items;
using Messages.FromClient.ToLobbyServer;

namespace Crpg.Module.Api.Models;

internal class CrpgGameClanArmoryAddItemRequest
{
    public int UserItemId { get; set; }
    public int UserId { get; set; }
    public int ClanId { get; set; }
}
