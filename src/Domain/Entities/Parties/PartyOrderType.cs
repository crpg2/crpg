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
    /// Moving to transfer offer a <see cref="Party"/>.
    /// </summary>
    TransferOfferParty,

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
    /// <see cref="BattleFighterApplication"/> with status == <see cref="BattleFighterApplicationStatus.Intent"/> that defines the intended side to join.
    /// </summary>
    JoinBattle,
}
