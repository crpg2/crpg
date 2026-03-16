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
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using Debug = TaleWorlds.Library.Debug;
#if CRPG_CLIENT
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.HarmonyPatches;
#endif

#if CRPG_EDITOR
using Crpg.Module.HarmonyPatches;
#endif

#if CRPG_SERVER
using Crpg.Module.Common.ChatCommands;
#else
#endif

namespace Crpg.Module;

internal class CrpgSubModule : MBSubModuleBase
{
    static CrpgSubModule()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Debug.Print(args.ExceptionObject.ToString(), color: Debug.DebugColor.Red);
    }

    private CrpgConstants _constants = null!;

#if CRPG_SERVER
    public override void OnGameInitializationFinished(Game game)
    {
        base.OnGameInitializationFinished(game);

        if (game.GetGameHandler<MapPoolHandler>() == null)
        {
            game.AddGameHandler<MapPoolHandler>();
        }

        // Add the chat command handler here so network messages are being processed first.
        if (game.GetGameHandler<ChatCommandsComponent>() == null)
        {
            game.AddGameHandler<ChatCommandsComponent>();
        }
    }

    public override void OnBeforeMissionBehaviorInitialize(Mission mission)
    {
        base.OnBeforeMissionBehaviorInitialize(mission);
        Game.Current.GetGameHandler<MapPoolHandler>()?.OnBeforeMissionBehaviorInitialize(mission);
    }
#endif

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
#elif CRPG_CLIENT
        KeyBinder.Initialize();
        BannerlordPatches.Apply();
        KeyBinder.RegisterContexts();

        // Uncomment to start watching UI changes.
        // UIResourceManager.ResourceDepot.StartWatchingChangesInDepot();
#elif CRPG_EDITOR
        BannerlordPatches.Apply();
#endif
    }

    protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
    {
        base.InitializeGameStarter(game, starterObject);
        InitializeGameModels(starterObject);
        CrpgSkills.Initialize(game);
        CrpgBannerEffects.Initialize(game);
        ManagedParameters.Instance.Initialize(ModuleHelper.GetXmlPath("cRPG", "managed_core_parameters"));
#if CRPG_CLIENT
        game.GameTextManager.LoadGameTexts();
#endif
    }

    protected override void OnApplicationTick(float delta)
    {
        base.OnApplicationTick(delta);
        // Uncomment to hot reload UI after changes.
#if CRPG_CLIENT
        // UIResourceManager.ResourceDepot.CheckForChanges();
#endif
    }

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
}
