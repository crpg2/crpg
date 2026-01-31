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
    /// Awaiting acceptance or rejection of our offer to exchange (gold, items, army) with another a <see cref="Party"/>.
    /// </summary>
    AwaitingPartyOfferDecision,

    /// <summary>
    /// Stationary because involved in a <see cref="Battle"/>.
    /// </summary>
    InBattle,

    /// <summary>
    /// Awaiting response to applications to join <see cref="Battle"/> as a <see cref="BattleFighter"/>
    /// Cannot move (to move, must remove requests). Can be attacked by other <see cref="Party"/> (this is done to prevent abuse mechanics).
    /// </summary>
    AwaitingBattleJoinDecision,
}
