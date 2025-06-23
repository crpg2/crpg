using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

internal class CrpgWeaponInfoUiHandler : MissionView
{
    private const float HideWeaponInfoDelay = 5f;
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

        if (Mission.MainAgent != null)
        {
            Mission.MainAgent.OnMainAgentWieldedItemChange += HandleMainAgentWieldedItemChanged;
            Mission.MainAgent.OnAgentHealthChanged += HandleAgentHealthChanged;
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

        if (Mission.MainAgent != null)
        {
            Mission.MainAgent.OnMainAgentWieldedItemChange -= HandleMainAgentWieldedItemChanged;
            Mission.MainAgent.OnAgentHealthChanged -= HandleAgentHealthChanged;
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

    private static (int usageIndex, string usageName, MissionWeapon mWeapon, Agent.HandIndex handIndex, string itemUsage, WeaponFlags? flags)? GetCurrentWeaponUsage(Agent agent)
    {
        if (agent == null || !agent.IsActive())
        {
            return null;
        }

        EquipmentIndex wieldedIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        Agent.HandIndex handIndex = Agent.HandIndex.MainHand;

        if (wieldedIndex == EquipmentIndex.None)
        {
            wieldedIndex = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            handIndex = Agent.HandIndex.OffHand;
        }

        if (wieldedIndex == EquipmentIndex.None || wieldedIndex < EquipmentIndex.WeaponItemBeginSlot || wieldedIndex > EquipmentIndex.ExtraWeaponSlot)
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
        string currentItemUsageText = mWeapon.CurrentUsageItem.ToString() ?? "Unknown";

        string usageName = weaponData?.WeaponDescriptionId?.ToString() ?? "Unknown";
        string itemUsage = weaponData?.ItemUsage?.ToString() ?? "Unknown";

        WeaponFlags? flags = weaponData?.WeaponFlags;

        return (usageIndex, usageName, mWeapon, handIndex, itemUsage, flags);
    }

    private void HandleAgentHealthChanged(Agent agent, float oldHealth, float newHealth)
    {
        if (agent == null || Mission.MainAgent == null || agent != Mission.MainAgent)
        {
            return;
        }

        if (newHealth <= 0) // agent died
        {
        }
    }

    private void HandleMainAgentWieldedItemChanged()
    {
        _hasDisplayedOnce = false;
        _processWeaponUsage = true;
        _dataSource.WeaponUsageIndex = -1;
        UpdateWeaponUsageGui();
    }

    private void HideWeaponUsageGui()
    {
        _dataSource.ShowWeaponUsageInfo = false;
        _timeSinceWeaponUsageChange = 0f;
    }

    private void UpdateWeaponUsageGui()
    {
        Agent agent = Mission.MainAgent;
        if (agent == null || !agent.IsActive())
        {
            _dataSource.ShowWeaponUsageInfo = false;
            _timeSinceWeaponUsageChange = 0f;
            return;
        }

        var usageInfo = GetCurrentWeaponUsage(agent);
        if (usageInfo == null)
        {
            _dataSource.ShowWeaponUsageInfo = false;
            _timeSinceWeaponUsageChange = 0f; // Reset timer since we hid the UI
            return;
        }

        string usageName = usageInfo.Value.usageName;
        MissionWeapon mWeapon = usageInfo.Value.mWeapon;
        Agent.HandIndex handIndex = usageInfo.Value.handIndex;
        string itemUsage = usageInfo.Value.itemUsage;
        string flagList = "Empty";
        bool indexChanged = _dataSource.WeaponUsageIndex != usageInfo.Value.usageIndex;
        bool weaponChanged = !_lastWeapon.HasValue || !_lastWeapon.Value.IsEqualTo(mWeapon);
        bool shouldUpdate = weaponChanged || indexChanged || !_hasDisplayedOnce;
        WeaponFlags? maybeFlags = usageInfo.Value.flags;

        if (shouldUpdate)
        {
            if (maybeFlags.HasValue)
            {
                WeaponFlags flags = maybeFlags.Value;
                flagList = string.Empty;

                foreach (WeaponFlags flag in Enum.GetValues(typeof(WeaponFlags)))
                {
                    if ((int)flag == 0)
                    {
                        continue; // Skip zero-valued (non-useful) flags
                    }

                    if ((flags & flag) == flag)
                    {
                        switch (flag)
                        {
                            case WeaponFlags.AutoReload:
                            case WeaponFlags.BonusAgainstShield:
                            case WeaponFlags.CanCrushThrough:
                            case WeaponFlags.CanDismount:
                            case WeaponFlags.CanHook:
                            case WeaponFlags.CanKnockDown:
                            case WeaponFlags.CanPenetrateShield:
                            case WeaponFlags.CanBlockRanged:
                            case WeaponFlags.HasHitPoints:
                                flagList += $"\n{flag}";
                                break;

                            default:
                                // skip other flags, do nothing
                                flagList += $"\n{flag}";
                                break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(flagList))
                {
                    flagList = "No Flags";
                }
            }
            else
            {
                flagList = "No Flags";
            }

            _dataSource.ShowWeaponUsageInfo = true;
            _timeSinceWeaponUsageChange = 0f;
            _lastWeapon = mWeapon;
            _dataSource.WeaponUsageIndex = usageInfo.Value.usageIndex;
            _dataSource.WeaponUsageName = flagList;
            _hasDisplayedOnce = true;
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
