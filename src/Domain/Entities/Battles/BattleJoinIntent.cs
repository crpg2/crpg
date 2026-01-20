using Crpg.Domain.Entities.Parties;

namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// Intent of a <see cref="Party"/> to move to a <see cref="Battle"/> location
/// without yet participating in the battle.
/// </summary>
/// Upon reaching the battle location, the intent is consumed and results in the creation
/// of a <see cref="BattleFighterApplication"/>, using the chosen <see cref="BattleSide"/>.
/// </remarks>
public class BattleJoinIntent
{
    public int Id { get; set; }

    /// <summary>
    /// The target <see cref="Battle"/> the party is moving towards.
    /// </summary>
    public int BattleId { get; set; }

    /// <summary>
    /// The <see cref="Party"/> that intends to join the battle.
    /// </summary>
    public int PartyId { get; set; }

    /// <summary>
    /// The side the party intends to apply for upon arrival.
    /// </summary>
    /// <remarks>
    /// This value represents an intention only and does not imply any participation
    /// in the battle until a <see cref="BattleFighterApplication"/> is created.
    /// </remarks>
    public BattleSide Side { get; set; }

    public Battle? Battle { get; set; }
    public Party? Party { get; set; }
}
