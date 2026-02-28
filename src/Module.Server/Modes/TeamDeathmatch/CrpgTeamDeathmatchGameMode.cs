#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#else

using Crpg.Module.GUI;
using Crpg.Module.GUI.AmmoQuiverChange;
using Crpg.Module.GUI.Commander;
using Crpg.Module.GUI.EndOfRound;
using Crpg.Module.GUI.HudExtension;
using Crpg.Module.GUI.Scoreboard;
using Crpg.Module.GUI.Warmup;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif
using Crpg.Module.Common;
using Crpg.Module.Common.AmmoQuiverChange;
using Crpg.Module.Common.Commander;
using Crpg.Module.Common.FriendlyFireReport;
using Crpg.Module.Common.HotConstants;
using Crpg.Module.Common.TeamSelect;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace Crpg.Module.Modes.TeamDeathmatch;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgTeamDeathmatchGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPGTeamDeathmatch";

    private static CrpgConstants _constants = null!; // Static so it's accessible from the views.

    public CrpgTeamDeathmatchGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgTeamDeathmatch(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "TeamDeathmatch", gameModeClient);

        return new[]
        {
            MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission), // Pick/drop items.
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            crpgEscapeMenu,
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            new CrpgMissionScoreboardUIHandler(false),
            new CrpgEndOfBattleUIHandler(),
            new CrpgRespawnTimerUiHandler(),
            MultiplayerViewCreator.CreatePollProgressUIHandler(),
            new CommanderPollingProgressUiHandler(),
            new MissionItemContourControllerView(), // Draw contour of item on the ground when pressing ALT.
            new MissionAgentContourControllerView(),
            MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
            new CrpgHudExtensionHandler(),
            new AmmoQuiverChangeUiHandler(),
            MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            // new SpectatorHudUiHandler(),
            new WarmupHudUiHandler(),
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
        var lobbyComponent = MissionLobbyComponent.CreateBehavior();
        CrpgScoreboardComponent scoreboardComponent = new(new CrpgBattleScoreboardData());

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        Game.Current.GetGameHandler<ChatCommandsComponent>()?.InitChatCommands(crpgClient);
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();
        CrpgTeamDeathmatchSpawningBehavior spawnBehavior = new(_constants);
        // MultiplayerRoundController roundController = new(); // starts/stops round, ends match
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, lobbyComponent,
            () => (new CrpgTeamDeathmatchSpawnFrameBehavior(), new CrpgTeamDeathmatchSpawningBehavior(_constants)));
        CrpgTeamSelectServerComponent teamSelectComponent = new(warmupComponent, null, MultiplayerGameType.TeamDeathmatch);
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: false, enableRating: true);
        CrpgTeamDeathmatchServer teamDeathmatchServer = new(scoreboardComponent, rewardServer);

#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, lobbyComponent, null);
        CrpgTeamSelectClientComponent teamSelectComponent = new();
#endif

        MissionState.OpenNew(GameName,
            new MissionInitializerRecord(scene) { SceneUpgradeLevel = 3, SceneLevels = string.Empty },
            _ => new MissionBehavior[]
            {
                lobbyComponent,
#if CRPG_CLIENT
                new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
                new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                new CrpgCommanderBehaviorClient(),
                new AmmoQuiverChangeBehaviorClient(),
                new CrpgRespawnTimerClient(),
                new FriendlyFireReportClientBehavior(), // Ctrl+M to report friendly fire
#endif
                new CrpgTeamDeathmatchClient(),
                new MultiplayerTimerComponent(),
                new MissionLobbyEquipmentNetworkComponent(),
                teamSelectComponent,
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new AgentVictoryLogic(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new CrpgCommanderPollComponent(),
                new AmmoQuiverChangeComponent(),
                new MissionOptionsComponent(),
                scoreboardComponent,
                new MissionAgentPanicHandler(),
                new EquipmentControllerLeaveLogic(),
                new MultiplayerPreloadHelper(),
                warmupComponent,
                notificationsComponent,
                new WelcomeMessageBehavior(warmupComponent),

#if CRPG_SERVER
                teamDeathmatchServer,
                rewardServer,
                new SpawnComponent(new CrpgTeamDeathmatchSpawnFrameBehavior(), spawnBehavior),
                new AgentHumanAILogic(),
                new MultiplayerAdminComponent(),
                new CrpgUserManagerServer(crpgClient, _constants),
                new KickInactiveBehavior(inactiveTimeLimit: 30, warmupComponent, teamSelectComponent),
                new MapPoolComponent(),
                new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                new ServerMetricsBehavior(),
                new NotAllPlayersReadyComponent(),
                new DrowningBehavior(),
                new PopulationBasedEntityVisibilityBehavior(lobbyComponent),
                new BreakableWeaponsBehaviorServer(),
                new CrpgCustomTeamBannersAndNamesServer(null),
                new CrpgCommanderBehaviorServer(),
                new CrpgRespawnTimerServer(teamDeathmatchServer, spawnBehavior),
                new FriendlyFireReportServerBehavior(), // Ctrl+M to report friendly fire
#else
                new MultiplayerAchievementComponent(),
                MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
                new MissionRecentPlayersComponent(),
                new CrpgRewardClient(),
                new HotConstantsClient(),
                new BreakableWeaponsBehaviorClient(),
                new CrpgCustomTeamBannersAndNamesClient(),
#endif
            });
    }
}
