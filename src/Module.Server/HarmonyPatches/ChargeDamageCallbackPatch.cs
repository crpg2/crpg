using System.Reflection;
using System.Reflection.Emit;
using Crpg.Module.Common;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

// This patch modifies the ChargeDamageCallback method in the Mission class to enable friendly fire by bypassing conditional.

[HarmonyPatch(typeof(Mission), "ChargeDamageCallback")]
public static class ChargeDamageCallbackPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Debug.Print("Patched ChargeDamageCallback ran!", 0, TaleWorlds.Library.Debug.DebugColor.Cyan);
        var codes = instructions.ToList();
        var method = AccessTools.Method(typeof(ChargeDamageControl), nameof(ChargeDamageControl.ShouldAllowChargeDamage));

        for (int i = 0; i < codes.Count - 2; i++)
        {
            if (codes[i].opcode == OpCodes.Ldarg_3 &&
                codes[i + 1].opcode == OpCodes.Ldarg_S &&
                codes[i + 2].opcode == OpCodes.Callvirt &&
                codes[i + 2].operand is MethodInfo isEnemyMethod &&
                isEnemyMethod.Name == "IsEnemyOf")
            {
                // Replace the check with our custom logic
                codes[i] = new CodeInstruction(OpCodes.Ldarg_3); // attacker
                codes[i + 1] = new CodeInstruction(OpCodes.Ldarg_S, codes[i + 1].operand); // victim
                codes[i + 2] = new CodeInstruction(OpCodes.Call, method); // call our method
                // Keep the branch instruction (i + 3) intact

                break;
            }
        }

        return codes;
    }
}
