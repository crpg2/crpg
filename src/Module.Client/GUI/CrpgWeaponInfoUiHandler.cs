using System.Reflection;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.GUI.Hud;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

internal class CrpgWeaponInfoUiHandler : MissionView
{
    private const float HideWeaponInfoDelay = 3f;

    private static readonly Dictionary<WeaponFlags, string> _friendlyFlagNames = new()
    {
        { WeaponFlags.AutoReload, "Auto Reload On" },
        { WeaponFlags.BonusAgainstShield, "Bonus vs. Shield" },
        { WeaponFlags.CanCrushThrough, "CrushThrough" },
        { WeaponFlags.CanDismount, "Dismount on Stab" },
        { WeaponFlags.CanHook, "Dismount on Swing" },
        { WeaponFlags.CanKnockDown, "Knockdown" },
        { WeaponFlags.CantReloadOnHorseback, "Can't Reload On Horseback" },
    };
    private static readonly Dictionary<string, string> _itemUsageKeywords = new()
{
    { "couch", "Can Couch" },
    { "bracing", "Can Brace" },
    // Add more as needed
};
    private readonly Dictionary<string, HudTextNotificationWidget> _persistentLines = new();
    private readonly Dictionary<string, HudTextNotificationWidget> _weaponFlagLines = new();
    private readonly Dictionary<string, HudTextNotificationWidget> _weaponUsageLines = new();


    private CrpgWeaponInfoVm _dataSource;
    private GauntletLayer? _gauntletLayer;
    private float _timeSinceWeaponUsageChange = 0f;

    public CrpgWeaponInfoUiHandler()
    {
        _dataSource = new CrpgWeaponInfoVm(Mission); // Guaranteed non-null
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        if (Mission.Current != null)
        {
            Mission.Current.OnItemPickUp += HandleAgentItemPickup;
            Mission.Current.OnItemDrop += HandleAgentItemDrop;
        }

        if (Mission.MainAgent != null)
        {
            Mission.MainAgent.OnMainAgentWieldedItemChange += HandleMainAgentWieldedItemChanged;
            Mission.MainAgent.OnAgentMountedStateChanged += HandleMainAgentMountedStateChanged;
        }

        _dataSource.ShowWeaponUsageIndex = false;
        _timeSinceWeaponUsageChange = 0f;
        _dataSource.WeaponUsageIndex = -1;

        // Initialize Gauntlet UI layer
        try
        {
            _gauntletLayer = new GauntletLayer(ViewOrderPriority);
            var movie = _gauntletLayer.LoadMovie("CrpgWeaponInfoHud", _dataSource);
            CrpgHudNotificationManager.Instance.Initialize(movie, _gauntletLayer);


            MissionScreen.AddLayer(_gauntletLayer);

        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash CrpgWeaponInfoHandler: {ex.Message}"));
            InformationManager.DisplayMessage(new InformationMessage(
                $"UI crash CrpgWeaponInfoHandler: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        if (Mission.Current != null)
        {
            Mission.Current.OnItemPickUp -= HandleAgentItemPickup;
            Mission.Current.OnItemDrop -= HandleAgentItemDrop;
        }

        if (Mission.MainAgent != null)
        {
            Mission.MainAgent.OnMainAgentWieldedItemChange -= HandleMainAgentWieldedItemChanged;
            Mission.MainAgent.OnAgentMountedStateChanged -= HandleMainAgentMountedStateChanged;
        }

        ClearTrackedHudLines(_persistentLines);
        ClearTrackedHudLines(_weaponFlagLines);
        ClearTrackedHudLines(_weaponUsageLines);

        _dataSource!.OnFinalize();
        _dataSource = null!;
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        Agent agent = Mission.Current.MainAgent;

        _timeSinceWeaponUsageChange += dt;

        if (_timeSinceWeaponUsageChange >= HideWeaponInfoDelay)
        {
            _dataSource.ShowWeaponUsageIndex = false;
        }

        UpdateWeaponUsageGui();
        _dataSource?.OnMissionScreenTick(dt);

        CrpgHudNotificationManager.Instance.Update(dt);
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);

        if (agent == null || Mission.MainAgent == null || agent != Mission.MainAgent || !agent.IsActive())
        {
            return;
        }

        UpdateWeaponUsageGui();
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

        if (Mission.MainAgent != null && affectedAgent == Mission.MainAgent)
        {
            ClearTrackedHudLines(_persistentLines);
            ClearTrackedHudLines(_weaponFlagLines);
            ClearTrackedHudLines(_weaponUsageLines);
            HideWeaponUsageGui();
        }
    }

    private static (int usageIndex, string usageName, MissionWeapon mWeapon, Agent.HandIndex handIndex, WeaponComponentData? weaponData, WeaponFlags? flags)? GetCurrentWeaponUsage(Agent agent)
    {
        if (agent == null || !agent.IsActive())
        {
            return null;
        }

        Agent.HandIndex handIndex;
        EquipmentIndex wieldedIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);

        if (wieldedIndex != EquipmentIndex.None)
        {
            handIndex = Agent.HandIndex.MainHand;
        }
        else
        {
            wieldedIndex = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (wieldedIndex == EquipmentIndex.None)
            {
                return null;
            }

            handIndex = Agent.HandIndex.OffHand;
        }

        if (wieldedIndex < EquipmentIndex.WeaponItemBeginSlot || wieldedIndex > EquipmentIndex.ExtraWeaponSlot)
        {
            return null;
        }

        MissionWeapon mWeapon = agent.Equipment[wieldedIndex];
        if (mWeapon.IsEmpty || mWeapon.IsEqualTo(MissionWeapon.Invalid))
        {
            return null;
        }

        int usageIndex = mWeapon.CurrentUsageIndex;
        WeaponComponentData? weaponData = mWeapon.GetWeaponComponentDataForUsage(usageIndex);
        WeaponFlags? flags = weaponData?.WeaponFlags;

        string usageName = weaponData?.WeaponDescriptionId?.ToString() ?? "Unknown";

        return (usageIndex, usageName, mWeapon, handIndex, weaponData, flags);
    }

    private void HandleMainAgentWieldedItemChanged()
    {
        _dataSource.WeaponUsageIndex = -1;
        UpdateWeaponUsageGui(true);
    }

    private void HandleMainAgentMountedStateChanged()
    {
        _dataSource.WeaponUsageIndex = -1;
        UpdateWeaponUsageGui(true);
    }

    private void HandleAgentItemDrop(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && Mission.MainAgent != null && agent == Mission.MainAgent && agent.IsActive())
        {
            InformationManager.DisplayMessage(new InformationMessage($"HandleAgentItemDrop()"));
            _dataSource.WeaponUsageIndex = -1;
            UpdateWeaponUsageGui(true);
        }
    }

