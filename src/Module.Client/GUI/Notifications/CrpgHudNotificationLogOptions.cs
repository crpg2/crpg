using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace Crpg.Module.GUI.Notifications;

public sealed class CrpgHudNotificationLogOptions
{
    // Screen anchor and pixel offset from that anchor (scaled units).
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
    public float PositionX { get; set; } = 0f;
    public float PositionY { get; set; } = 0f;

    // How many entries can be visible at once before the oldest is cycled out.
    public int MaxEntries { get; set; } = 5;

    // true  = VerticalBottomToTop: status-log style, newest entry at top, list grows upward.
    // false = VerticalTopToBottom: standard top-down list.
    public bool GrowUpward { get; set; } = true;

    // Horizontal alignment applied to each entry widget within the log panel.
    // Useful when entries have varying widths — e.g. Right keeps icons pinned to the right
    // edge even when a wider text entry above them forces the panel wider.
    public HorizontalAlignment ContentHorizontalAlignment { get; set; } = HorizontalAlignment.Left;
}
