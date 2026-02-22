using Crpg.Module.Common.Network;
using Crpg.Module.Notifications;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Common;

internal class BreakableWeaponsBehaviorClient : MissionNetwork
{
    public int LastRoll { get; private set; }
    public int LastBlow { get; private set; }

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

        for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NonWeaponItemBeginSlot; i++)
        {
            MissionWeapon weapon = agent.Equipment[i];

            if (!BreakableWeaponsBehaviorServer.BreakAbleItemsHitPoints.TryGetValue(weapon.Item?.StringId ?? string.Empty, out short baseHitPoints))
            {
                continue;
            }

            agent.ChangeWeaponHitPoints((EquipmentIndex)i, baseHitPoints);
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UpdateWeaponHealth>(HandleUpdateWeaponHealth);
    }

    private void HandleUpdateWeaponHealth(UpdateWeaponHealth message)
    {
        Agent agentToUpdate = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentIndex, true);
        if (agentToUpdate == null)
        {
            Debug.Print($"CRPGLOG : HandleUpdateWeaponHealth received a null agent : {message.AgentIndex}  ");
            return;
        }

        agentToUpdate.ChangeWeaponHitPoints(message.EquipmentIndex, (short)message.WeaponHealth);
        LastRoll = message.LastRoll;
        LastBlow = message.LastBlow;
    }
}
