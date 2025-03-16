using Crpg.Domain.Entities.Servers;

namespace Crpg.GameServerManager.Common.Services;

internal static class GameModeService
{
    private static readonly Dictionary<GameModeAlias, GameMode> GameModeByInstanceAlias = new()
    {
        { GameModeAlias.A, GameMode.CRPGBattle },
        { GameModeAlias.B, GameMode.CRPGConquest },
        { GameModeAlias.C, GameMode.CRPGDuel },
        { GameModeAlias.E, GameMode.CRPGDTV },
        { GameModeAlias.D, GameMode.CRPGSkirmish },
        { GameModeAlias.F, GameMode.CRPGTeamDeathmatch },
        { GameModeAlias.Z, GameMode.CRPGUnknownGameMode },
        { GameModeAlias.G, GameMode.CRPGCaptain },
    };

    private static readonly Dictionary<GameMode, GameModeAlias> AliasByGameMode =
       GameModeByInstanceAlias.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static GameMode GetGameModeByInstanceAlias(GameModeAlias alias)
    {
        return GameModeByInstanceAlias[alias];
    }

    public static GameModeAlias GetInstanceAliasByGameMode(GameMode mode)
    {
        return AliasByGameMode.TryGetValue(mode, out var alias) ? alias : GameModeAlias.Z;
    }
}
