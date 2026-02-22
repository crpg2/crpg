using Crpg.Module.Common.AmmoQuiverChange;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.GUI.AmmoQuiverChange;

internal class AmmoQuiverChangeVm : ViewModel
{
    private const float MarginRightThrow = 18;
    private const float MarginRightOther = 80;

    private bool _showQuiverAmmoCount;
    private bool _showQuiverName;
    private bool _isQuiverAmmoCountAlertEnabled;
    private string _quiverName;
    private float _quiverAmmoCountMarginRight;

    private MissionWeapon _wieldedWeapon;
    private MissionWeapon _currentQuiver;
    private int _currentQuiverAmmo;
    private bool _rangedWeaponEquipped;

    private ImageIdentifierVM _quiverImage0 = null!;
    private ImageIdentifierVM _quiverImage1 = null!;
    private ImageIdentifierVM _quiverImage2 = null!;
    private ImageIdentifierVM _quiverImage3 = null!;

    public AmmoQuiverChangeVm(Mission mission)
    {
        _quiverName = string.Empty;
        _currentQuiverAmmo = 0;
        _isQuiverAmmoCountAlertEnabled = false;
        _showQuiverAmmoCount = false;
        _showQuiverName = false;
        _wieldedWeapon = MissionWeapon.Invalid;
        _currentQuiver = MissionWeapon.Invalid;
        _rangedWeaponEquipped = false;

        RefreshValues();
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
    }

    public sealed override void RefreshValues()
    {
        base.RefreshValues();
    }

    public void Tick(float dt)
    {
        if (Mission.Current?.MainAgent?.IsActive() != true)
        {
            ShowQuiverAmmoCount = false;
            ShowQuiverName = false;
        }
    }

    public bool GetCurrentQuiverAmmoAmount(out int activeQuiverAmmo, out int maxQuiverAmmo, out MissionWeapon ammoWeapon, out bool hasThrowing, out bool hasChanged)
    {
        activeQuiverAmmo = 0;
        maxQuiverAmmo = 0;
        ammoWeapon = MissionWeapon.Invalid;
        hasThrowing = false;
        hasChanged = false;

        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        // Early-out if not wielding a ranged weapon
        if (!agent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand).IsRangedWeapon)
        {
            if (!CurrentQuiver.IsEqualTo(MissionWeapon.Invalid))
            {
                hasChanged = true;
            }

            CurrentQuiver = MissionWeapon.Invalid;
            return false;
        }

        MissionWeapon mWeaponWielded = agent.Equipment[agent.GetPrimaryWieldedItemIndex()];
        if (mWeaponWielded.IsEmpty || mWeaponWielded.Item == null)
        {
            return false;
        }

        ItemObject.ItemTypeEnum currentAmmoType = ItemObject.GetAmmoTypeForItemType(mWeaponWielded.Item.Type);

        // Special case: muskets use bullets even if GetAmmoTypeForItemType doesn't return it
        if (mWeaponWielded.Item.Type == ItemObject.ItemTypeEnum.Musket)
        {
            currentAmmoType = ItemObject.ItemTypeEnum.Bullets;
        }

        // Thrown weapon special handling
        if (mWeaponWielded.Item.Type == ItemObject.ItemTypeEnum.Thrown)
        {
            if (!CurrentQuiver.IsEqualTo(mWeaponWielded))
            {
                hasChanged = true;
                CurrentQuiver = mWeaponWielded;
            }

            CurrentQuiverAmmo = mWeaponWielded.Amount;
            activeQuiverAmmo = mWeaponWielded.Amount;
            maxQuiverAmmo = mWeaponWielded.MaxAmmo;
            ammoWeapon = mWeaponWielded;
            hasThrowing = true;
            QuiverAmmoCountMarginRight = MarginRightThrow;

            return true;
        }

