using Crpg.Domain.Entities.Servers;

namespace Crpg.Application.Common.Services;

internal interface IGameModeService
{
    GameMode GameModeByInstanceAlias(GameModeAlias alias);
}

internal class GameModeService : IGameModeService
{
    private readonly Dictionary<GameModeAlias, GameMode> gameModeByInstanceAlias = new()
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

    public GameMode GameModeByInstanceAlias(GameModeAlias alias)
    {
        return gameModeByInstanceAlias[alias];
    }
}
