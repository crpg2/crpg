using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.TeamDeathmatch;
public class CrpgTeamDeathmatchSpawnFrameBehavior : SpawnFrameBehaviorBase
{
    private List<GameEntity>[] _spawnPointsByTeam = null!;

    public override void Initialize()
    {
        base.Initialize();
        _spawnPointsByTeam = new List<GameEntity>[2];
        _spawnPointsByTeam[1] = SpawnPoints.Where(x => x.HasTag("attacker")).ToList();
        _spawnPointsByTeam[0] = SpawnPoints.Where(x => x.HasTag("defender")).ToList();

        if (_spawnPointsByTeam[0].Count < 1 | _spawnPointsByTeam[1].Count < 1) // If spawnpoints missing
        {
            _spawnPointsByTeam[0] = SpawnPoints.ToList();
            _spawnPointsByTeam[1] = SpawnPoints.ToList();
        }
    }

    public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
    {
        List<GameEntity> spawnPoints = SpawnPoints.ToList();

        if (isInitialSpawn)
        {
            spawnPoints = _spawnPointsByTeam[(int)team.Side];
        }

        return GetSpawnFrameFromSpawnPoints(spawnPoints, team, hasMount);
    }
}
