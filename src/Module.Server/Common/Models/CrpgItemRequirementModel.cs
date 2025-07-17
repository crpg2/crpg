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
            case ItemObject.ItemTypeEnum.Musket:
            case ItemObject.ItemTypeEnum.Pistol:
                return ComputeCrossbowRequirement(item);
        }

        return 0;
    }

    private static int ComputeCrossbowRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenCrossbow;

        // Check if the item is a crossbow or firearm
        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow &&
            item.ItemType != ItemObject.ItemTypeEnum.Musket &&
            item.ItemType != ItemObject.ItemTypeEnum.Pistol)
        {
            throw new ArgumentException(item.Name.ToString() + " is not a crossbow or firearm");
        }

        // Adjust the strength requirement for light crossbows and light guns
        var usage = item.WeaponComponent.PrimaryWeapon.ItemUsage;
        if (usage.Contains("crossbow_light") || usage.Contains("crpg_light_gun"))
        {
            strengthRequirementForTierTenCrossbow = 18; // For light xbows & guns
        }
        else
        {
            strengthRequirementForTierTenCrossbow = 20; // Default for xbows & guns
        }

        // Compute the strength requirement based on tier
        return (int)(Math.Ceiling((item.Tierf * (strengthRequirementForTierTenCrossbow / 9.9f)) / 3) * 3);
    }
}
