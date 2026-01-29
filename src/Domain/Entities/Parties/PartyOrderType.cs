using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Parties;

public enum PartyOrderType
{
    /// <summary>
    /// Moving to an arbitrary location.
    /// </summary>
    MoveToPoint,

    /// <summary>
    /// Following a <see cref="Party"/>.
    /// </summary>
    FollowParty,

    /// <summary>
    /// Moving to attack a <see cref="Party"/>.
    /// </summary>
    AttackParty,

    /// <summary>
    /// Following a <see cref="Settlement"/>.
    /// </summary>
    MoveToSettlement,

    /// <summary>
    /// Moving to attack a <see cref="Settlement"/>.
    /// </summary>
    AttackSettlement,

    /// <summary>
    /// Moving towards a <see cref="Battle"/> location with an existing
    /// <see cref="BattleJoinIntent"/> that defines the intended side to join.
    /// </summary>
    JoinBattle,
}