        // Scan equipment slots for matching ammo quiver
        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; ++equipmentIndex)
        {
            MissionWeapon mWeaponAmmo = agent.Equipment[equipmentIndex];

            if (mWeaponAmmo.IsEmpty || mWeaponAmmo.Item == null)
            {
                continue;
            }

            if (mWeaponAmmo.Item.Type != currentAmmoType)
            {
                continue;
            }

            if (mWeaponAmmo.Amount <= 0)
            {
                if (CurrentQuiver.IsEqualTo(mWeaponAmmo))
                {
                    hasChanged = true;
                }

                continue;
            }

            if (!CurrentQuiver.IsEqualTo(mWeaponAmmo))
            {
                hasChanged = true;
                CurrentQuiver = mWeaponAmmo;
            }

            CurrentQuiverAmmo = mWeaponAmmo.Amount;
            activeQuiverAmmo = mWeaponAmmo.Amount;
            maxQuiverAmmo = mWeaponAmmo.MaxAmmo;
            ammoWeapon = mWeaponAmmo;
            QuiverAmmoCountMarginRight = MarginRightOther;

            return true;
        }

        return false;
    }

    public void UpdateWieldedWeapon(EquipmentIndex weaponIndex, MissionWeapon weapon)
    {
        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive())
        {
            return;
        }

        RangedWeaponEquipped = AmmoQuiverChangeComponent.IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon, out bool isThrowingWeapon);
        string weaponName = string.Empty;
        if (!wieldedWeapon.IsEmpty)
        {
            WieldedWeapon = wieldedWeapon;
            weaponName = WieldedWeapon.Item?.Name.ToString() ?? string.Empty;
        }
        else
        {
            WieldedWeapon = MissionWeapon.Invalid;
        }
    }

    public void UpdateQuiverImages()
    {
        Agent agent = Agent.Main;

        if (agent == null || !agent.IsActive() || RangedWeaponEquipped == false || WieldedWeapon.IsEmpty || WieldedWeapon.IsEqualTo(MissionWeapon.Invalid) || WieldedWeapon.Item == null)
        {
            QuiverImage0 = new GenericImageIdentifierVM(null);
            QuiverImage1 = new GenericImageIdentifierVM(null);
            QuiverImage2 = new GenericImageIdentifierVM(null);
            QuiverImage3 = new GenericImageIdentifierVM(null);
            return;
        }

        AmmoQuiverChangeComponent.GetAgentQuiversWithAmmoEquippedForWieldedWeapon(agent, out List<int> ammoQuivers);

        // Throwing - remove wielded weapon from ammoQuivers list since there is a big picture of the weapon already
        if (WieldedWeapon.Item.Type == ItemObject.ItemTypeEnum.Thrown)
        {
            int? matchingIndex = ammoQuivers
                .FirstOrDefault(index => agent.Equipment[(EquipmentIndex)index].IsEqualTo(WieldedWeapon));

            if (matchingIndex.HasValue)
            {
                ammoQuivers.Remove(matchingIndex.Value);
            }
        }

        ImageIdentifierVM[] quiverImages = new ImageIdentifierVM[4]; // assuming max 4 quivers

        for (int i = 0; i < quiverImages.Length; i++)
        {
            if (i < ammoQuivers.Count)
            {
                // check that matches ammo
                MissionWeapon mQuiver = agent.Equipment[ammoQuivers[i]];
                quiverImages[i] = mQuiver.IsEmpty ? new GenericImageIdentifierVM(null) : new ItemImageIdentifierVM(mQuiver.Item);
            }
            else
            {
                quiverImages[i] = new GenericImageIdentifierVM(null);
            }
        }

        QuiverImage0 = quiverImages[0];
        QuiverImage1 = quiverImages[1];
        QuiverImage2 = quiverImages[2];
        QuiverImage3 = quiverImages[3];
    }

    public void UpdateWeaponStatuses()
    {
        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive())
        {
            ShowQuiverAmmoCount = false;
            ShowQuiverName = false;
            UpdateQuiverImages();
            return;
        }

        // Show Ammo count textwidget if there are multiple quivers and its not empty
        if (!GetCurrentQuiverAmmoAmount(out int currentQuiverAmmo, out int maxQuiverAmmo, out MissionWeapon ammoWeapon, out bool hasThrowing, out bool hasChanged) || currentQuiverAmmo <= 0)
        {
            if (hasChanged)
            {
                UpdateQuiverImages();
            }

            ShowQuiverAmmoCount = false;
            ShowQuiverName = false;
            return;
        }

        ShowQuiverAmmoCount = false;

        // check for this being the last quiver with ammo
        if (AmmoQuiverChangeComponent.GetAgentQuiversWithAmmoEquippedForWieldedWeapon(agent, out List<int> ammoQuivers))
        {
            // hide quiver ammo if its the only quiver or thrown weapon
            if (ammoQuivers.Count() > 1 || hasThrowing)
            {
                ShowQuiverAmmoCount = true;
                float f = maxQuiverAmmo * 0.2f;
                IsQuiverAmmoCountAlertEnabled = maxQuiverAmmo != CurrentQuiverAmmo && CurrentQuiverAmmo <= MathF.Ceiling(f);
            }
        }

        // display quiver name,
        if (!ammoWeapon.IsEqualTo(MissionWeapon.Invalid))
        {
            // check if the ammo weapon is the wielded weapon ie throwing
            EquipmentIndex wieldedWeaponIndex = agent.GetPrimaryWieldedItemIndex();
            if (wieldedWeaponIndex != EquipmentIndex.None)
            {
                MissionWeapon wieldedWeapon = agent.Equipment[wieldedWeaponIndex];
                if (!wieldedWeapon.IsEmpty && wieldedWeapon.Item != null)
                {
                    QuiverName = wieldedWeapon.Item.Type == ItemObject.ItemTypeEnum.Thrown
                        ? wieldedWeapon.Item.Name.ToString()
                        : ammoWeapon.Item.Name.ToString();

                    ShowQuiverName = true;
                }
            }
        }

        // different quiver than CurrentQuiver used to be
        if (hasChanged)
        {
            UpdateQuiverImages();
        }
    }

    [DataSourceProperty]
    public bool ShowQuiverAmmoCount
    {
        get
        {
            return _showQuiverAmmoCount;
        }
        set
        {
            if (value != _showQuiverAmmoCount)
            {
                _showQuiverAmmoCount = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int CurrentQuiverAmmo
    {
        get => _currentQuiverAmmo;
        set
        {
            if (value != _currentQuiverAmmo)
            {
                _currentQuiverAmmo = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsQuiverAmmoCountAlertEnabled
    {
        get => _isQuiverAmmoCountAlertEnabled;
        set
        {
            if (value != _isQuiverAmmoCountAlertEnabled)
            {
                _isQuiverAmmoCountAlertEnabled = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string QuiverName
    {
        get => _quiverName;
        set
        {
            if (value != _quiverName)
            {
                _quiverName = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool ShowQuiverName
    {
        get => _showQuiverName;
        set
        {
            if (value != _showQuiverName)
            {
                _showQuiverName = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public float QuiverAmmoCountMarginRight
    {
        get => _quiverAmmoCountMarginRight;
        set
        {
            if (value != _quiverAmmoCountMarginRight)
            {
                _quiverAmmoCountMarginRight = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public bool RangedWeaponEquipped
    {
        get => _rangedWeaponEquipped;
        set
        {
            if (value != _rangedWeaponEquipped)
            {
                _rangedWeaponEquipped = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public MissionWeapon WieldedWeapon
    {
        get => _wieldedWeapon;
        set
        {
            if (!_wieldedWeapon.IsEqualTo(value))
            {
                _wieldedWeapon = value;
                OnPropertyChanged(nameof(_wieldedWeapon));
            }
        }
    }

    public MissionWeapon CurrentQuiver
    {
        get => _currentQuiver;
        set
        {
            if (!_currentQuiver.IsEqualTo(value))
            {
                _currentQuiver = value;
                OnPropertyChanged(nameof(_currentQuiver));
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM QuiverImage0
    {
        get => _quiverImage0;
        set
        {
            if (value != _quiverImage0)
            {
                _quiverImage0 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value);
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM QuiverImage1
    {
        get => _quiverImage1;
        set
        {
            if (value != _quiverImage1)
            {
                _quiverImage1 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value);
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM QuiverImage2
    {
        get => _quiverImage2;
        set
        {
            if (value != _quiverImage2)
            {
                _quiverImage2 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value);
            }
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM QuiverImage3
    {
        get => _quiverImage3;
        set
        {
            if (value != _quiverImage3)
            {
                _quiverImage3 = value;
                OnPropertyChangedWithValue<ImageIdentifierVM>(value);
            }
        }
    }
}
