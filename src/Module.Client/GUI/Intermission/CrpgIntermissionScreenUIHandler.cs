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
        Construct();
    }

    public CrpgIntermissionScreenUIHandler(LobbyGameStateCommunityClient gameState)
    {
        Construct();
    }

    private void Construct()
    {
        SpriteData spriteData = UIResourceManager.SpriteData;
        TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
        ResourceDepot uiresourceDepot = UIResourceManager.ResourceDepot;
        _customGameClientCategory = spriteData.SpriteCategories["ui_mpintermission"];
        _customGameClientCategory.Load(resourceContext, uiresourceDepot);
        _dataSource = new CrpgIntermissionVM();
        Layer = new GauntletLayer("CrpgIntermission", 100);
        Layer.IsFocusLayer = true;
        AddLayer(Layer);
        Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
        Layer.LoadMovie("CrpgIntermission", _dataSource);
    }

    protected override void OnFrameTick(float dt)
    {
        base.OnFrameTick(dt);
        _dataSource?.Tick();
    }

    protected override void OnFinalize()
    {
        base.OnFinalize();
        _customGameClientCategory.Unload();
        Layer?.InputRestrictions.ResetInputRestrictions();
        Layer = null;
        _dataSource?.OnFinalize();
        _dataSource = null;
    }

    void IGameStateListener.OnActivate()
    {
        Layer?.InputRestrictions.SetInputRestrictions();
        ScreenManager.TrySetFocus(Layer);
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
        if (Layer != null)
        {
            isTeamChatAvailable = false;
            inputEnabled = true;
            inputContext = Layer.Input;
        }
    }

    private CrpgIntermissionVM? _dataSource;

    private SpriteCategory _customGameClientCategory = null!;
}

