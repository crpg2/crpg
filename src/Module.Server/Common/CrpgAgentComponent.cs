using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common
{
    internal class CrpgAgentComponent : AgentComponent
    {
        public CrpgAgentComponent(Agent agent)
            : base(agent)
        {
            agent.OnAgentWieldedItemChange = (Action)Delegate.Combine(agent.OnAgentWieldedItemChange, new Action(DropShieldIfNeeded));
        }

        public override void OnMount(Agent mount)
        {
            DropShieldIfNeeded();
            RecalculateMountStats();
        }

        public override void OnItemPickup(SpawnedItemEntity item)
        {
            RecalculateMountStats();
        }

        public override void OnWeaponDrop(MissionWeapon droppedWeapon)
        {
            RecalculateMountStats();
        }

        private void DropShieldIfNeeded()
        {
            if (!Agent.HasMount)
                return;

            var equipment = Agent.Equipment;
            var offHandIndex = Agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            var offHandItem = offHandIndex != EquipmentIndex.None
                ? equipment[offHandIndex].CurrentUsageItem
                : null;

            if (offHandItem?.WeaponClass == WeaponClass.LargeShield)
            {
                Agent.DropItem(offHandIndex);
            }
        }

        private void RecalculateMountStats()
        {
#if CRPG_SERVER
            if (Agent.HasMount && Agent.MountAgent != null)
            {
                CrpgSubModule.AgentStatCalculateModel.UpdateMountAgentStats(Agent, Agent.MountAgent.AgentDrivenProperties);
            }
#endif
        }
    }
}
