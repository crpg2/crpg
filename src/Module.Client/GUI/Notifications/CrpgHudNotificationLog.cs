using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Crpg.Module.GUI.Notifications;

/// <summary>
/// A positioned ListPanel that accumulates notification entries and cycles them out
/// when they expire or the capacity limit is reached — similar to the status log.
///
/// Create via CrpgHudNotificationWidget.Current.CreateLog(...) and hold the reference.
/// Call AddEntry() to push new items. Call Destroy() to remove it from the screen.
///
/// Example:
///   var log = CrpgHudNotificationWidget.Current?.CreateLog(new CrpgHudNotificationLogOptions
///   {
///       PositionX  = 50f,
///       PositionY  = 600f,
///       MaxEntries = 6,
///   });
///   log?.AddEntry(new CrpgHudNotificationOptions { Text = "Player joined", Duration = 4f }).
/// </summary>
internal class CrpgHudNotificationLog : ListPanel
{
    private record struct LogEntry(Widget Widget, float RemainingTime);

    private readonly List<LogEntry> _entries = [];
    private readonly int _maxEntries;
    private readonly HorizontalAlignment _contentHorizontalAlignment;

    public CrpgHudNotificationLog(UIContext context, CrpgHudNotificationLogOptions options)
        : base(context)
    {
        WidthSizePolicy = SizePolicy.CoverChildren;
        HeightSizePolicy = SizePolicy.CoverChildren;
        HorizontalAlignment = options.HorizontalAlignment;
        VerticalAlignment = options.VerticalAlignment;
        ScaledPositionXOffset = options.PositionX;
        ScaledPositionYOffset = options.PositionY;
        DoNotAcceptEvents = true;
        _maxEntries = options.MaxEntries;
        _contentHorizontalAlignment = options.ContentHorizontalAlignment;
        SetLayoutMethod(options.GrowUpward ? "VerticalBottomToTop" : "VerticalTopToBottom");
    }

    /// <summary>
    /// Adds an entry to the log. If at capacity, the oldest entry is removed first.
    /// Returns the widget so it can be removed later via RemoveEntry.
    /// </summary>
    public Widget AddEntry(CrpgHudNotificationOptions options)
    {
        if (_entries.Count >= _maxEntries)
        {
            RemoveChild(_entries[0].Widget);
            _entries.RemoveAt(0);
        }

        var entryWidget = CrpgNotificationWidgetFactory.BuildEntry(Context, options);
        entryWidget.HorizontalAlignment = _contentHorizontalAlignment;
        AddChild(entryWidget);
        _entries.Add(new LogEntry(entryWidget, options.Duration));
        return entryWidget;
    }

    /// <summary>Removes a specific entry from the log by its widget reference.</summary>
    public void RemoveEntry(Widget widget)
    {
        int index = _entries.FindIndex(e => e.Widget == widget);
        if (index < 0)
        {
            return;
        }

        RemoveChild(_entries[index].Widget);
        _entries.RemoveAt(index);
    }

    /// <summary>Removes all current entries from the log without destroying the log itself.</summary>
    public void ClearEntries()
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            RemoveChild(_entries[i].Widget);
        }

        _entries.Clear();
    }

    /// <summary>Removes this log from the screen.</summary>
    public void Destroy() => ParentWidget?.RemoveChild(this);

    protected override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var entry = _entries[i];
            float newRemaining = entry.RemainingTime - dt;
            if (newRemaining <= 0f)
            {
                RemoveChild(entry.Widget);
                _entries.RemoveAt(i);
            }
            else
            {
                _entries[i] = entry with { RemainingTime = newRemaining };
            }
        }
    }

    // LayoutMethod's type lives in a TW assembly we can't reference by name, so set it via reflection.
    private void SetLayoutMethod(string methodName)
    {
        var prop = StackLayout?.GetType().GetProperty("LayoutMethod");
        if (prop == null)
        {
            return;
        }

        object value = Enum.Parse(prop.PropertyType, methodName);
        prop.SetValue(StackLayout, value);
    }
}
