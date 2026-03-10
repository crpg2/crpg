using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;
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
    }

    public void DropShieldIfNeeded()
    {
        if (Agent.HasMount)
        {
            MissionEquipment equipment = Agent.Equipment;
            EquipmentIndex offHandItemIndex = Agent.GetOffhandWieldedItemIndex();
            WeaponComponentData? offHandItem = offHandItemIndex != EquipmentIndex.None
                ? equipment[offHandItemIndex].CurrentUsageItem
                : null;
            if (offHandItem == null)
            {
                return;
            }

            if (offHandItem.WeaponClass == WeaponClass.LargeShield)
            {
                Agent.DropItem(offHandItemIndex);
            }
        }
    }
}
