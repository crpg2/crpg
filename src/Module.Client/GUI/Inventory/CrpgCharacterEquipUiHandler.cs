using Crpg.Module.Common;
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using Crpg.Module.Common.Network.Armory;
using Crpg.Module.GUI.Notifications;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace Crpg.Module.GUI.Inventory;

public class CrpgCharacterEquipUiHandler : MissionView, IUseKeyBinder
{
    private static readonly TimeSpan AutoRefreshCooldown = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ManualRefreshCooldown = TimeSpan.FromSeconds(10);
    internal static CrpgConstants Constants { get; private set; } = null!;
    internal bool IsCharacterEquipVisible => _characterEquipVm?.IsVisible ?? false;
    private DateTime _lastAutoRefreshTime = DateTime.MinValue;
    private DateTime _lastManualRefreshTime = DateTime.MinValue;
    private Action<object?>? _onVmCloseHandler;
    private Action<object?>? _onVmSelectedReadyHandler;
    private Action<object?>? _onVmCloseDestoryHandler;

    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;

    private CrpgCharacterLoadoutBehaviorClient? _userLoadout;
    private CrpgClanArmoryClient? _clanArmory;
    private MissionLobbyComponent? _lobbyComponent;
    private CrpgTeamInventoryClient? _teamInventory;
    private GauntletLayer? _characterEquipLayer;
    private CrpgCharacterEquipVM? _characterEquipVm;
    private GauntletMovieIdentifier? _characterEquipMovieId;

