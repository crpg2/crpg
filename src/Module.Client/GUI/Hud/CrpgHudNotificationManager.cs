using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Hud;

public class CrpgHudNotificationManager
{
    private static CrpgHudNotificationManager? _instance;
    public static CrpgHudNotificationManager Instance => _instance ??= new CrpgHudNotificationManager();

    private HudTextNotificationRootWidget? _rootWidget;

    private CrpgHudNotificationManager()
    {
    }

    // Initialize directly with a specific movie
    public void Initialize(IGauntletMovie movie, GauntletLayer? layer = null)
    {
        if (_rootWidget != null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgHudNotificationManager already initialized."));
            return;
        }

        if (layer is null)
        {
            throw new InvalidOperationException("CrpgHudNotificationManager: GauntletLayer must be provided to get UIContext");
        }

        UIContext context = layer.UIContext;
        Widget root = movie.RootWidget;

        _rootWidget = new HudTextNotificationRootWidget(context);
        root.AddChild(_rootWidget);
    }

    public bool IsInitialized => _rootWidget != null;

    public void Update(float dt)
    {
        _rootWidget?.Update(dt);
    }

    public HudTextNotificationWidget? AddRight(string text, string style = "Default", float lifetime = 10f)
    {
        if (_rootWidget == null)
        {
            throw new InvalidOperationException("CrpgHudNotificationManager must be initialized before use.");
        }

        return _rootWidget?.AddRightNotificationWidget(text, style, lifetime);
    }

    public HudTextNotificationWidget? AddLeft(string text, string style = "Default", float lifetime = 10f)
    {
        if (_rootWidget == null)
        {
            throw new InvalidOperationException("CrpgHudNotificationManager must be initialized before use.");
        }

        return _rootWidget?.AddLeftNotificationWidget(text, style, lifetime);
    }

    public void Remove(HudTextNotificationWidget widget)
    {
        _rootWidget?.RemoveNotificationWidget(widget);
    }

    public void Cleanup()
    {
        _rootWidget = null;
    }

    // Helper to get UIContext and root widget from the latest loaded movie in the layer
    private static (UIContext context, Widget rootWidget) GetMovieContextAndRoot(GauntletLayer layer)
    {
        if (layer.MoviesAndDataSources.Count == 0)
        {
            throw new InvalidOperationException("No movies loaded in GauntletLayer");
        }

        var movie = layer.MoviesAndDataSources.Last().Item1;
        UIContext context = layer.UIContext; // public field
        Widget rootWidget = movie.RootWidget; // public property

        return (context, rootWidget);
    }
}
