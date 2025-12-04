using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Intermission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.Intermission;


[GameStateScreen(typeof(LobbyGameStateCustomGameClient))]
[GameStateScreen(typeof(LobbyGameStateCommunityClient))]
public class CrpgIntermissionScreenUIHandler : ScreenBase, IGameStateListener, IChatLogHandlerScreen
{
    public GauntletLayer? Layer { get; private set; }

    public CrpgIntermissionScreenUIHandler(LobbyGameStateCustomGameClient gameState)
    {
        this.Construct();
    }

    public CrpgIntermissionScreenUIHandler(LobbyGameStateCommunityClient gameState)
    {
        this.Construct();
    }

    private void Construct()
    {
        SpriteData spriteData = UIResourceManager.SpriteData;
        TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
        ResourceDepot uiresourceDepot = UIResourceManager.ResourceDepot;
        this._customGameClientCategory = spriteData.SpriteCategories["ui_mpintermission"];
        this._customGameClientCategory.Load(resourceContext, uiresourceDepot);
        this._dataSource = new CrpgIntermissionVM();
        this.Layer = new GauntletLayer("CrpgIntermission", 100, false);
        this.Layer.IsFocusLayer = true;
        base.AddLayer(this.Layer);
        this.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
        this.Layer.LoadMovie("CrpgIntermission", this._dataSource);
    }

    protected override void OnFrameTick(float dt)
    {
        base.OnFrameTick(dt);
        this._dataSource?.Tick();
    }

    protected override void OnFinalize()
    {
        base.OnFinalize();
        this._customGameClientCategory.Unload();
        this.Layer?.InputRestrictions.ResetInputRestrictions();
        this.Layer = null;
        this._dataSource?.OnFinalize();
        this._dataSource = null;
    }

    void IGameStateListener.OnActivate()
    {
        this.Layer?.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        ScreenManager.TrySetFocus(this.Layer);
        LoadingWindow.EnableGlobalLoadingWindow();
    }

    void IGameStateListener.OnDeactivate()
    {
    }

    void IGameStateListener.OnInitialize()
    {
    }

    void IGameStateListener.OnFinalize()
    {
    }

    void IChatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref bool isToggleChatHintAvailable, ref bool isMouseVisible, ref InputContext inputContext)
    {
        if (this.Layer != null)
        {
            isTeamChatAvailable = false;
            inputEnabled = true;
            inputContext = this.Layer.Input;
        }
    }

    private CrpgIntermissionVM? _dataSource;

    private SpriteCategory _customGameClientCategory = default!;
}

