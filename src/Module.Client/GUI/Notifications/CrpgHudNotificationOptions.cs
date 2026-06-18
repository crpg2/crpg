using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Notifications;

public sealed class CrpgHudNotificationOptions
{
    public string Text { get; set; } = string.Empty;

    // Screen anchor and pixel offset from that anchor (scaled units).
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
    public float PositionX { get; set; } = 0f;
    public float PositionY { get; set; } = 0f;

    public float Duration { get; set; } = 5f;

    // Text styling.
    public int FontSize { get; set; } = 20;
    public Color Color { get; set; } = Color.White;

    // Font name as registered in FontFactory. Empty string falls back to FontFactory.DefaultFont.
    // Examples: "Blizzard Regular", "FiraSansExtraCondensed-Regular", "FiraSansExtraCondensed-Medium"
    public string FontName { get; set; } = string.Empty;

    // Optional image shown to the left of the text.
    // Set to the Gauntlet sprite path, e.g. "General\\Icons\\my_icon".
    // Leave null for text-only.
    public string? SpriteName { get; set; } = null;
    public float ImageWidth { get; set; } = 48f;
    public float ImageHeight { get; set; } = 48f;
    public Color ImageColor { get; set; } = Color.White;

    // Optional additional sprite names placed to the right of SpriteName, all sharing the same
    // ImageWidth / ImageHeight / ImageColor. Use this to show 2+ icons side by side in one entry.
    public List<string>? ExtraSprites { get; set; } = null;
}
