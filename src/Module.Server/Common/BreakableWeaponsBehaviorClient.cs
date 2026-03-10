using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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

            agent.ChangeWeaponHitPoints(i, baseHitPoints);
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
