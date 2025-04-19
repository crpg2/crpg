using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

internal class RangedWeaponAmmoMissionView : MissionView
{
    private RangedWeaponAmmoViewModel _viewModel;
    private AmmoQuiverChangeMissionBehavior? _weaponChangeBehavior;
    private GauntletLayer? _gauntletLayer;
    public RangedWeaponAmmoMissionView()
    {
        _viewModel = new RangedWeaponAmmoViewModel(Mission); // Guaranteed non-null
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        _viewModel = new RangedWeaponAmmoViewModel(Mission);
        _weaponChangeBehavior = Mission.GetMissionBehavior<AmmoQuiverChangeMissionBehavior>();

        // Subscribe to mission behavior events
        _weaponChangeBehavior.OnMissileShot -= OnMissileShot;
        _weaponChangeBehavior.OnMissileShot += OnMissileShot;

        _weaponChangeBehavior.WieldedItemChanged -= OnWieldedItemChanged;
        _weaponChangeBehavior.WieldedItemChanged += OnWieldedItemChanged;

        _weaponChangeBehavior.OnItemDrop -= OnItemDrop;
        _weaponChangeBehavior.OnItemDrop += OnItemDrop;

        _weaponChangeBehavior.OnItemPickUp -= OnItemPickUp;
        _weaponChangeBehavior.OnItemPickUp += OnItemPickUp;

        _weaponChangeBehavior.OnAmmoQuiverChanged -= OnAmmoQuiverChanged;
        _weaponChangeBehavior.OnAmmoQuiverChanged += OnAmmoQuiverChanged;

        // Initialize Gauntlet UI layer
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("RangedWeaponAmmoHud", _viewModel);
        MissionScreen.AddLayer(_gauntletLayer);
        base.OnMissionScreenInitialize();
    }

    public override void OnMissionScreenFinalize()
    {
        if (_weaponChangeBehavior != null)
        {
            _weaponChangeBehavior.OnMissileShot -= OnMissileShot;
            _weaponChangeBehavior.WieldedItemChanged -= OnWieldedItemChanged;
            _weaponChangeBehavior.OnItemDrop -= OnItemDrop;
            _weaponChangeBehavior.OnItemDrop -= OnItemPickUp;
            _weaponChangeBehavior.OnAmmoQuiverChanged -= OnAmmoQuiverChanged;
        }

        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        _viewModel!.OnFinalize();
        _viewModel = null!;
        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        /*
                if (Input.IsGameKeyPressed(HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey("ToggleWeaponMode").Id)) // default is X
                {
                    _weaponChangeBehavior?.RequestChangeRangedAmmo();
                }
        */
        if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.C)) // C for now
        {
            _weaponChangeBehavior?.RequestChangeRangedAmmo();
        }

        _viewModel!.Tick(dt);
    }

    private void OnMissileShot(Agent shooterAgent, EquipmentIndex weaponIndex)
    {
        // _viewModel?.OnMissileShot(shooterAgent, weaponIndex);
        _viewModel?.UpdateWeaponStatuses();
        _viewModel?.UpdateQuiverImages();
    }

    private void OnWieldedItemChanged(EquipmentIndex newWeaponIndex, MissionWeapon missionWeapon)
    {
        _viewModel?.OnAgentWieldedWeaponChanged(newWeaponIndex, missionWeapon);
    }

    private void OnItemDrop(Agent agent, SpawnedItemEntity spawnedItem)
    {
        _viewModel?.UpdateWeaponStatuses();
        _viewModel?.UpdateQuiverImages();
    }

    private void OnItemPickUp(Agent agent, SpawnedItemEntity spawnedItem)
    {
        _viewModel?.UpdateWeaponStatuses();
        _viewModel?.UpdateQuiverImages();
    }

    private void OnAmmoQuiverChanged(Agent agent)
    {
        _viewModel?.UpdateWeaponStatuses();
        _viewModel?.UpdateQuiverImages();
    }
}