    private void HandleAgentItemPickup(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && Mission.MainAgent != null && agent == Mission.MainAgent && agent.IsActive())
        {
            InformationManager.DisplayMessage(new InformationMessage($"HandleAgentItemPickup()"));
            _dataSource.WeaponUsageIndex = -1;
            UpdateWeaponUsageGui(true);
        }
    }

    private void HideWeaponUsageGui()
    {
        _dataSource.ShowWeaponUsageIndex = false;
        _timeSinceWeaponUsageChange = 0f;
    }

    private bool IsSwashbucklerPossible(Agent agent, MissionWeapon wieldedWeapon)
    {
        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        if (wieldedWeapon.IsEmpty || wieldedWeapon.IsEqualTo(MissionWeapon.Invalid))
        {
            return false;
        }

        if (wieldedWeapon.CurrentUsageItem.WeaponClass != WeaponClass.OneHandedSword ||
            wieldedWeapon.CurrentUsageItem.SwingDamageType != DamageTypes.Cut)
        {
            return false;
        }

        for (int i = 0; i < (int)EquipmentIndex.NumAllWeaponSlots; i++)
        {
            MissionWeapon iWeapon = agent.Equipment[i];

            if (iWeapon.IsEmpty || iWeapon.IsEqualTo(MissionWeapon.Invalid))
            {
                continue;
            }

            if (iWeapon.IsShield() || iWeapon.CurrentUsageItem?.IsRangedWeapon == true)
            {
                return false;
            }

            ItemObject item = iWeapon.Item;
            if (item?.WeaponComponent?.Weapons == null)
            {
                continue;
            }

            foreach (WeaponComponentData wCompData in item.WeaponComponent.Weapons)
            {
                if (wCompData?.IsRangedWeapon == true)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void UpdateSpecialUsageNotification(string key, bool shouldShow, string text, string style = "Blue")
    {
        if (CrpgHudNotificationManager.Instance == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgHudNotificationManager.Instance not initialized!"));
            return;
        }

        if (shouldShow)
        {
            if (!_persistentLines.ContainsKey(key))
            {
                HudTextNotificationWidget? widget = CrpgHudNotificationManager.Instance.AddRight(text, style, -1f); // infinite lifetime
                if (widget != null)
                {
                    _persistentLines[key] = widget;
                }
            }
        }
        else
        {
            if (_persistentLines.TryGetValue(key, out var widget))
            {
                CrpgHudNotificationManager.Instance.Remove(widget);
                _persistentLines.Remove(key);
            }
        }
    }


    /*
        private void UpdateSpecialUsageNotification(string key, bool shouldShow, string text, string style = "Blue")
        {
            if (shouldShow)
            {
                if (!_persistentLines.ContainsKey(key))
                {
                    var line = HudTextNotificationManager.Instance.AddRight(text, style, -1f); // permanent
                    line.RefreshValues();
                    line.SetPropertyValue("Style", style);
                    _dataSource.RefreshValues();
                    _persistentLines[key] = line;
                }
            }
            else
            {
                if (_persistentLines.TryGetValue(key, out var line))
                {
                    HudTextNotificationManager.Instance.RightLines.Remove(line);
                    _persistentLines.Remove(key);
                }
            }
        }
    */

    private void UpdateSpecialUsageInfo(Agent mainAgent, MissionWeapon mWeapon)
    {
        UpdateSpecialUsageNotification("Swashbuckler", IsSwashbucklerPossible(mainAgent, mWeapon), "Swashbuckler", "Yellow");
        // Add more as needed
    }

    private void UpdateWeaponFlagLines(WeaponFlags flags)
    {
        HashSet<string> currentActiveKeys = new();

        foreach (var kvp in _friendlyFlagNames)
        {
            var flag = kvp.Key;
            var label = kvp.Value;

            if ((flags & flag) == flag)
            {
                currentActiveKeys.Add(label);

                if (!_weaponFlagLines.ContainsKey(label))
                {
                    HudTextNotificationWidget? widget = CrpgHudNotificationManager.Instance.AddRight(label, "Grey", 5f);
                    if (widget != null)
                    {
                        _weaponFlagLines[label] = widget;
                    }
                }
            }
        }

        // Remove stale lines
        var toRemove = _weaponFlagLines.Keys.Where(k => !currentActiveKeys.Contains(k)).ToList();
        foreach (string? key in toRemove)
        {
            if (_weaponFlagLines.TryGetValue(key, out var widget))
            {
                CrpgHudNotificationManager.Instance.Remove(widget);
                _weaponFlagLines.Remove(key);
            }
        }
    }

    private void UpdateWeaponUsageLinesFromItemUsage(WeaponComponentData? weaponData)
    {
        if (weaponData == null)
        {
            return;
        }

        string itemUsageLower = weaponData.ItemUsage?.ToLower() ?? string.Empty;
        HashSet<string> currentLabels = new();

        foreach (var kvp in _itemUsageKeywords)
        {
            string keyword = kvp.Key;
            string displayText = kvp.Value;

            if (itemUsageLower.Contains(keyword))
            {
                currentLabels.Add(displayText);

                if (!_weaponUsageLines.ContainsKey(displayText))
                {
                    HudTextNotificationWidget? widget = CrpgHudNotificationManager.Instance.AddRight(displayText, "Yellow", -1f); // infinite lifetime
                    if (widget != null)
                    {
                        _weaponUsageLines[displayText] = widget;
                    }
                }
            }
        }

        var staleKeys = _weaponUsageLines.Keys.Where(k => !currentLabels.Contains(k)).ToList();
        foreach (var key in staleKeys)
        {
            if (_weaponUsageLines.TryGetValue(key, out var widget))
            {
                CrpgHudNotificationManager.Instance.Remove(widget);
                _weaponUsageLines.Remove(key);
            }
        }
    }

    private void ClearTrackedHudLines(Dictionary<string, HudTextNotificationWidget> lineDict)
    {
        if (CrpgHudNotificationManager.Instance == null)
        {
            return;
        }

        foreach (var widget in lineDict.Values)
        {
            CrpgHudNotificationManager.Instance.Remove(widget);
        }

        lineDict.Clear();
    }


    private void UpdateWeaponUsageGui(bool forced = false)
    {
        Agent mainAgent = Mission.MainAgent;
        if (mainAgent == null || !mainAgent.IsActive())
        {
            _dataSource.ShowWeaponUsageIndex = false;
            _timeSinceWeaponUsageChange = 0f;
            return;
        }

        // start new logic
        EquipmentIndex wIndexMainHand = mainAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        EquipmentIndex wIndexOffHand = mainAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        MissionWeapon mWeapon = MissionWeapon.Invalid;
        MissionWeapon mWeapon2 = MissionWeapon.Invalid;

        if (wIndexMainHand != EquipmentIndex.None &&
            (wIndexMainHand >= EquipmentIndex.WeaponItemBeginSlot && wIndexMainHand <= EquipmentIndex.ExtraWeaponSlot))
        {
            mWeapon = mainAgent.Equipment[wIndexMainHand];
        }

        if (wIndexOffHand != EquipmentIndex.None &&
        (wIndexOffHand >= EquipmentIndex.WeaponItemBeginSlot && wIndexOffHand <= EquipmentIndex.ExtraWeaponSlot))
        {
            mWeapon2 = mainAgent.Equipment[wIndexOffHand];
        }

        if ((mWeapon.IsEmpty || mWeapon.IsEqualTo(MissionWeapon.Invalid)) && (mWeapon2.IsEmpty || mWeapon2.IsEqualTo(MissionWeapon.Invalid))) // no weapon wielded
        {
            ClearTrackedHudLines(_persistentLines);
            ClearTrackedHudLines(_weaponFlagLines);
            ClearTrackedHudLines(_weaponUsageLines);

            _dataSource.ShowWeaponUsageIndex = false;
            _timeSinceWeaponUsageChange = 0f;
            return;
        }

        if (!mWeapon.IsEmpty)
        {
            int usageIndex = mWeapon.CurrentUsageIndex;
            WeaponComponentData? weaponData = mWeapon.GetWeaponComponentDataForUsage(usageIndex);
            WeaponFlags? maybeFlags = weaponData?.WeaponFlags;

            UpdateSpecialUsageInfo(mainAgent, mWeapon);
            if (maybeFlags.HasValue)
            {
                var flags = maybeFlags.Value;
                UpdateWeaponFlagLines(flags);
            }

            UpdateWeaponUsageLinesFromItemUsage(weaponData);

            _dataSource.ShowWeaponUsageIndex = true;
            _timeSinceWeaponUsageChange = 0f;
            _dataSource.WeaponUsageIndex = usageIndex;
        }
        else
        {
            if (!mWeapon2.IsEmpty)
            {
                int usageIndex = mWeapon2.CurrentUsageIndex;
                WeaponComponentData? weaponData = mWeapon2.GetWeaponComponentDataForUsage(usageIndex);
                WeaponFlags? maybeFlags = weaponData?.WeaponFlags;

                UpdateSpecialUsageInfo(mainAgent, mWeapon2);
                if (maybeFlags.HasValue)
                {
                    var flags = maybeFlags.Value;
                    UpdateWeaponFlagLines(flags);
                }

                UpdateWeaponUsageLinesFromItemUsage(weaponData);

                _dataSource.ShowWeaponUsageIndex = true;
                _timeSinceWeaponUsageChange = 0f;
                _dataSource.WeaponUsageIndex = usageIndex;
            }
        }

        _dataSource.RefreshValues();
    }
}




/*
  public enum WeaponFlags : ulong
  {
    MeleeWeapon = 1,
    RangedWeapon = 2,
    WeaponMask = RangedWeapon | MeleeWeapon, // 0x0000000000000003
    FirearmAmmo = 4,
    NotUsableWithOneHand = 16, // 0x0000000000000010
    NotUsableWithTwoHand = 32, // 0x0000000000000020
    HandUsageMask = NotUsableWithTwoHand | NotUsableWithOneHand, // 0x0000000000000030
    WideGrip = 64, // 0x0000000000000040
    AttachAmmoToVisual = 128, // 0x0000000000000080
    Consumable = 256, // 0x0000000000000100
    HasHitPoints = 512, // 0x0000000000000200
    DataValueMask = HasHitPoints | Consumable, // 0x0000000000000300
    HasString = 1024, // 0x0000000000000400
    StringHeldByHand = 3072, // 0x0000000000000C00
    UnloadWhenSheathed = 4096, // 0x0000000000001000
    AffectsArea = 8192, // 0x0000000000002000
    AffectsAreaBig = 16384, // 0x0000000000004000
    Burning = 32768, // 0x0000000000008000
    BonusAgainstShield = 65536, // 0x0000000000010000
    CanPenetrateShield = 131072, // 0x0000000000020000
    CantReloadOnHorseback = 262144, // 0x0000000000040000
    AutoReload = 524288, // 0x0000000000080000
    CanKillEvenIfBlunt = 1048576, // 0x0000000000100000
    TwoHandIdleOnMount = 2097152, // 0x0000000000200000
    NoBlood = 4194304, // 0x0000000000400000
    PenaltyWithShield = 8388608, // 0x0000000000800000
    CanDismount = 16777216, // 0x0000000001000000
    CanHook = 33554432, // 0x0000000002000000
    CanKnockDown = 67108864, // 0x0000000004000000
    CanCrushThrough = 134217728, // 0x0000000008000000
    CanBlockRanged = 268435456, // 0x0000000010000000
    MissileWithPhysics = 536870912, // 0x0000000020000000
    MultiplePenetration = 1073741824, // 0x0000000040000000
    LeavesTrail = 2147483648, // 0x0000000080000000
    UseHandAsThrowBase = 4294967296, // 0x0000000100000000
    AmmoBreaksOnBounceBack = 68719476736, // 0x0000001000000000
    AmmoCanBreakOnBounceBack = 137438953472, // 0x0000002000000000
    AmmoBreakOnBounceBackMask = AmmoCanBreakOnBounceBack | AmmoBreaksOnBounceBack, // 0x0000003000000000
    AmmoSticksWhenShot = 274877906944, // 0x0000004000000000
  }
}
*/
