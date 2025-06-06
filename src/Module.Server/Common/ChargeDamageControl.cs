using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/*
    // This class controls the charge damage behavior in the game through ChargeDamageCallbackPatch
    DisableAllChargeDamage = true   No charge damage at all ***overrides other flags***
    AllowChargeFriends = true       You can bump allies
    AllowChargeEnemies = true      You can bump enemies
*/
public static class ChargeDamageControl
{
    public static bool DisableAllChargeDamage { get; set; } = false;
    public static bool AllowChargeFriends { get; set; } = true;
    public static bool AllowChargeEnemies { get; set; } = true;

    public static bool ShouldAllowChargeDamage(Agent attacker, Agent victim)
    {
        if (DisableAllChargeDamage)
        {
            return false;
        }

        if (!AllowChargeEnemies && attacker.IsEnemyOf(victim))
        {
            return false;
        }

        if (!AllowChargeFriends && !attacker.IsEnemyOf(victim))
        {
            return false;
        }

        // Allow charge damage if no blocking rule applies
        return true;
    }
}
