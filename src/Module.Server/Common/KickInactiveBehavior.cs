using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.TeamSelect;
using Crpg.Module.Modes.Battle;
using Crpg.Module.Notifications;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Common;

/// <summary>
/// Basically a copy of <see cref="MissionPeer.TickInactivityStatus"/>.
/// </summary>
internal class KickInactiveBehavior : MissionBehavior
{
    private readonly MissionTime _inactiveTimeLimit;
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly Dictionary<PlayerId, ActivityStatus> _lastActiveStatuses;
    private readonly CrpgTeamSelectServerComponent? _crpgTeamSelectServerComponent;
    private bool _isBattleServer;
    private Timer? _checkTimer;
    private Timer? _battleTimer;

    public KickInactiveBehavior(
        float inactiveTimeLimit,
        MultiplayerWarmupComponent warmupComponent,
        CrpgTeamSelectServerComponent? teamSelectComponent = null)
    {
        _inactiveTimeLimit = MissionTime.Seconds(inactiveTimeLimit);
        _warmupComponent = warmupComponent;
        _lastActiveStatuses = new Dictionary<PlayerId, ActivityStatus>();
        _crpgTeamSelectServerComponent = teamSelectComponent;
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnBehaviorInitialize()
    {
        _isBattleServer = Mission.Current.GetMissionBehavior<CrpgBattleServer>()?.GetMissionType() == MultiplayerGameType.Battle;
    }

    public override void OnClearScene()
    {
        if (_isBattleServer)
        {
            _battleTimer = new Timer(Mission.CurrentTime, 65f, false);
        }
    }

    public override void OnMissionTick(float dt)
    {
        if (_warmupComponent.IsInWarmup)
        {
            return;
        }

        _checkTimer ??= new Timer(Mission.CurrentTime, 1f);
        if (!_checkTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        if (_isBattleServer)
        {
            _battleTimer ??= new Timer(Mission.CurrentTime, 65f, false);
            if (_battleTimer.Check(Mission.CurrentTime))
            {
                return;
            }
        }

        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var playerId = networkPeer.VirtualPlayer.Id;
            var agent = networkPeer.ControlledAgent;
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (agent == null || !agent.IsActive() || crpgPeer.User!.Role == CrpgUserRole.Admin)
            {
                _lastActiveStatuses.Remove(playerId);
                continue;
            }

            if (!_lastActiveStatuses.TryGetValue(playerId, out var lastActiveStatus))
            {
                _lastActiveStatuses[playerId] = new ActivityStatus
                {
                    LastActive = MissionTime.Now,
                    MovementFlags = agent.MovementFlags,
                    MovementInputVector = agent.MovementInputVector,
                    LookDirection = agent.LookDirection,
                    Warned = false,
                };
                continue;
            }

            ActivityStatus newActiveStatus = new()
            {
                LastActive = MissionTime.Now,
                MovementFlags = agent.MovementFlags,
                MovementInputVector = agent.MovementInputVector,
                LookDirection = agent.LookDirection,
                IsUsingGameObject = agent.IsUsingGameObject,
                Warned = false,
            };

            if (newActiveStatus.IsUsingGameObject
                || lastActiveStatus.MovementFlags != newActiveStatus.MovementFlags
                || lastActiveStatus.MovementInputVector.DistanceSquared(newActiveStatus.MovementInputVector) > 1E-05f
                || lastActiveStatus.LookDirection.DistanceSquared(newActiveStatus.LookDirection) > 1E-05f)
            {
                _lastActiveStatuses[playerId] = newActiveStatus;
                continue;
            }

            if (MissionTime.Now - lastActiveStatus.LastActive > _inactiveTimeLimit)
            {
                Debug.Print($"Kick inactive user {crpgPeer.User!.Character.Name} ({crpgPeer.User.Platform}#{crpgPeer.User.PlatformUserId})");

                if (_crpgTeamSelectServerComponent != null)
                {
                    _crpgTeamSelectServerComponent.ChangeTeamServer(networkPeer, Mission.SpectatorTeam);
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new CrpgNotificationId
                    {
                        Type = CrpgNotificationType.Notification,
                        TextId = "str_notification",
                        TextVariation = "moved_to_spectator",
                        SoundEvent = string.Empty,
                    });
                    GameNetwork.EndModuleEventAsServer();
                }
                else
                {
                    KickHelper.Kick(networkPeer, DisconnectType.Inactivity);
                }

                return;
            }

            const int warningTime = 15;
            if (MissionTime.Now - lastActiveStatus.LastActive > _inactiveTimeLimit - MissionTime.Seconds(warningTime) && !lastActiveStatus.Warned)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotificationId
                {
                    Type = CrpgNotificationType.Notification,
                    TextId = "str_notification",
                    TextVariation = "inactivity_warning",
                    SoundEvent = "event:/ui/notification/alert",
                    Variables = { ["SECONDS"] = warningTime.ToString() },
                });
                GameNetwork.EndModuleEventAsServer();

                lastActiveStatus.Warned = true;
                _lastActiveStatuses[playerId] = lastActiveStatus;
                return;
            }
        }
    }

    private struct ActivityStatus
    {
        public MissionTime LastActive { get; set; }
        public Agent.MovementControlFlag MovementFlags { get; set; }
        public Vec2 MovementInputVector { get; set; }
        public Vec3 LookDirection { get; set; }
        public bool IsUsingGameObject { get; set; }
        public bool Warned { get; set; }
    }
}
