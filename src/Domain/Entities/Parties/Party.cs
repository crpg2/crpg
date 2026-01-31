using Crpg.Domain.Common;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Parties;

/// <summary>
/// A party is a user information on the strategus map.
/// </summary>
public class Party : AuditableEntity
{
    /// <summary>
    /// Same as <see cref="User.Id"/> (one-to-one mapping).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Money of the user on Strategus. Different from <see cref="Users.User.Gold"/>.
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// Number of troops the user recruited.
    /// </summary>
    /// <remarks>Type is a float to be able to add a fraction of troop for each strategus tick.</remarks>
    public float Troops { get; set; }

    public List<PartyItem> Items { get; set; } = [];

    /// <summary>
    /// User position on the strategus map.
    /// </summary>
    public Point Position { get; set; } = Point.Empty;

    /// <summary>
    /// Status of the user. TODO: text about the differences between Party.Status and PartyOrder.Type.
    /// </summary>
    public PartyStatus Status { get; set; }

    public List<PartyOrder> Orders { get; set; } = [];

    /// <summary>
    /// The id of the party are exchanging if <see cref="Status"/> == <see cref="PartyStatus.AwaitingPartyOfferDecision"/>.
    /// </summary>
    public int? CurrentPartyId { get; set; }

    /// <summary>
    /// The id of the settlement the party is staying in if <see cref="Status"/> == <see cref="PartyStatus.IdleInSettlement"/>.
    /// The id of the settlement the party is recruiting in if <see cref="Status"/> == <see cref="PartyStatus.RecruitingInSettlement"/>.
    /// </summary>
    public int? CurrentSettlementId { get; set; }

    /// <summary>
    /// The id of the battle the party is moving to if <see cref="Status"/> == <see cref="PartyStatus.InBattle"/>.
    /// </summary>
    public int? CurrentBattleId { get; set; }

    /// <summary>See <see cref="CurrentPartyId"/>.</summary>
    public Party? CurrentParty { get; set; }

    /// <summary>See <see cref="CurrentSettlementId"/>.</summary>
    public Settlement? CurrentSettlement { get; set; }

    /// <summary>See <see cref="CurrentBattleId"/>.</summary>
    public Battle? CurrentBattle { get; set; }

    public User? User { get; set; }
    public List<Settlement> OwnedSettlements { get; set; } = []; // TODO: подумать
}
