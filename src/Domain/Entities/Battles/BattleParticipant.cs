using Crpg.Domain.Entities.Characters;

namespace Crpg.Domain.Entities.Battles;

/// <summary>
/// A specific participant in the <see cref="Battle"/>, a real user and character.
/// Can be a battlefighter - <see cref="BattleParticipantType.Party"/>, mercenary - <see cref="BattleParticipantType.Mercenary"/>, or clan member - <see cref="BattleParticipantType.ClanMember"/>. After the battle ends, records the result (K/D/A, .etc).
/// </summary>
public class BattleParticipant
{
    public int Id { get; set; }

    /// <summary>The id of the battle the user will fight in.</summary>
    public int BattleId { get; set; }
    public BattleSide Side { get; set; }

    public BattleParticipantType Type { get; set; }

    /// <summary>The id of the character the user will fight with.</summary>
    public int CharacterId { get; set; }

    /// <summary>The id of the fighter the user will fight for.</summary>
    public int CaptainFighterId { get; set; }

    /// <summary>The id of the application that got the user accepted in the battle as mercenary. Relevant for <see cref="BattleParticipantType.Mercenary"/>.</summary>
    public int? MercenaryApplicationId { get; set; }

    /// <summary>See <see cref="CharacterId"/>.</summary>
    public Character? Character { get; set; }

    /// <summary>See <see cref="BattleId"/>.</summary>
    public Battle? Battle { get; set; }

    /// <summary>See <see cref="CaptainFighterId"/>.</summary>
    public BattleFighter? CaptainFighter { get; set; }

    /// <summary>See <see cref="ApplicationId"/>.</summary>
    public BattleMercenaryApplication? MercenaryApplication { get; set; }

    public BattleParticipantStatistic? Statistic { get; set; }
}
