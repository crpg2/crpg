using Crpg.Module.Common;
using Crpg.Module.Common.AmmoQuiverChange;
using Crpg.Module.Common.Commander;
using Crpg.Module.Common.FriendlyFireReport;
using Crpg.Module.Common.HotConstants;
using Crpg.Module.Common.TeamSelect;
using Crpg.Module.Modes.Skirmish;
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
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using Crpg.Module.GUI;
using Crpg.Module.GUI.AmmoQuiverChange;
using Crpg.Module.GUI.Commander;
using Crpg.Module.GUI.EndOfRound;
using Crpg.Module.GUI.HudExtension;
using Crpg.Module.GUI.Scoreboard;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

#endif

namespace Crpg.Module.Modes.Battle;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
{
    private const string BattleGameName = "cRPGBattle";
    private const string SkirmishGameName = "cRPGSkirmish";
    private const string CaptainGameName = "cRPGCaptain";
    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.
    private MultiplayerGameType _gameType;
    public CrpgBattleGameMode(CrpgConstants constants, MultiplayerGameType gameType)
        : base(gameType switch
        {
            MultiplayerGameType.Battle => BattleGameName,
            MultiplayerGameType.Skirmish => SkirmishGameName,
            MultiplayerGameType.Captain => CaptainGameName,
            _ => throw new ArgumentException(message: "Invalid game type", paramName: nameof(gameType)),
        })
    {
        _gameType = gameType;
        _constants = constants;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(BattleGameName)]
    public static MissionView[] OpenCrpgBattle(Mission mission) => OpenCrpgBattleOrSkirmish(mission);
    [ViewMethod(SkirmishGameName)]
    public static MissionView[] OpenCrpgSkirmish(Mission mission) => OpenCrpgBattleOrSkirmish(mission);
    [ViewMethod(CaptainGameName)]
    public static MissionView[] OpenCrpgCaptain(Mission mission) => OpenCrpgBattleOrSkirmish(mission);

    [ViewMethod("")] // All static instances in ViewCreatorModule classes are expected to have a ViewMethod attribute.
    private static MissionView[] OpenCrpgBattleOrSkirmish(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "Battle", gameModeClient);

        return new[]
        {
            MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission), // Pick/drop items.
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            new MissionGauntletMultiplayerOrderUIHandler(),
            new OrderTroopPlacer(null),
            crpgEscapeMenu,
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            new CrpgMissionScoreboardUIHandler(false),
            new CrpgEndOfRoundUiHandler(),
            new CrpgEndOfBattleUIHandler(),
            MultiplayerViewCreator.CreatePollProgressUIHandler(),
            new CommanderPollingProgressUiHandler(),
            new MissionItemContourControllerView(), // Draw contour of item on the ground when pressing ALT.
            new MissionAgentContourControllerView(),
            MultiplayerViewCreator.CreateMissionKillNotificationUIHandler(),
            new CrpgHudExtensionHandler(),
            new AmmoQuiverChangeUiHandler(),
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "<En attente>")]
    public override void StartMultiplayerGame(string scene)
    {
        // Inherits the MultiplayerGameNotificationsComponent component.
        // used to send notifications (e.g. flag captured, round won) to peer
        CrpgNotificationComponent notificationsComponent = new();
        var lobbyComponent = MissionLobbyComponent.CreateBehavior();

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        Game.Current.GetGameHandler<ChatCommandsComponent>()?.InitChatCommands(crpgClient);
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();

        MultiplayerRoundController roundController = new(); // starts/stops round, ends match
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, () =>
            (new FlagDominationSpawnFrameBehavior(),
                    _gameType switch
                    {
                        MultiplayerGameType.Battle => new CrpgBattleSpawningBehavior(_constants, roundController, _gameType),
                        MultiplayerGameType.Skirmish => new CrpgSkirmishSpawningBehavior(_constants, roundController),
                        MultiplayerGameType.Captain => new CrpgBattleSpawningBehavior(_constants, roundController, _gameType),
                        _ => throw new ArgumentException(message: "Invalid game type", paramName: nameof(_gameType)),
                    }));
        CrpgTeamSelectServerComponent teamSelectComponent = new(warmupComponent, roundController, _gameType);
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: true, enableRating: true);
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
        CrpgTeamSelectClientComponent teamSelectComponent = new();
#endif
        CrpgBattleClient battleClient = new(_gameType);

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
                    new CrpgCommanderBehaviorClient(),
                    new AmmoQuiverChangeBehaviorClient(),
                    new FriendlyFireReportClientBehavior(), // Ctrl+M to report friendly fire
#endif
                    battleClient,
                    new MultiplayerTimerComponent(), // round timer
                    new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                    teamSelectComponent,
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(), // set walkable boundaries
                    new AgentVictoryLogic(), // AI cheering when winning round
                    new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                    new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                    new CrpgCommanderPollComponent(),
                    new AmmoQuiverChangeComponent(),
                    new MissionOptionsComponent(),
                    new CrpgScoreboardComponent(_gameType switch
                            {
                                    MultiplayerGameType.Battle => new CrpgBattleScoreboardData(),
                                    MultiplayerGameType.Skirmish => new CrpgSkirmishScoreboardData(),
                                    MultiplayerGameType.Captain => new CrpgBattleScoreboardData(),
                                    _ => throw new ArgumentException(message: "Invalid game type", paramName: nameof(_gameType)),
                            }),
                    new MissionAgentPanicHandler(),
                    new EquipmentControllerLeaveLogic(),
                    new MultiplayerPreloadHelper(),
                    warmupComponent,
                    notificationsComponent,
                    new WelcomeMessageBehavior(warmupComponent),

#if CRPG_SERVER
                    roundController,
                    new CrpgBattleServer(battleClient, _gameType, rewardServer),
                    rewardServer,
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new BattleSpawnFrameBehavior(),
                    _gameType switch
                            {
                                    MultiplayerGameType.Battle => new CrpgBattleSpawningBehavior(_constants, roundController, _gameType),
                                    MultiplayerGameType.Skirmish => new CrpgSkirmishSpawningBehavior(_constants, roundController),
                                    MultiplayerGameType.Captain => new CrpgBattleSpawningBehavior(_constants, roundController, _gameType),
                                    _ => throw new ArgumentException(message: "Invalid game type", paramName: nameof(_gameType)),
                            }),

                    new AgentHumanAILogic(), // bot intelligence
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new CrpgUserManagerServer(crpgClient, _constants),
                    new KickInactiveBehavior(inactiveTimeLimit: 30, warmupComponent, teamSelectComponent),
                    new MapPoolComponent(),
                    new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                    new ServerMetricsBehavior(),
                    new NotAllPlayersReadyComponent(),
                    new DrowningBehavior(),
                    new PopulationBasedEntityVisibilityBehavior(lobbyComponent),
                    new BreakableWeaponsBehaviorServer(),
                    new CrpgCustomTeamBannersAndNamesServer(roundController),
                    new CrpgCommanderBehaviorServer(),
                    new FriendlyFireReportServerBehavior(), // Ctrl+M to report friendly fire
#else
                    new MultiplayerRoundComponent(),
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
