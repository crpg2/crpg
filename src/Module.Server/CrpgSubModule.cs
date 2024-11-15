using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using Crpg.Module.Modes.Battle;
using Crpg.Module.Modes.Conquest;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Modes.Duel;
using Crpg.Module.Modes.Siege;
using Crpg.Module.Modes.TeamDeathmatch;
using Crpg.Module.Modes.TrainingGround;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.InputSystem;
using TaleWorlds.Engine.InputSystem;

using TaleWorlds.MountAndBlade.GameKeyCategory;



#if CRPG_SERVER
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.MountAndBlade.ListedServer;
using TaleWorlds.PlayerServices;
using WindowsFirewallHelper;
#else
using TaleWorlds.Engine.GauntletUI;
#endif

#if CRPG_EXPORT
using System.Runtime.CompilerServices;
using Crpg.Module.DataExport;
using TaleWorlds.Library;
using TaleWorlds.Localization;
#endif

namespace Crpg.Module;

internal class CrpgSubModule : MBSubModuleBase
{
#if CRPG_SERVER
    private static readonly Random Random = new();
    private static bool _mapPoolAdded;
    public static CrpgSubModule Instance = default!;
    public Dictionary<PlayerId, IAddress> WhitelistedIps = new();
    public int Port()
    {
        return TaleWorlds.MountAndBlade.Module.CurrentModule.StartupInfo.ServerPort;
    }

#endif
    static CrpgSubModule()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Debug.Print(args.ExceptionObject.ToString(), color: Debug.DebugColor.Red);
    }

    private CrpgConstants _constants = default!;

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();
        _constants = LoadCrpgConstants();
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode(_constants, MultiplayerGameType.Battle));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode(_constants, MultiplayerGameType.Skirmish));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode(_constants, MultiplayerGameType.Captain));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgConquestGameMode(_constants));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgSiegeGameMode(_constants));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgTeamDeathmatchGameMode(_constants));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgDuelGameMode(_constants));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgDtvGameMode(_constants));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgTrainingGroundGameMode(_constants));
#if CRPG_SERVER
        CrpgServerConfiguration.Init();
        CrpgFeatureFlags.Init();
#endif

#if CRPG_EXPORT
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ExportData",
            new TextObject("Export Data"), 4578, ExportData, () => (false, null)));
#endif

    // Uncomment to start watching UI changes.
#if CRPG_CLIENT
    // UIResourceManager.UIResourceDepot.StartWatchingChangesInDepot();
#endif
}

    protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
    {
        base.InitializeGameStarter(game, starterObject);
        InitializeGameModels(starterObject);
        CrpgSkills.Initialize(game);
        CrpgBannerEffects.Initialize(game);
        ManagedParameters.Instance.Initialize(ModuleHelper.GetXmlPath("Crpg", "managed_core_parameters"));
#if CRPG_CLIENT
        game.GameTextManager.LoadGameTexts();
#endif
    }

    protected override void OnApplicationTick(float delta)
    {
        base.OnApplicationTick(delta);
        // Uncomment to hot reload UI after changes.
#if CRPG_CLIENT
        // UIResourceManager.UIResourceDepot.CheckForChanges();
#endif
    }
#if CRPG_SERVER
    public override void OnGameInitializationFinished(Game game)
    {
        base.OnGameInitializationFinished(game);
        AddMaps();
        Debug.Print($"Now Adding Maps", color: Debug.DebugColor.Cyan);
        CrpgGamemodeManager.AddMaps();
    }

    private static void AddMaps()
    {
        if (_mapPoolAdded)
        {
            return;
        }

        string currentConfigFilePath = TaleWorlds.MountAndBlade.Module.CurrentModule.StartupInfo.CustomGameServerConfigFile;
        string mapconfigfilepath = Path.Combine(Directory.GetCurrentDirectory(), ModuleHelper.GetModuleFullPath("cRPG"), currentConfigFilePath.Replace(".txt", string.Empty) + "_maps.txt");
        if (File.Exists(mapconfigfilepath))
        {
            try
            {
                string[] maps = File.ReadAllLines(mapconfigfilepath);

                int startIndex = Random.Next(maps.Length); // Random start index between 0 and maps.Length - 1
                for (int i = 0; i < maps.Length; i++)
                {
                    int currentIndex = (startIndex + i) % maps.Length;
                    string map = maps[currentIndex];

                    if (map == string.Empty)
                    {
                        continue;
                    }

                    if (ServerSideIntermissionManager.Instance != null)
                    {
                        ServerSideIntermissionManager.Instance.AddMapToUsableMaps(map);
                        ServerSideIntermissionManager.Instance.AddMapToAutomatedBattlePool(map);
                        Debug.Print($"added {map} to map pool", color: Debug.DebugColor.Red);
                    }
                    else
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            Debug.Print($"There's no instance of ServerSideIntermissionManager", color: Debug.DebugColor.Red);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print($"could not read the map file {mapconfigfilepath}", color: Debug.DebugColor.Red);
                Debug.Print($"{e.Message}", color: Debug.DebugColor.Red);
            }
        }
        else
        {
            Debug.Print("No separate map file");
        }

        _mapPoolAdded = true;
        return;
    }

#endif
    private CrpgConstants LoadCrpgConstants()
    {
        string path = ModuleHelper.GetModuleFullPath("cRPG") + "ModuleData/constants.json";
        return JsonConvert.DeserializeObject<CrpgConstants>(File.ReadAllText(path))!;
    }

    private void InitializeGameModels(IGameStarter basicGameStarter)
    {
        basicGameStarter.AddModel(new CrpgAgentStatCalculateModel(_constants));
        basicGameStarter.AddModel(new CrpgItemValueModel(_constants));
        basicGameStarter.AddModel(new CrpgAgentApplyDamageModel(_constants));
        basicGameStarter.AddModel(new CrpgStrikeMagnitudeModel(_constants));
    }

#if CRPG_EXPORT
    private void ExportData()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Exporting data."));
        string gitRepoPath = FindGitRepositoryRootPath();
        Task.WhenAll(exporters.Select(e => e.Export(gitRepoPath))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private string FindGitRepositoryRootPath([CallerFilePath] string currentFilePath = default!)
    {
        var dir = Directory.GetParent(currentFilePath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find cRPG git repository");
    }
#endif
}
