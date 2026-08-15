using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Crpg.Module.GUI.Notifications;

/// <summary>
/// Full-screen container widget. Manages two kinds of on-screen notifications:
///   1. Floating notifications — transient widgets placed at an absolute position.
///   2. Log panels — positioned ListPanels that accumulate and cycle entries.
///
/// Loaded into the scene via CrpgHudNotificationContainer.xml.
/// </summary>
internal class CrpgHudNotificationWidget : Widget
{
    private readonly List<(Widget container, float remainingTime)> _notifications = [];

    internal static CrpgHudNotificationWidget? Current { get; private set; }

    public CrpgHudNotificationWidget(UIContext context)
        : base(context)
    {
        Current = this;
        WidthSizePolicy = SizePolicy.StretchToParent;
        HeightSizePolicy = SizePolicy.StretchToParent;
        DoNotAcceptEvents = true;
    }

    /// <summary>
    /// Shows a transient notification at a fixed screen position.
    /// The widget is removed automatically when Duration elapses.
    /// </summary>
    internal void AddNotification(CrpgHudNotificationOptions options)
    {
        var entryWidget = CrpgNotificationWidgetFactory.BuildEntry(Context, options);
        entryWidget.HorizontalAlignment = options.HorizontalAlignment;
        entryWidget.VerticalAlignment = options.VerticalAlignment;
        entryWidget.ScaledPositionXOffset = options.PositionX;
        entryWidget.ScaledPositionYOffset = options.PositionY;

        AddChild(entryWidget);
        _notifications.Add((entryWidget, options.Duration));
    }

    /// <summary>
    /// Creates a log panel as a child of this widget and returns it.
    /// Hold the reference to call AddEntry() on it later.
    /// </summary>
    internal CrpgHudNotificationLog CreateLog(CrpgHudNotificationLogOptions options)
    {
        var log = new CrpgHudNotificationLog(Context, options);
        AddChild(log);
        return log;
    }

    protected override void OnDisconnectedFromRoot()
    {
        base.OnDisconnectedFromRoot();
        if (Current == this)
        {
            Current = null;
        }
    }

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);
        for (int i = _notifications.Count - 1; i >= 0; i--)
        {
            var (container, remaining) = _notifications[i];
            float newRemaining = remaining - dt;
            if (newRemaining <= 0f)
            {
                RemoveChild(container);
                _notifications.RemoveAt(i);
            }
            else
            {
                _notifications[i] = (container, newRemaining);
            }
        }
    }
}
