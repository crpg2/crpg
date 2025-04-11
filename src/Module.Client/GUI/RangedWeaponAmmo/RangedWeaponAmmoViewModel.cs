using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
namespace Crpg.Module.GUI;

internal class RangedWeaponAmmoViewModel : ViewModel
{
    // private MissionMultiplayerGameModeBaseClient _gameMode;
    private readonly Mission _mission;
    private bool _showQuiverAmmoCount;
    private int _quiverAmmoCount;
    private bool _isQuiverAmmoCountAlertEnabled;
    private string _quiverName;
    private bool _showQuiverName;

    public RangedWeaponAmmoViewModel(Mission mission)
    {
        _mission = mission;
        _quiverName = string.Empty;
        _quiverAmmoCount = 0;
        _isQuiverAmmoCountAlertEnabled = false;
        _showQuiverAmmoCount = false;
        _showQuiverName = false;
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
        bool isPlayerActive = _mission?.MainAgent?.IsActive() == true;

        if (isPlayerActive)
        {
            UpdateWeaponStatuses();
        }
        else
        {
            ShowQuiverAmmoCount = false;
            ShowQuiverName = false;
        }
    }

    public bool RequestChangeRangedAmmo()
    {
        Agent agent = Agent.Main;

        // Check if agent is wielding a weapon that uses quiver. bow xbow or musket
        if (agent == null || !agent.IsActive() || !AmmoQuiverChangeComponent.IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon mWeaponWielded))
        {
            return false;
        }

        // check agent quivers
        if (!AmmoQuiverChangeComponent.GetAgentQuiversWithAmmoEquippedForWieldedWeapon(agent, out List<int> ammoQuivers))
        {
            return false;
        }

        // multiple quivers with ammo found
        if (ammoQuivers.Count() > 1)
        {
            // InformationManager.DisplayMessage(new InformationMessage("RequestChangeRangedAmmo() Quivers Found: " + ammoQuivers.Count()));
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new ClientRequestAmmoQuiverChange());
            GameNetwork.EndModuleEventAsClient();
            return true;
        }
        else
        {
            // InformationManager.DisplayMessage(new InformationMessage("RequestChangeRangedAmmo() No Additional Quivers Found: "));
        }

        return false;
    }

    public bool GetCurrentQuiverAmmoAmount(out int currentQuiverAmmo, out int maxQuiverAmmo, out MissionWeapon ammoWeapon)
    {
        Agent agent = Agent.Main;
        ItemObject.ItemTypeEnum currentAmmoType = ItemObject.ItemTypeEnum.Invalid;
        currentQuiverAmmo = 0;
        maxQuiverAmmo = 0;
        ammoWeapon = MissionWeapon.Invalid;

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        // Get current weapon and type, figure out ammo type
        if (agent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand).IsRangedWeapon)
        {
            MissionWeapon mWeaponWielded = agent.Equipment[agent.GetWieldedItemIndex(Agent.HandIndex.MainHand)];
            if (!mWeaponWielded.IsEmpty)
            {
                currentAmmoType = ItemObject.GetAmmoTypeForItemType(mWeaponWielded.Item.Type);
                // set for handguns
                if (mWeaponWielded.Item.Type == ItemObject.ItemTypeEnum.Musket)
                {
                    currentAmmoType = ItemObject.ItemTypeEnum.Bullets;
                }
            }

            // cycle through equipment and find first of ammo type
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.ExtraWeaponSlot; ++equipmentIndex)
            {
                MissionWeapon mWeaponAmmo = agent.Equipment[equipmentIndex];
                if (!mWeaponAmmo.IsEmpty) // go to next ammo if its empty
                {
                    if (mWeaponAmmo.Item.Type == currentAmmoType) // matches the current ammo type needed
                    {
                        if (mWeaponAmmo.Amount <= 0) // no ammo left in quiver
                        {
                            continue;
                        }

                        QuiverAmmoLeft = mWeaponAmmo.Amount;
                        currentQuiverAmmo = mWeaponAmmo.Amount;
                        maxQuiverAmmo = mWeaponAmmo.MaxAmmo;
                        ammoWeapon = mWeaponAmmo;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void UpdateWeaponStatuses()
    {
        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive())
        {
            ShowQuiverAmmoCount = false;
            ShowQuiverName = false;
            return;
        }

        // Show Ammo count textwidget if there are multiple quivers and its not empty
        if (!GetCurrentQuiverAmmoAmount(out int currentQuiverAmmo, out int maxQuiverAmmo, out MissionWeapon ammoWeapon) || currentQuiverAmmo <= 0)
        {
            ShowQuiverAmmoCount = false;
            ShowQuiverName = false;
            return;
        }

        ShowQuiverAmmoCount = false;

        // check for this being the last quiver with ammo
        if (AmmoQuiverChangeComponent.GetAgentQuiversWithAmmoEquippedForWieldedWeapon(agent, out List<int> ammoQuivers))
        {
            // hide quiver ammo if its the only quiver or thrown weapon
            bool isThrown = ammoWeapon.Item.ItemType == ItemObject.ItemTypeEnum.Thrown;
            if (ammoQuivers.Count() > 1 || isThrown)
            {
                ShowQuiverAmmoCount = true;
                float f = (float)maxQuiverAmmo * 0.2f;
                IsQuiverAmmoCountAlertEnabled = maxQuiverAmmo != QuiverAmmoLeft && QuiverAmmoLeft <= MathF.Ceiling(f);
            }
        }

        // display quiver name, still display name if thrown but no ammo amount
        if (!ammoWeapon.IsEqualTo(MissionWeapon.Invalid))
        {
            // check if the ammo weapon is the wielded weapon ie throwing
            EquipmentIndex wieldedWeaponIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
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
    public int QuiverAmmoLeft
    {
        get => _quiverAmmoCount;
        set
        {
            if (value != _quiverAmmoCount)
            {
                _quiverAmmoCount = value;
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
}
