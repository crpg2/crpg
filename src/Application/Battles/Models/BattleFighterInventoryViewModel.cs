using System.Text.Json.Serialization;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;

namespace Crpg.Application.Battles.Models;

public record BattleFighterInventoryViewModel
{
    public int FighterId { get; init; }
    [JsonRequired]
    public PartyPublicViewModel? Party { get; init; }
    [JsonRequired]
    public SettlementPublicViewModel? Settlement { get; init; }
    public IList<ItemStack> Items { get; init; } = Array.Empty<ItemStack>();
}
