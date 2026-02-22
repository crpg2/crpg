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

internal class BreakableWeaponsBehaviorServer : MissionBehavior
{
    // Todo : Move this to an Xml
    public static readonly Dictionary<string, short> BreakAbleItemsHitPoints = new()
    {
        { "crpg_joustinglance_striped_ry_v1_h0", 500 },
        { "crpg_joustinglance_striped_ry_v1_h1", 550 },
        { "crpg_joustinglance_striped_ry_v1_h2", 600 },
        { "crpg_joustinglance_striped_ry_v1_h3", 650 },
    };

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

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

            if (!BreakAbleItemsHitPoints.TryGetValue(weapon.Item?.StringId ?? string.Empty, out short baseHitPoints))
            {
                continue;
            }

            agent.ChangeWeaponHitPoints((EquipmentIndex)i, baseHitPoints);
        }
    }

    public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
    {
        if (collisionData.AffectorWeaponSlotOrMissileIndex == -1) // fists
        {
            return;
        }

        if (attacker?.Equipment[collisionData.AffectorWeaponSlotOrMissileIndex].Item == null)
        {
            return;
        }

        if (!BreakAbleItemsHitPoints.TryGetValue(attacker?.Equipment[collisionData.AffectorWeaponSlotOrMissileIndex].Item.StringId ?? string.Empty, out short baseHitPoints))
        {
            return;
        }

        EquipmentIndex attackerWeaponIndex = (EquipmentIndex)collisionData.AffectorWeaponSlotOrMissileIndex;

        int blowDone = collisionData.AbsorbedByArmor + collisionData.InflictedDamage;

        if (attacker!.WieldedWeapon.HitPoints == 1) // Roll to see if Item will break
        {
            int randomNumber = MBRandom.RandomInt(0, 1000);

            if (randomNumber >= blowDone) // does not break
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpdateWeaponHealth { AgentIndex = attacker.Index, EquipmentIndex = attackerWeaponIndex, WeaponHealth = 1, LastBlow = blowDone, LastRoll = randomNumber });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
            else // item breaks
            {
                int soundIndex = SoundEvent.GetEventIdFromString("event:/mission/combat/shield/broken");
                attacker.RemoveEquippedWeapon(attackerWeaponIndex);
                Mission.Current.MakeSound(soundIndex, attacker.Position, false, true, -1, -1);
            }
        }
        else // item loses hp
        {
            short newHealth = (short)Math.Max(1, attacker!.WieldedWeapon.HitPoints - blowDone);

            attacker!.ChangeWeaponHitPoints(attackerWeaponIndex, newHealth);

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateWeaponHealth { AgentIndex = attacker.Index, EquipmentIndex = attackerWeaponIndex, WeaponHealth = newHealth });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }
    }
}
