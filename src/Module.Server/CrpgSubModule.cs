using System.Diagnostics;
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
using Debug = TaleWorlds.Library.Debug;

#if CRPG_SERVER
using Crpg.Module.Common.ChatCommands;
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
using Crpg.Module.HarmonyPatches;
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
        LoadMainMenu();
        BannerlordPatches.Apply();
        /*
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Scale",
            new TextObject("Scale"), 4578, Scale, () => (false, null)));*/
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

        // Add the chat command handler here so network messages are being processed first.
        if (game.GetGameHandler<ChatCommandsComponent>() == null)
        {
            game.AddGameHandler<ChatCommandsComponent>();
        }
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

                // Remove empty or invalid map entries
                maps = maps.Where(map => !string.IsNullOrWhiteSpace(map)).ToArray();

                // Shuffle maps using Fisher-Yates Shuffle
                Debug.Print("Shuffling maps for random order.", color: Debug.DebugColor.Cyan);
                for (int i = maps.Length - 1; i > 0; i--)
                {
                    int j = Random.Next(i + 1);
                    (maps[i], maps[j]) = (maps[j], maps[i]);
                }

                // Debug: Print shuffled map list
                Debug.Print("Shuffled map order:", color: Debug.DebugColor.Green);
                foreach (string map in maps)
                {
                    Debug.Print($"- {map}", color: Debug.DebugColor.Green);
                }

                // Add each map to the server's usable maps and automated battle pool
                foreach (string map in maps)
                {
                    if (ServerSideIntermissionManager.Instance != null)
                    {
                        ServerSideIntermissionManager.Instance.AddMapToUsableMaps(map);
                        ServerSideIntermissionManager.Instance.AddMapToAutomatedBattlePool(map);
                        Debug.Print($"Added {map} to usable maps and automated battle pool.", color: Debug.DebugColor.Red);
                    }
                    else
                    {
                        Debug.Print("ServerSideIntermissionManager instance is null. Unable to add maps.", color: Debug.DebugColor.Red);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print($"Error reading the map file {mapconfigfilepath}: {e.Message}", color: Debug.DebugColor.Red);
            }
        }
        else
        {
            Debug.Print($"Map configuration file not found: {mapconfigfilepath}", color: Debug.DebugColor.Red);
        }

        _mapPoolAdded = true;
        Debug.Print("Finished adding maps to the rotation.", color: Debug.DebugColor.Cyan);
    }

#endif
    private CrpgConstants LoadCrpgConstants()
    {
        string path = ModuleHelper.GetModuleFullPath("cRPG_Exporter") + "ModuleData/constants.json";
        return JsonConvert.DeserializeObject<CrpgConstants>(File.ReadAllText(path))!;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Debug.Print(args.ExceptionObject.ToString(), color: Debug.DebugColor.Red);
    }

    private void InitializeGameModels(IGameStarter basicGameStarter)
    {
        basicGameStarter.AddModel(new CrpgAgentStatCalculateModel(_constants));
        basicGameStarter.AddModel(new CrpgItemValueModel(_constants));
        basicGameStarter.AddModel(new CrpgAgentApplyDamageModel(_constants));
        basicGameStarter.AddModel(new CrpgStrikeMagnitudeModel(_constants));
    }

#if CRPG_EXPORT
    private void LoadMainMenu()
    {
        List<InitialStateOption> mainMenuOptions = new()
        {
            new InitialStateOption("ExportData",
            new TextObject("Export Data"), 4578, ExportData, () => (false, null)),
            new InitialStateOption("ComputeAutoStats",
            new TextObject("Compute AutoGenerated Stats"), 4578, ComputeAutoStats, () => (false, null)),
            new InitialStateOption("ExportImages",
            new TextObject("Export Thumbnails"), 4578, ExportImages, () => (false, null)),
            new InitialStateOption("SelectWhatToRefund",
            new TextObject("Change Refund"), 4578, ChangeRefund, () => (false, null)),
            new InitialStateOption("SelectWhatToRefund",
            new TextObject("Refund Selected"), 4578, Refund, () => (false, null)),
            new InitialStateOption("Scale",
            new TextObject("Scale"), 4578, Scale, () => (false, null)),
            new InitialStateOption("ScaleWeapon",
            new TextObject("ScaleWeapon"), 4578, ScaleWeapon, () => (false, null)),

        };
        foreach (var opt in mainMenuOptions)
        {
            TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(opt);
        }
    }

    private void RefundFirearm()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Firearms."));
        Task.WhenAll(exporters.Select(e => e.RefundFirearm("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Firearms were refunded"));
        });
    }

    private void RefundCrossbow()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Crossbows."));
        Task.WhenAll(exporters.Select(e => e.RefundCrossbow("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Crossbows were refunded"));
        });
    }

    private void RefundArmor()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Armors."));
        Task.WhenAll(exporters.Select(e => e.RefundArmor("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Armors were refunded"));
        });
    }

    private void RefundWeapons()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Armors."));
        Task.WhenAll(exporters.Select(e => e.RefundWeapons("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Weapons were refunded"));
        });
    }

    private void RefundThrowing()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Throwing."));
        Task.WhenAll(exporters.Select(e => e.RefundThrowing("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Throwing were refunded"));
        });
    }

    private void RefundCav()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Cav."));
        Task.WhenAll(exporters.Select(e => e.RefundCav("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("cav has been refunded"));
        });
    }

    private void RefundBow()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Bows."));
        Task.WhenAll(exporters.Select(e => e.RefundBow("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Bow were Refunded"));
        });
    }

    private void RefundShield()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Refunding Bows."));
        Task.WhenAll(exporters.Select(e => e.RefundShield("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Shield were Refunded"));
        });
    }

    private void Scale()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Editing Class."));
        Task.WhenAll(exporters.Select(e => e.Scale("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private void ScaleWeapon()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Editing Class."));
        Task.WhenAll(exporters.Select(e => e.ScaleWeapon("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private void ComputeAutoStats()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Computing Auto Generated Stats."));
        Task.WhenAll(exporters.Select(e => e.ComputeAutoStats("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private void ExportData()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Exporting data."));
        Task.WhenAll(exporters.Select(e => e.Export("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private void ExportImages()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Exporting Images."));
        Task.WhenAll(exporters.Select(e => e.ImageExport("lol"))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private void Refund()
    {
        switch (_toRefund)
        {
            case 0:
                RefundCrossbow();
                break;
            case 1:
                RefundThrowing();
                break;
            case 2:
                RefundBow();
                break;
            case 3:
                RefundShield();
                break;
            case 4:
                RefundArmor();
                break;
            case 5:
                RefundCav();
                break;
            case 6:
                RefundWeapons();
                break;
            case 7:
                RefundFirearm();
                break;
            default:
                throw new ArgumentException("Invalid argument for 'toRefund'");
        }
    }

    private void ChangeRefund()
    {
        _toRefund = (_toRefund + 1) % 8;
        string message = _toRefund switch
        {
            0 => "Refund Crossbows",
            1 => "Refund Throwings",
            2 => "Refund Bows",
            3 => "Refund Shields",
            4 => "Refund Armors",
            5 => "Refund Cav",
            6 => "Refund Weapons",
            7 => "Refund firearms"
        }

        + " has been selected";
        InformationManager.DisplayMessage(new InformationMessage(message));
    }

    private int _toRefund = 0;

#endif
}
