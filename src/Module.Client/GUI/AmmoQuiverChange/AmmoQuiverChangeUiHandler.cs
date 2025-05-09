using Crpg.Module.Common.AmmoQuiverChange;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.AmmoQuiverChange;

internal class AmmoQuiverChangeUiHandler : MissionView
{
    private AmmoQuiverChangeVm _dataSource;
    private AmmoQuiverChangeBehaviorClient? _weaponChangeBehavior;
    private GauntletLayer? _gauntletLayer;
    public AmmoQuiverChangeUiHandler()
    {
        _dataSource = new AmmoQuiverChangeVm(Mission); // Guaranteed non-null
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        _dataSource = new AmmoQuiverChangeVm(Mission);
        _weaponChangeBehavior = Mission.GetMissionBehavior<AmmoQuiverChangeBehaviorClient>();

        if (_weaponChangeBehavior == null)
        {
            return;
        }

        // Subscribe to mission behavior events
        _weaponChangeBehavior.OnQuiverEvent += HandleQuiverEvent;

        // Initialize Gauntlet UI layer
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("RangedWeaponAmmoHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        base.OnMissionScreenInitialize();
    }

    public override void OnMissionScreenFinalize()
    {
        if (_weaponChangeBehavior != null)
        {
            _weaponChangeBehavior.OnQuiverEvent -= HandleQuiverEvent;
        }

        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        _dataSource!.OnFinalize();
        _dataSource = null!;
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
        if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.C)) // C for now, i would like to make it customizable in game menu but couldnt figure it out
        {
            _weaponChangeBehavior?.RequestChangeRangedAmmo();
        }

        _dataSource!.Tick(dt);
    }

    private void HandleQuiverEvent(AmmoQuiverChangeBehaviorClient.QuiverEventType type, object[] parameters)
    {
        if (type == AmmoQuiverChangeBehaviorClient.QuiverEventType.WieldedItemChanged && parameters.Length >= 2)
        {
            _dataSource?.UpdateWieldedWeapon((EquipmentIndex)parameters[0], (MissionWeapon)parameters[1]);
        }

        _dataSource?.UpdateWeaponStatuses();
    }
}
