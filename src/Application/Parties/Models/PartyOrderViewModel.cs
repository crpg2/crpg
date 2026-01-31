using System.Text.Json.Serialization;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Parties;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

public record PartyOrderViewModel : IMapFrom<PartyOrder>
{
    public PartyOrderType Type { get; init; }
    public int OrderIndex { get; set; }
    public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
    [JsonRequired]
    public PartyVisibleViewModel? TargetedParty { get; init; }
    [JsonRequired]
    public SettlementPublicViewModel? TargetedSettlement { get; init; }
    [JsonRequired]
    public BattleViewModel? TargetedBattle { get; init; }
    public BattleJoinIntentViewModel[] BattleJoinIntents { get; init; } = [];
    public double Distance { get; set; }
}
