using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
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

    private CrpgWeaponInfoVm _dataSource;
    private GauntletLayer? _gauntletLayer;
    private bool _processWeaponUsage = false;
    private bool _hasDisplayedOnce = false;
    private MissionWeapon? _lastWeapon;
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

        _processWeaponUsage = false;
        _dataSource.ShowWeaponUsageInfo = false;
        _hasDisplayedOnce = false;
        _timeSinceWeaponUsageChange = 0f;
        _dataSource.WeaponUsageIndex = -1;

        // Initialize Gauntlet UI layer
        try
        {
            _gauntletLayer = new GauntletLayer(ViewOrderPriority);
            _gauntletLayer.LoadMovie("CrpgWeaponInfoHud", _dataSource);
            MissionScreen.AddLayer(_gauntletLayer);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash: {ex.Message}"));
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

        _dataSource!.OnFinalize();
        _dataSource = null!;
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        Agent agent = Mission.Current.MainAgent;

        if (!_processWeaponUsage)
        {
            return;
        }

        _timeSinceWeaponUsageChange += dt;

        if (_timeSinceWeaponUsageChange >= HideWeaponInfoDelay)
        {
            _dataSource.ShowWeaponUsageInfo = false;
        }

        UpdateWeaponUsageGui();
        UpdateExpiringLines();

        _dataSource!.OnMissionScreenTick(dt);
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);

        if (agent == null || Mission.MainAgent == null || agent != Mission.MainAgent || !agent.IsActive())
        {
            return;
        }

        _hasDisplayedOnce = false;
        _processWeaponUsage = true;
        UpdateWeaponUsageGui();
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

        if (Mission.MainAgent != null && affectedAgent == Mission.MainAgent)
        {
            _hasDisplayedOnce = false;
            _processWeaponUsage = false;
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
        _hasDisplayedOnce = false;
        _processWeaponUsage = true;
        _dataSource.WeaponUsageIndex = -1;
        UpdateWeaponUsageGui();
    }

    private void HandleMainAgentMountedStateChanged()
    {
        _hasDisplayedOnce = false;
        _processWeaponUsage = true;
        _dataSource.WeaponUsageIndex = -1;
        UpdateWeaponUsageGui();
    }

    private void HandleAgentItemDrop(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && Mission.MainAgent != null && agent == Mission.MainAgent && agent.IsActive())
        {
            _hasDisplayedOnce = false;
            _processWeaponUsage = true;
            _dataSource.WeaponUsageIndex = -1;
            UpdateWeaponUsageGui();
        }
    }

    private void HandleAgentItemPickup(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && Mission.MainAgent != null && agent == Mission.MainAgent && agent.IsActive())
        {
            _hasDisplayedOnce = false;
            _processWeaponUsage = true;
            _dataSource.WeaponUsageIndex = -1;
            UpdateWeaponUsageGui();
        }
    }

    private void HideWeaponUsageGui()
    {
        _dataSource.ShowWeaponUsageInfo = false;
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

    private void UpdateExpiringLines()
    {
        float now = (float)MissionTime.Now.ToSeconds;
        for (int i = _dataSource.Lines.Count - 1; i >= 0; i--)
        {
            var line = _dataSource.Lines[i];
            if (line.IsExpired(now))
            {
                _dataSource.Lines.RemoveAt(i);
            }
        }
    }

    private void UpdateWeaponUsageGui()
    {
        Agent mainAgent = Mission.MainAgent;
        if (mainAgent == null || !mainAgent.IsActive())
        {
            _dataSource.ShowWeaponUsageInfo = false;
            _timeSinceWeaponUsageChange = 0f;
            return;
        }

        var usageInfo = GetCurrentWeaponUsage(mainAgent);
        if (usageInfo == null)
        {
            _dataSource.ShowWeaponUsageInfo = false;
            _timeSinceWeaponUsageChange = 0f; // Reset timer since we hid the UI
            return;
        }

        string usageName = usageInfo.Value.usageName;
        MissionWeapon mWeapon = usageInfo.Value.mWeapon;
        Agent.HandIndex handIndex = usageInfo.Value.handIndex;
        WeaponComponentData? weaponData = usageInfo.Value.weaponData;
        bool indexChanged = _dataSource.WeaponUsageIndex != usageInfo.Value.usageIndex;
        bool weaponChanged = !_lastWeapon.HasValue || !_lastWeapon.Value.IsEqualTo(mWeapon);
        bool shouldUpdate = weaponChanged || indexChanged || !_hasDisplayedOnce;
        WeaponFlags? maybeFlags = usageInfo.Value.flags;

        if (shouldUpdate)
        {
            var sb = new System.Text.StringBuilder();

            if (maybeFlags.HasValue)
            {
                var flags = maybeFlags.Value;

                foreach (WeaponFlags flag in Enum.GetValues(typeof(WeaponFlags)))
                {
                    if ((int)flag == 0 || (flags & flag) != flag)
                    {
                        continue;
                    }

                    if (_friendlyFlagNames.TryGetValue(flag, out string friendlyName))
                    {
                        sb.AppendLine(friendlyName);
                    }
                }
            }

            if (weaponData != null)
            {
                string itemUsageLower = weaponData.ItemUsage?.ToLower() ?? string.Empty;

                if (itemUsageLower.Contains("couch"))
                {
                    sb.AppendLine("Can Couch");
                }

                if (itemUsageLower.Contains("bracing"))
                {
                    sb.AppendLine("Can Brace");
                }
            }

            if (IsSwashbucklerPossible(mainAgent, mWeapon))
            {
                sb.AppendLine("Swashbuckler");
            }

            string finalFlags = sb.Length > 0 ? sb.ToString().TrimEnd() : "No Flags";

            _dataSource.ShowWeaponUsageInfo = true;
            _timeSinceWeaponUsageChange = 0f;
            _lastWeapon = mWeapon;
            _dataSource.WeaponUsageIndex = usageInfo.Value.usageIndex;
            _dataSource.WeaponUsageName = finalFlags;
            _hasDisplayedOnce = true;

            _dataSource.UpdateLines(new List<string>
            {
                "happy birthday",
                "To me",
                "hello",
                "another one",
            });
        }
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
