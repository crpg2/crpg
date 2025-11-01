using Crpg.Module.Common;
using Crpg.Module.Common.AmmoQuiverChange;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;


#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#else
using Crpg.Module.GUI;
using Crpg.Module.GUI.AmmoQuiverChange;
using Crpg.Module.GUI.HudExtension;
using Crpg.Module.GUI.Inventory;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Modes.Siege;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgSiegeGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPGSiege";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    public CrpgSiegeGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgSiege(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();

        return new[]
        {
            MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
            MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission), // Pick/drop items.
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "Siege", gameModeClient),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            MultiplayerViewCreator.CreatePollProgressUIHandler(),
            new MissionItemContourControllerView(), // Draw contour of item on the ground when pressing ALT.
            new MissionAgentContourControllerView(),
            MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
            new CrpgHudExtensionHandler(),
            new AmmoQuiverChangeUiHandler(),
            new CrpgMainGuiMissionView(), // Character/Equip gui
            MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new SpectatorCameraView(),
            new CrpgAgentHud(experienceTable),
            // Draw flags but also player names when pressing ALT. (Native: CreateMissionFlagMarkerUIHandler)
            ViewCreatorManager.CreateMissionView<CrpgMarkerUiHandler>(isNetwork: false, null, gameModeClient),
        };
    }
#endif

    public override void StartMultiplayerGame(string scene)
    {
        CrpgNotificationComponent notificationsComponent = new();
        CrpgSiegeClient siegeClient = new();
        CrpgScoreboardComponent scoreboardComponent = new(new CrpgBattleScoreboardData());
        var lobbyComponent = MissionLobbyComponent.CreateBehavior();

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        Game.Current.GetGameHandler<ChatCommandsComponent>()?.InitChatCommands(crpgClient);
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent,
            () => (new SiegeSpawnFrameBehavior(), new CrpgSiegeSpawningBehavior(_constants)));
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: false, enableRating: false);
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
#endif

        MissionState.OpenNew(GameName,
            new MissionInitializerRecord(scene) { SceneUpgradeLevel = 3, SceneLevels = string.Empty },
            _ => new MissionBehavior[]
            {
                lobbyComponent,
#if CRPG_CLIENT
                new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
                new MultiplayerMissionAgentVisualSpawnComponent(),
                new AmmoQuiverChangeBehaviorClient(),
                new CrpgClanArmoryClient(), // Clan armory Sync with API/Client for GUI
                new CrpgCharacterLoadoutBehaviorClient(_constants), // Inventory & Equipment Sync for GUI  
#endif
                warmupComponent,
                siegeClient,
                new MultiplayerTimerComponent(),
                new SpawnComponent(new SiegeSpawnFrameBehavior(), new CrpgSiegeSpawningBehavior(_constants)),
                new MultiplayerTeamSelectComponent(),
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new MultiplayerAdminComponent(),
                notificationsComponent,
                new AmmoQuiverChangeComponent(),
                new MissionOptionsComponent(),
                scoreboardComponent,
                new MissionAgentPanicHandler(),
                new AgentHumanAILogic(),
                new EquipmentControllerLeaveLogic(),
                new MultiplayerPreloadHelper(),
                new WelcomeMessageBehavior(warmupComponent),
                new MissionLobbyEquipmentNetworkComponent(),

#if CRPG_SERVER
                new CrpgSiegeServer(siegeClient, scoreboardComponent, rewardServer),
                new CrpgUserManagerServer(crpgClient, _constants),
                new KickInactiveBehavior(inactiveTimeLimit: 90, warmupComponent),
                new MapPoolComponent(),
                new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                new ServerMetricsBehavior(),
                new NotAllPlayersReadyComponent(),
                new DrowningBehavior(),
                new PopulationBasedEntityVisibilityBehavior(lobbyComponent),
                new CrpgClanArmoryServer(crpgClient), // Clan armory Sync with API/Client for GUI
                new CrpgCharacterLoadoutBehaviorServer(crpgClient), // Inventory & Equipment Sync for GUI
#else
                new MultiplayerAchievementComponent(),
                MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
                new MissionRecentPlayersComponent(),
                new CrpgRewardClient(),
#endif
            });
    }
}
