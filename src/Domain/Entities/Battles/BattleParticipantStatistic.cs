using Crpg.Domain.Entities.Battles;

/// <summary>
/// Post-battle <see cref="Battle"/> statistics of the <see cref="BattleParticipant"/>.
/// </summary>
public class BattleParticipantStatistic
{
    public int Id { get; set; }

    public int BattleId { get; set; }
    public int ParticipantId { get; set; }

    /// <summary>
    /// Actual participation (whether <see cref="BattleParticipant"/> visited the game server).
    /// </summary>
    public bool Participated { get; set; }

    public int Kills { get; set; }
    public int Assists { get; set; }
    public int Deaths { get; set; }

    public BattleParticipant? Participant { get; set; }
    public Battle? Battle { get; set; }
}
