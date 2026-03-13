using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Servers;

namespace Crpg.Application.Games.Models;

public record GameUserUpdate
{
    public int UserId { get; init; }
    public int CharacterId { get; init; }
    public GameUserReward Reward { get; init; } = new();
    public CharacterStatisticsViewModel Statistics { get; init; } = new();
    public IList<GameUserDamagedItem> BrokenItems { get; init; } = Array.Empty<GameUserDamagedItem>();
    public GameMode GameMode { get; init; }
    public string Instance { get; init; } = string.Empty;
}
