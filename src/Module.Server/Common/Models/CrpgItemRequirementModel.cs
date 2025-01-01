using TaleWorlds.Core;

namespace Crpg.Module.Common.Models;

internal class CrpgItemRequirementModel
{
    private readonly CrpgConstants _constants;

    public CrpgItemRequirementModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public static int ComputeItemRequirement(ItemObject item)
    {
        switch (item.ItemType)
        {
            case ItemObject.ItemTypeEnum.Crossbow:
                return ComputeCrossbowRequirement(item);
        }

        return 0;
    }

    private static int ComputeCrossbowRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenCrossbow;

        // Check if the item is a crossbow
        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow)
        {
            throw new ArgumentException(item.Name.ToString() + " is not a crossbow");
        }

        // Adjust the strength requirement for light crossbows
        if (item.WeaponComponent.PrimaryWeapon.ItemUsage.Contains("crossbow_light"))
        {
            strengthRequirementForTierTenCrossbow = 18; // For light crossbows
        }
        else
        {
            strengthRequirementForTierTenCrossbow = 20; // Default for other crossbows
        }

        // Compute the strength requirement based on tier
        return (int)(Math.Ceiling((item.Tierf * (strengthRequirementForTierTenCrossbow / 9.9f)) / 3) * 3);
    }
}
