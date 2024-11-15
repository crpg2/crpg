using Crpg.Module.Common;
using Crpg.Module.Common.Commander;
using Crpg.Module.Common.TeamSelect;
using Crpg.Module.Modes.Siege;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Multiplayer;

#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#else
using Crpg.Module.GUI;
using Crpg.Module.GUI.Commander;
using Crpg.Module.GUI.Conquest;
using Crpg.Module.GUI.Spectator;
using Crpg.Module.GUI.Warmup;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Modes.Conquest;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgConquestGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPGConquest";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    public CrpgConquestGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgConquest(Mission mission)
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
            ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "cRPGConquest", gameModeClient),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            MultiplayerViewCreator.CreatePollProgressUIHandler(),
            new CrpgRespawnTimerUiHandler(),
            new CommanderPollingProgressUiHandler(),
            new MissionItemContourControllerView(), // Draw contour of item on the ground when pressing ALT.
            new MissionAgentContourControllerView(),
            MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
            new SpectatorHudUiHandler(),
            new WarmupHudUiHandler(),
            new ConquestHudUiHandler(),
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
        CrpgScoreboardComponent scoreboardComponent = new(new CrpgBattleScoreboardData());
        var lobbyComponent = MissionLobbyComponent.CreateBehavior();

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();
        CrpgSiegeSpawningBehavior spawnBehavior = new(_constants);
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent,
            () => (new SiegeSpawnFrameBehavior(), new CrpgSiegeSpawningBehavior(_constants)));
        CrpgTeamSelectServerComponent teamSelectComponent = new(warmupComponent, null, MultiplayerGameType.Siege);
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: false, enableRating: false);
        CrpgConquestServer conquestServer = new(scoreboardComponent, rewardServer);
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
        CrpgTeamSelectClientComponent teamSelectComponent = new();
#endif

        MissionState.OpenNew(GameName,
            new MissionInitializerRecord(scene) { SceneUpgradeLevel = 3, SceneLevels = string.Empty },
            _ => new MissionBehavior[]
            {
                lobbyComponent,
#if CRPG_CLIENT
                new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
                // Shit that need to stay because BL code is extremely coupled to the visual spawning.
                new MultiplayerMissionAgentVisualSpawnComponent(),
                new CrpgCommanderBehaviorClient(),
                new CrpgRespawnTimerClient(),
#endif
                warmupComponent,
                new CrpgConquestClient(),
                new MultiplayerTimerComponent(),
                teamSelectComponent,
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new CrpgCommanderPollComponent(),
                new MultiplayerAdminComponent(),
                notificationsComponent,
                new MissionOptionsComponent(),
                scoreboardComponent,
                new MissionAgentPanicHandler(),
                new AgentHumanAILogic(),
                new EquipmentControllerLeaveLogic(),
                new MultiplayerPreloadHelper(),
                new WelcomeMessageBehavior(warmupComponent),
                new MissionLobbyEquipmentNetworkComponent(),

#if CRPG_SERVER
                conquestServer,
                rewardServer,
                new SpawnComponent(new SiegeSpawnFrameBehavior(), spawnBehavior),
                new CrpgUserManagerServer(crpgClient, _constants),
                new KickInactiveBehavior(inactiveTimeLimit: 90, warmupComponent),
                new MapPoolComponent(),
                new ChatCommandsComponent(chatBox, crpgClient),
                new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                new ServerMetricsBehavior(),
                new NotAllPlayersReadyComponent(),
                new DrowningBehavior(),
                new PopulationBasedEntityVisibilityBehavior(lobbyComponent),
                new CrpgCommanderBehaviorServer(),
                new CrpgRespawnTimerServer(conquestServer, spawnBehavior),
#else
                new MultiplayerAchievementComponent(),
                MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
                new MissionRecentPlayersComponent(),
                new CrpgRewardClient(),
#endif
            });
    }
}
