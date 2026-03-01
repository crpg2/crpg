#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#endif
using Crpg.Module.Common;
using Crpg.Module.Common.AmmoQuiverChange;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;
#if CRPG_CLIENT
using Crpg.Module.GUI;
using Crpg.Module.GUI.AmmoQuiverChange;
using Crpg.Module.GUI.Inventory;
using Crpg.Module.GUI.TrainingGround;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Modes.TrainingGround;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgTrainingGroundGameMode : MissionBasedMultiplayerGameMode
{
    public const string GameName = "cRPGTrainingGround";

    private static CrpgConstants _constants = null!; // Static so it's accessible from the views.

    public CrpgTrainingGroundGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgTrainingGround(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "cRPGTrainingGround", gameModeClient);

        return new[]
        {
            MultiplayerViewCreator.CreateMissionServerStatusUIHandler(),
            MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission),
            MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission),
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            crpgEscapeMenu,
            MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, true),
            MultiplayerViewCreator.CreateLobbyEquipmentUIHandler(),
            new CrpgTrainingGroundUiHandler(),
            new AmmoQuiverChangeUiHandler(), // Ammo Quiver change feature
            MultiplayerViewCreator.CreatePollProgressUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new MissionItemContourControllerView(),
            new MissionAgentContourControllerView(),
            MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler(), // Draw flags but also player names when pressing ALT.
            new CrpgAgentHud(experienceTable),
            new CrpgInventoryScreen(),
            // Draw flags but also player names when pressing ALT. (Native: CreateMissionFlagMarkerUIHandler)
            ViewCreatorManager.CreateMissionView<CrpgMarkerUiHandler>(isNetwork: false, null, gameModeClient),
        };
    }
#endif

    public override void StartMultiplayerGame(string scene)
    {
        var lobbyComponent = MissionLobbyComponent.CreateBehavior();

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        Game.Current.GetGameHandler<ChatCommandsComponent>()?.InitChatCommands(crpgClient);
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();
        CrpgRewardServer rewardServer = new(crpgClient, _constants, null, enableTeamHitCompensations: false, enableRating: false, enableLowPopulationUpkeep: true);
        CrpgTrainingGroundServer duelServer = new(rewardServer);
#endif
        CrpgTrainingGroundMissionMultiplayerClient duelClient = new();
        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            _ =>
                new MissionBehavior[]
                {
                    lobbyComponent,
#if CRPG_CLIENT
                    new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
                    new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                    new AmmoQuiverChangeBehaviorClient(), // Ammo Quiver change feature
                    new CrpgInventoryClient(),
#endif
                    duelClient,
                    new MultiplayerTimerComponent(), // round timer
                    new CrpgNotificationComponent(), // Inherits the MultiplayerGameNotificationsComponent component.
                    // new ConsoleMatchStartEndHandler(),
                    new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                    new MultiplayerTeamSelectComponent(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(), // set walkable boundaries
                    new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                    new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                    new AmmoQuiverChangeComponent(), // Ammo Quiver change feature
                    new MissionOptionsComponent(),
                    new CrpgScoreboardComponent(new CrpgTrainingGroundScoreboardData()), // score board
                    new MultiplayerPreloadHelper(),
#if CRPG_SERVER
                    duelServer,
                    rewardServer,
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new CrpgTrainingGroundSpawnFrameBehavior(), new CrpgTrainingGroundSpawningBehavior(_constants, duelServer)),
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new MissionAgentPanicHandler(),
                    new AgentHumanAILogic(), // bot intelligence
                    new EquipmentControllerLeaveLogic(),
                    new CrpgUserManagerServer(crpgClient, _constants),
                    new CrpgInventoryBehavior(),
                    new CrpgActivityLogsBehavior(null, chatBox, crpgClient),
                    new ServerMetricsBehavior(),
                    new NotAllPlayersReadyComponent(),
                    new PopulationBasedEntityVisibilityBehavior(lobbyComponent),
#else
                    new MultiplayerAchievementComponent(),
                    MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
                    new MissionRecentPlayersComponent(),
                    new CrpgRewardClient(),
#endif
                });
    }
}
