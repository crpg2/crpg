using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Parties;

public class PartyOrder
{
    public int Id { get; set; }
    public int PartyId { get; set; }

    public PartyOrderType Type { get; set; }

    public int OrderIndex { get; set; }

    /// <summary>
    /// Sequence of points the user is moving to if <see cref="Type"/> == <see cref="PartyOrderType.MoveToPoint"/>.
    /// </summary>
    public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;

    /// <summary>
    /// The id of the party to follow if <see cref="Type"/> == <see cref="PartyOrderType.FollowParty"/>.
    /// The id of the party to attack if <see cref="Type"/> == <see cref="PartyOrderType.AttackParty"/>.
    /// </summary>
    public int? TargetedPartyId { get; set; }

    /// <summary>
    /// The id of the settlement the party is moving to if <see cref="Type"/> == <see cref="PartyOrderType.MoveToSettlement"/>.
    /// The id of the settlement to attack if <see cref="Type"/> == <see cref="PartyOrderType.AttackSettlement"/>.
    /// </summary>
    public int? TargetedSettlementId { get; set; }

    /// <summary>
    /// The id of the battle the party is moving to if <see cref="Type"/> == <see cref="PartyOrderType.JoinBattle"/>.
    /// </summary>
    public int? TargetedBattleId { get; set; }

    /// <summary>See <see cref="TargetedPartyId"/>.</summary>
    public Party? TargetedParty { get; set; }

    /// <summary>See <see cref="TargetedSettlementId"/>.</summary>
    public Settlement? TargetedSettlement { get; set; }

    /// <summary>See <see cref="TargetedBattleId"/>.</summary>
    public Battle? TargetedBattle { get; set; }

    public Party? Party { get; set; }
}