    private GameKey? _toggleEquipmentSelectKey;

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_toggle_equip_select",
                Name = new TextObject("{=KC9dx127}Open Character/Equipment Manager").ToString(),
                Description = new TextObject("{=KC9dx128}Open character/equipment manager gui").ToString(),
                DefaultInputKey = InputKey.I,
            },
        },
    };

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _onVmCloseHandler = OnVmCloseRequested;
        _onVmCloseDestoryHandler = OnVmCloseDestroyRequested;
        _onVmSelectedReadyHandler = OnVmSelectedReadyRequested;
        _teamInventory = Mission.Current.GetMissionBehavior<CrpgTeamInventoryClient>();
        _userLoadout = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        _clanArmory = Mission.Current.GetMissionBehavior<CrpgClanArmoryClient>();
        _lobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();

        if (_userLoadout != null)
        {
            _userLoadout.OnUserCharacterLoadoutEnabledChanged += HandleUserLoadoutEnabledChanged;
            _userLoadout.OnForceOpenEquipMenu += HandleForceEquipMenu;
        }

        if (_teamInventory != null)
        {
            _teamInventory.OnTeamInventoryEnabledChanged += HandleTeamInventoryEnabledChanged;
            _teamInventory.OnForceOpenEquipMenu += HandleForceEquipMenu;
        }

        if (_lobbyComponent != null)
        {
            _lobbyComponent.OnPostMatchEnded += HandlePostMatchEnded;
        }
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        if (_userLoadout != null)
        {
            _userLoadout.OnUserCharacterLoadoutEnabledChanged -= HandleUserLoadoutEnabledChanged;
            _userLoadout.OnForceOpenEquipMenu -= HandleForceEquipMenu;
        }

        if (_teamInventory != null)
        {
            _teamInventory.OnTeamInventoryEnabledChanged -= HandleTeamInventoryEnabledChanged;
            _teamInventory.OnForceOpenEquipMenu -= HandleForceEquipMenu;
        }

        if (_lobbyComponent != null)
        {
            _lobbyComponent.OnPostMatchEnded -= HandlePostMatchEnded;
        }

        HideCharacterEquip(destroy: true);
    }

    public override void EarlyStart()
    {
        base.EarlyStart();
        _toggleEquipmentSelectKey = HotKeyManager.GetCategory(KeyCategoryId).RegisteredGameKeys.Find(gk => gk != null && gk.StringId == "key_toggle_equip_select");
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        bool characterEquipOpen = _characterEquipVm?.IsVisible ?? false;

        bool escapePressed = false;
        if (_characterEquipLayer != null && _characterEquipLayer.IsFocusLayer)
        {
            escapePressed = _characterEquipLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") ||
                            _characterEquipLayer.Input.IsHotKeyReleased("Exit");
        }

        if (characterEquipOpen && escapePressed)
        {
            HideCharacterEquip();
            return;
        }

        if (_toggleEquipmentSelectKey != null && Input.IsKeyPressed(_toggleEquipmentSelectKey.KeyboardKey.InputKey))
        {
            ToggleCharacterEquip();
            return;
        }
    }

    // called from game mode
    internal static void Configure(CrpgConstants constants)
    {
        Constants = constants;
    }

    private void OnVmCloseRequested(object? sender) => HideCharacterEquip();
    private void OnVmCloseDestroyRequested(object? sender) => HideCharacterEquip(destroy: false); // was set to true for debug purposes to force rebuilding the UI

    private void OnVmSelectedReadyRequested(object? sender)
    {
        if (_teamInventory != null && _teamInventory.IsEnabled)
        {
            _teamInventory.RequestSpawnWithTeamEquipment();
            HideCharacterEquip();
        }
        else if (_userLoadout != null && _userLoadout.IsEnabled)
        {
            _userLoadout.RequestSetSpawnReady();
            _characterEquipVm?.IsSpawnButtonVisible = false;
            HideCharacterEquip();
        }
    }

    private void ToggleCharacterEquip()
    {
        if (_characterEquipVm == null || !_characterEquipVm.IsVisible)
        {
            OpenCharacterEquip();
        }
        else
        {
            HideCharacterEquip();
        }
    }

    private void OpenCharacterEquip()
    {
        if (_userLoadout == null || !_userLoadout.IsEnabled)
        {
            return;
        }

        if (_lobbyComponent?.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Ending)
        {
            return;
        }

        if (_teamInventory?.IsEnabled == true) // disable showing gui if teaminventory enabled and hasnt picked a side
        {
            if (GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team?.Side == TaleWorlds.Core.BattleSideEnum.None)
            {
                return;
            }
        }

        if (_characterEquipLayer == null)
        {
            _characterEquipVm = new CrpgCharacterEquipVM();
            _characterEquipVm.OnCloseButtonClicked += _onVmCloseHandler;
            _characterEquipVm.OnCloseButtonAlternateClicked += _onVmCloseDestoryHandler;
            _characterEquipVm.OnSelectedReadyClicked += _onVmSelectedReadyHandler;
            _characterEquipVm.InventoryGrid.OnInventoryRefreshRequestedClick += HandleInventoryRefreshRequested;

            _characterEquipLayer = new GauntletLayer("CrpgCharacterEquipPrefab", 110) { IsFocusLayer = true };
            _characterEquipLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _characterEquipLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _characterEquipMovieId = _characterEquipLayer.LoadMovie("CrpgCharacterEquipPrefab", _characterEquipVm);

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.AddLayer(_characterEquipLayer);
            }
            else
            {
                ScreenManager.TopScreen?.AddLayer(_characterEquipLayer);
            }
        }

        _characterEquipVm!.IsVisible = true;
        _characterEquipLayer.IsFocusLayer = true;
        _characterEquipLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        ScreenManager.TrySetFocus(_characterEquipLayer);
        SoundEvent.PlaySound2D("event:/ui/panels/panel_inventory_open");
        _characterEquipVm.RefreshSpawnButtonVisibility();

        var now = DateTime.UtcNow;
        if (now - _lastAutoRefreshTime >= AutoRefreshCooldown)
        {
            _lastAutoRefreshTime = now;
            _userLoadout?.RequestGetUserInfo();
            _userLoadout?.RequestGetUpdatedCharacterBasic();

            if (_teamInventory?.IsEnabled == true)
            {
                // _teamInventory.RequestGetTeamItemsList();
            }
            else
            {
                _userLoadout?.RequestGetUserInventoryItems();
                _userLoadout?.RequestGetUpdatedCharacterEquippedItems();
                _clanArmory?.RequestArmoryAction(ClanArmoryActionType.Get, null, force: true);
            }
        }
    }

    private void HideCharacterEquip(bool destroy = false)
    {
        if (_characterEquipLayer == null || _characterEquipVm == null)
        {
            return;
        }

        _characterEquipLayer.IsFocusLayer = false;
        _characterEquipLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_characterEquipLayer);

        if (destroy)
        {
            _characterEquipVm.OnCloseButtonClicked -= _onVmCloseHandler;
            _characterEquipVm.OnCloseButtonAlternateClicked -= _onVmCloseDestoryHandler;
            _characterEquipVm.OnSelectedReadyClicked -= _onVmSelectedReadyHandler;
            _characterEquipVm.InventoryGrid.OnInventoryRefreshRequestedClick -= HandleInventoryRefreshRequested;
            _characterEquipVm.OnFinalize();
            _characterEquipVm = null;

            _lastAutoRefreshTime = DateTime.MinValue;
            _lastManualRefreshTime = DateTime.MinValue;

            if (_characterEquipMovieId != null)
            {
                _characterEquipLayer.ReleaseMovie(_characterEquipMovieId);
                _characterEquipMovieId = null;
            }

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.RemoveLayer(_characterEquipLayer);
            }
            else
            {
                ScreenManager.TopScreen?.RemoveLayer(_characterEquipLayer);
            }

            _characterEquipLayer = null;
        }
        else
        {
            _characterEquipVm.IsVisible = false;
        }
    }

    private void HandleUserLoadoutEnabledChanged(bool isEnabled)
    {
        if (_characterEquipVm != null)
        {
            bool wasVisible = _characterEquipVm.IsVisible;
            HideCharacterEquip(destroy: true);
        }
    }

    private void HandleTeamInventoryEnabledChanged(bool isEnabled)
    {
        // destroy and recreate the VM with the new enabled state
        if (_characterEquipVm != null)
        {
            bool wasVisible = _characterEquipVm.IsVisible;
            HideCharacterEquip(destroy: true);
            if (wasVisible)
            {
                OpenCharacterEquip();
            }
        }
    }

    private void HandleForceEquipMenu(bool show, bool showReadyButton, string msg)
    {
        if (show)
        {
            if (_characterEquipVm?.IsVisible != true)
            {
                OpenCharacterEquip();
            }

            _characterEquipVm?.SelectNavBarTab(NavBarTab.Equipment);
            CrpgHudNotificationUiHandler.AddToLog("statuslog", new CrpgHudNotificationOptions { Text = msg, Color = Colors.Red, Duration = 10f });
            _characterEquipVm?.NotifyUnavailableLastEquippedItems();
        }
        else
        {
            HideCharacterEquip();
        }
    }

    private void HandleInventoryRefreshRequested(InventoryGridVM.InventorySection section)
    {
        var now = DateTime.UtcNow;
        if (now - _lastManualRefreshTime < ManualRefreshCooldown)
        {
            SoundEvent.PlaySound2D("event:/ui/notification/alert");
            CrpgHudNotificationUiHandler.AddToLog("statuslog", new CrpgHudNotificationOptions
            {
                Text = new TextObject("{=KC9dx118}Please wait to send another API request.").ToString(),
                Color = Colors.Red,
                Duration = 3f,
            });
            return;
        }

        _lastManualRefreshTime = now;

        string tabName = section switch
        {
            InventoryGridVM.InventorySection.UserInventory => new TextObject("{=KC9dx136}User").ToString(),
            InventoryGridVM.InventorySection.Armory => new TextObject("{=KC9dx137}Armory").ToString(),
            InventoryGridVM.InventorySection.TeamInventory => new TextObject("{=KC9dx138}Team").ToString(),
            _ => string.Empty,
        };

        CrpgHudNotificationUiHandler.AddToLog("statuslog", new CrpgHudNotificationOptions
        {
            Text = new TextObject("{=KC9dx244}Refreshing {TAB_NAME} Inventory...").SetTextVariable("TAB_NAME", tabName).ToString(),
            Color = Colors.Cyan,
            Duration = 3f,
        });

        switch (section)
        {
            case InventoryGridVM.InventorySection.UserInventory:
                _userLoadout?.RequestGetUserInventoryItems();
                _clanArmory?.RequestArmoryAction(ClanArmoryActionType.Get, null);
                break;
            case InventoryGridVM.InventorySection.Armory:
                _clanArmory?.RequestArmoryAction(ClanArmoryActionType.Get, null);
                break;
            case InventoryGridVM.InventorySection.TeamInventory:
                _teamInventory?.RequestGetTeamItemsList();
                break;
        }
    }

    private void HandlePostMatchEnded()
    {
        HideCharacterEquip(destroy: true);
    }
}
