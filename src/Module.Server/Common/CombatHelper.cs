using TaleWorlds.Library;

namespace Crpg.Module.Helpers
{
    public static class CrpgCombatHelpers
    {
        /// Computes the penalty multiplier applied to mount stats when wielding overly long weapons for a given strength.
        public static float ComputeMountedWeaponPenalty(int strength, float weaponLength)
        {
            float maxLength = MaxWeaponLengthForStrLevel(strength);
            float rawRatio = maxLength / weaponLength;
            return MBMath.ClampFloat(MBMath.Lerp(0.5f, 1.0f, rawRatio), 0.5f, 1.0f);
        }

        public static float MaxWeaponLengthForStrLevel(int strengthSkill)
        {
            int uncapped = (int)(22 + (strengthSkill - 3) * 7.5 + System.Math.Pow(System.Math.Min(strengthSkill - 3, 24) * 0.115f, 7.75f));
            return System.Math.Min(uncapped, 650);
        }
    }
}
