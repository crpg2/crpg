using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Settlements;

namespace Crpg.Domain.Entities.Parties;

public enum PartyStatus
{
    /// <summary>
    /// Inactive.
    /// </summary>
    Idle,

    /// <summary>
    /// Inactive in a <see cref="Settlement"/>.
    /// </summary>
    IdleInSettlement,

    /// <summary>
    /// Recruiting troops in a <see cref="Settlement"/>.
    /// </summary>
    RecruitingInSettlement,

    /// <summary>
    /// Stationary because involved in a <see cref="Battle"/>.
    /// </summary>
    InBattle,

    /// <summary>
    /// Awaiting response to applications to join <see cref="Battle"/> as a <see cref="BattleFighter"/>
    /// Cannot move (to move, must remove requests). Can be attacked by other <see cref="Party"/> (this is done to prevent abuse mechanics).
    /// </summary>
    AwaitingBattleJoinDecision,

    /// <summary>
    /// Moving to an arbitrary location.
    /// </summary>
    // MovingToPoint,

    /// <summary>
    /// Following a <see cref="Party"/>.
    /// </summary>
    // FollowingParty,

    /// <summary>
    /// Following a <see cref="Settlement"/>.
    /// </summary>
    // MovingToSettlement,

    /// <summary>
    /// Moving to attack a <see cref="Party"/>.
    /// </summary>
    // MovingToAttackParty,

    /// <summary>
    /// Moving to attack a <see cref="Settlement"/>.
    /// </summary>
    // MovingToAttackSettlement,


    /// <summary>
    /// Moving towards a <see cref="Battle"/> location with an existing
    /// <see cref="BattleJoinIntent"/> that defines the intended side to join.
    /// </summary>
    // MovingToBattle,


}
