namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// Status of a <see cref="BattleFighterApplication"/>.
/// </summary>
public enum BattleFighterApplicationStatus
{
    /// <summary>
    /// <see cref="BattleFighterApplication"/> represents an intention to join the battle,
    /// but the fighter has not yet arrived and cannot be accepted or declined.
    /// </summary>
    Intent,

    /// <summary>
    /// <see cref="BattleFighterApplication"/> is waiting for a response.
    /// </summary>
    Pending,

    /// <summary>
    /// <see cref="BattleFighterApplication"/> was declined.
    /// </summary>
    Declined,

    /// <summary>
    /// <see cref="BattleFighterApplication"/> was accepted.
    /// </summary>
    Accepted,
}
