using Crpg.Module.Common.AmmoQuiverChange;
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.AmmoQuiverChange;

internal class AmmoQuiverChangeUiHandler : MissionView, IUseKeyBinder
{
    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;
    private AmmoQuiverChangeVm _dataSource;
    private AmmoQuiverChangeBehaviorClient? _weaponChangeBehavior;
    private GauntletLayer? _gauntletLayer;

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_change_quiver",
                Name = "Change Quiver",
                Description = "Change ammo quiver",
                DefaultInputKey = InputKey.C,
            },
        },
    };

    private GameKey? quiverChangeKey;

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
        _gauntletLayer = new GauntletLayer("RangedWeaponAmmoHud", ViewOrderPriority);
        _gauntletLayer.LoadMovie("RangedWeaponAmmoHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        base.OnMissionScreenInitialize();
    }

    public override void EarlyStart()
    {
        // TODO: fix
         quiverChangeKey = HotKeyManager.GetCategory(KeyCategoryId).RegisteredGameKeys.Find(gk => gk != null && gk.StringId == "key_change_quiver");
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

        if (quiverChangeKey != null && (Input.IsKeyPressed(quiverChangeKey.KeyboardKey.InputKey) || Input.IsKeyPressed(quiverChangeKey.ControllerKey.InputKey)))
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
