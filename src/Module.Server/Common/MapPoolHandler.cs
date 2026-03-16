using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ListedServer;
using Debug = TaleWorlds.Library.Debug;

namespace Crpg.Module.Common;

/// <summary>
/// Manages the map pool: reads maps from a config file, shuffles them, registers them with the server,
/// and advances maps sequentially after each mission.
///
/// Lives as a <see cref="GameHandler"/> (game lifetime) so maps are registered before the first mission.
/// Injects a lightweight <see cref="MissionLogic"/> to advance maps on mission end.
///
/// <b>Why SetAutomatedMapPool?</b>
/// After each mission, <c>ServerSideIntermissionManager.TickAutomatedBattles</c> randomly picks a map from
/// <c>AutomatedMapPool</c> and writes it into <c>CurrentMapOptions</c>, bypassing <c>SetValue</c>. We cannot prevent
/// this because <c>ServerSideIntermissionManager</c> is in a closed-source DLL. Instead, we replace the pool contents
/// with a single map (the one we want next) so the "random" pick is always correct.
/// </summary>
internal class MapPoolHandler : GameHandler
{
    private static int _nextMapId = -1;
    private static string[] _maps = [];

    private string? _forcedNextMap;

    public void ForceNextMap(string map)
    {
        if (!_maps.Contains(map))
        {
            return;
        }

        _forcedNextMap = map;
    }

    public void OnBeforeMissionBehaviorInitialize(Mission mission)
    {
        mission.AddMissionBehavior(new MapPoolMissionLogic(this));
    }

    public override void OnBeforeSave()
    {
    }

    public override void OnAfterSave()
    {
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        AddMaps();

        MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;
        MultiplayerIntermissionVotingManager.Instance.IsDisableMapVoteOverride = true;

        if (_maps.Length > 0)
        {
            AdvanceMap();
        }
    }

    private void AdvanceMap()
    {
        _nextMapId = (_nextMapId + 1) % _maps.Length;
        string nextMap = _forcedNextMap ?? _maps[_nextMapId];

        // Replaces the AutomatedMapPool contents with a single map so that TickAutomatedBattles can only "randomly"
        // pick our desired map. See class summary for full explanation.
        var pool = (IList<string>)ReflectionHelper.GetField(
            ListedServerCommandManager.ServerSideIntermissionManager.AutomatedMapPool,
            "list")!;
        pool.Clear();
        pool.Add(nextMap);

        MultiplayerOptions.OptionType.Map.SetValue(nextMap, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
        MultiplayerOptions.OptionType.Map.SetValue(nextMap, MultiplayerOptions.MultiplayerOptionsAccessMode.NextMapOptions);
        _forcedNextMap = null;

        Debug.Print($"Next map: {nextMap}", color: Debug.DebugColor.Green);
    }

    private static void AddMaps()
    {
        string configFilePath = TaleWorlds.MountAndBlade.Module.CurrentModule.StartupInfo.CustomGameServerConfigFile;
        string mapsFilePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            ModuleHelper.GetModuleFullPath("cRPG"),
            configFilePath.Replace(".txt", "") + "_maps.txt");

        if (!File.Exists(mapsFilePath))
        {
            Debug.Print($"Map configuration file not found: {mapsFilePath}", color: Debug.DebugColor.Red);
            return;
        }

        try
        {
            _maps = File.ReadAllLines(mapsFilePath)
                .Where(map => !string.IsNullOrWhiteSpace(map))
                .ToArray();

            _maps.Shuffle();

            Debug.Print("Shuffled map order:", color: Debug.DebugColor.Green);
            foreach (string map in _maps)
            {
                Debug.Print($"  - {map}", color: Debug.DebugColor.Green);
                ServerSideIntermissionManager.Instance.AddMapToUsableMaps(map);
                ServerSideIntermissionManager.Instance.AddMapToAutomatedBattlePool(map);
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error reading the map file {mapsFilePath}: {e.Message}", color: Debug.DebugColor.Red);
        }
    }

    private class MapPoolMissionLogic(MapPoolHandler mapPool) : MissionLogic
    {
        protected override void OnEndMission()
        {
            mapPool.AdvanceMap();
        }
    }
}
