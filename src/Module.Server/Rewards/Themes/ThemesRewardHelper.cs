using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Themes;
using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Rewards.Themes;

internal static class ThemesRewardHelper
{
    internal static (float expMultiplier, float goldMultiplier) GetActiveEventMultipliers(CrpgPeer crpgPeer, List<ThemeEvent> activeThemeEvents)
    {
        var playerEquippedItems = crpgPeer.User!.Character.EquippedItems;
        float currentHighestEventExpMultiplier = 1.0f;
        float currentHighestEventGoldMultiplier = 1.0f;

        foreach (var themeEvent in activeThemeEvents)
        {
            var playerSlotsWithEligibleItemEquipped = playerEquippedItems.Where(x => themeEvent.EligibleItemIds.Contains(x.UserItem.ItemId)).Select(x => x.Slot).ToList();

            if (PlayerIsEligibleForThemeEvent(themeEvent, playerSlotsWithEligibleItemEquipped))
            {
                if (themeEvent.ExpMultiplier > currentHighestEventExpMultiplier)
                {
                    currentHighestEventExpMultiplier = themeEvent.ExpMultiplier;
                }

                if (themeEvent.GoldMultiplier > currentHighestEventGoldMultiplier)
                {
                    currentHighestEventGoldMultiplier = themeEvent.GoldMultiplier;
                }
            }
        }

        if (currentHighestEventExpMultiplier > 1.0f || currentHighestEventGoldMultiplier > 1.0f)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CrpgRewardThemeEvent());
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        return (currentHighestEventExpMultiplier, currentHighestEventGoldMultiplier);
    }

    private static bool PlayerIsEligibleForThemeEvent(ThemeEvent themeEvent, List<CrpgItemSlot> playerSlotsWithEligibleItemEquipped)
    {
        if (playerSlotsWithEligibleItemEquipped.Count < themeEvent.MinimumThemedItemsEquipped)
        {
            return false;
        }

        var weaponSlots = new HashSet<CrpgItemSlot>
        {
            CrpgItemSlot.Weapon0, CrpgItemSlot.Weapon1,
            CrpgItemSlot.Weapon2, CrpgItemSlot.Weapon3,
            CrpgItemSlot.WeaponExtra,
        };

        int requiredWeapons = themeEvent.RequiredEquipmentSlotsMatchingTheme.Count(r => r == ThemeEquipmentSlot.Weapon);
        int playerWeapons = playerSlotsWithEligibleItemEquipped.Count(s => weaponSlots.Contains(s));

        if (playerWeapons < requiredWeapons)
        {
            return false;
        }

        return themeEvent.RequiredEquipmentSlotsMatchingTheme
            .Where(r => r != ThemeEquipmentSlot.Weapon)
            .All(r => playerSlotsWithEligibleItemEquipped.Contains(MapToThemeEquipmentSlot(r)));
    }

    private static CrpgItemSlot MapToThemeEquipmentSlot(ThemeEquipmentSlot crpgItemSlot)
    {
        switch (crpgItemSlot)
        {
            case ThemeEquipmentSlot.Head:
                return CrpgItemSlot.Head;
            case ThemeEquipmentSlot.Shoulder:
                return CrpgItemSlot.Shoulder;
            case ThemeEquipmentSlot.Body:
                return CrpgItemSlot.Body;
            case ThemeEquipmentSlot.Hand:
                return CrpgItemSlot.Hand;
            case ThemeEquipmentSlot.Leg:
                return CrpgItemSlot.Leg;
            case ThemeEquipmentSlot.MountHarness:
                return CrpgItemSlot.MountHarness;
            case ThemeEquipmentSlot.Mount:
                return CrpgItemSlot.Mount;
            default:
                throw new ArgumentOutOfRangeException("Theme equipment slot did not exist on in character equipment slots.");
        }
    }
}
