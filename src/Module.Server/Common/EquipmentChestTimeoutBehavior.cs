using Crpg.Module.Api.Models.Users;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Scripts;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class EquipmentChestTimeoutBehavior : MissionBehavior
{
    public event Action<CrpgPeer> OnEquipChestUsed = default!;
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    public Dictionary<int, float> TimeSinceLastRearmed = new Dictionary<int, float>();

    public MissionTime CurrentRoundStartTime;

    private readonly CrpgSpawningBehaviorBase _spawnBehavior;
    private readonly float rearmTimeout = 3000; // Time between equipment chest uses in ms
    private CrpgUserManagerServer? _crpgUserManager;

    public EquipmentChestTimeoutBehavior(CrpgSpawningBehaviorBase spawnBehavior)
    {
        _spawnBehavior = spawnBehavior;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        if (agent == null)
        {
            return;
        }

        if (agent.Equipment == null)
        {
            return;
        }
    }

    public override async void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
    {
        if (usedObject.GetType() == typeof(Scripts.CrpgStandingPoint))
        {
            CrpgStandingPoint crpgPoint = (CrpgStandingPoint)usedObject;
            if (crpgPoint != null)
            {
                if (crpgPoint.parentUsableMachine != null)
                {
                    if (crpgPoint.parentUsableMachine.GetType() == typeof(EquipmentChest))
                    {
                        var networkPeer = userAgent.MissionPeer.GetNetworkPeer();

                        if (networkPeer != null)
                        {
                            CrpgPeer? crpgPeer = networkPeer.GetComponent<CrpgPeer>();

                            if (crpgPeer.User != null)
                            {
                                int userId = crpgPeer.User.Id;
                                if (!TimeSinceLastRearmed.TryGetValue(userId, out float lastUsedChest))
                                {
                                    lastUsedChest = 0;
                                }

                                float timeDiff = CurrentRoundStartTime.ElapsedMilliseconds - lastUsedChest;
                                if (timeDiff > rearmTimeout)
                                {
                                    base.OnObjectUsed(userAgent, usedObject);
                                    OnEquipChestUsed?.Invoke(crpgPeer);

                                    _crpgUserManager = Mission.GetMissionBehavior<CrpgUserManagerServer>();
                                    CrpgUser crpgUser = await _crpgUserManager.GetUpdatedCrpgUser(networkPeer);

                                    TimeSinceLastRearmed[userId] = CurrentRoundStartTime.ElapsedMilliseconds;
                                    Debug.Print("AHHHH");
                                    Debug.Print(CurrentRoundStartTime.ElapsedMilliseconds.ToString());
                                    float previousHealth = userAgent.Health;

                                    Agent? newAgent = _spawnBehavior.RefreshPlayerWithNewLoadout(networkPeer, crpgUser);
                                    if (newAgent != null)
                                    {
                                        newAgent.Health = previousHealth;
                                    }
                                }
                                else
                                {
                                    float timeoutDuration = rearmTimeout - timeDiff;
                                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                                    GameNetwork.WriteMessage(new CrpgDtvEquipChestTimeoutMessage { TimeoutDuration = (float)timeoutDuration });
                                    GameNetwork.EndModuleEventAsServer();
                                    userAgent.StopUsingGameObject();
                                }
                            }
                        }

                        return;
                    }
                }
            }
        }
    }

}
