using System.Text.Json.Serialization;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record GameBattleFighterViewModel : IMapFrom<BattleFighter>
{
    public int Id { get; init; }
    public BattleSide Side { get; init; }
    public bool Commander { get; init; }
    public int ParticipantSlots { get; init; }
    [JsonRequired]
    public BattlePartyViewModel? Party { get; init; }
    [JsonRequired]
    public GameBattleSettlementViewModel? Settlement { get; init; }
}
