using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Notifications;

/// <summary>
/// Shared factory for building notification entry widgets from options.
/// Used by both CrpgHudNotificationWidget (floating) and CrpgHudNotificationLog (log panel).
/// </summary>
internal static class CrpgNotificationWidgetFactory
{
    /// <summary>
    /// Builds the appropriate entry widget for the given options:
    ///   - image + text  -> Widget containing both, text offset by ImageWidth
    ///   - image only    -> Widget with Sprite
    ///   - text only     -> RichTextWidget.
    /// </summary>
    internal static Widget BuildEntry(UIContext context, CrpgHudNotificationOptions options)
    {
        bool hasImage = !string.IsNullOrEmpty(options.SpriteName);
        bool hasText = !string.IsNullOrEmpty(options.Text);
        bool hasExtraSprites = options.ExtraSprites?.Count > 0;

        // Multiple sprites (or sprite + text) — horizontal container using MarginLeft offsets.
        if (hasImage && (hasExtraSprites || hasText))
        {
            var container = new Widget(context)
            {
                WidthSizePolicy = SizePolicy.CoverChildren,
                HeightSizePolicy = SizePolicy.CoverChildren,
                DoNotAcceptEvents = true,
            };

            var firstImage = BuildImage(context, options);
            container.AddChild(firstImage);

            float xOffset = options.ImageWidth + 8f;

            if (hasExtraSprites)
            {
                foreach (string spriteName in options.ExtraSprites!)
                {
                    var extraOptions = new CrpgHudNotificationOptions
                    {
                        SpriteName = spriteName,
                        ImageWidth = options.ImageWidth,
                        ImageHeight = options.ImageHeight,
                        ImageColor = options.ImageColor,
                    };
                    var extraImage = BuildImage(context, extraOptions);
                    extraImage.MarginLeft = xOffset;
                    container.AddChild(extraImage);
                    xOffset += options.ImageWidth + 8f;
                }
            }

            if (hasText)
            {
                var text = BuildText(context, options);
                text.MarginLeft = xOffset;
                container.AddChild(text);
            }

            return container;
        }

        if (hasImage)
        {
            return BuildImage(context, options);
        }

        return BuildText(context, options);
    }

    internal static Widget BuildImage(UIContext context, CrpgHudNotificationOptions options)
    {
        var widget = new Widget(context)
        {
            WidthSizePolicy = SizePolicy.Fixed,
            HeightSizePolicy = SizePolicy.Fixed,
            SuggestedWidth = options.ImageWidth,
            SuggestedHeight = options.ImageHeight,
            VerticalAlignment = VerticalAlignment.Center,
            DoNotAcceptEvents = true,
            Color = options.ImageColor,
            // NOTE: if SpriteData.GetSprite doesn't compile, check UIContext for the sprite accessor.
            Sprite = context.SpriteData.GetSprite(options.SpriteName!),
        };
        return widget;
    }

    internal static RichTextWidget BuildText(UIContext context, CrpgHudNotificationOptions options)
    {
        var widget = new RichTextWidget(context)
        {
            WidthSizePolicy = SizePolicy.CoverChildren,
            HeightSizePolicy = SizePolicy.CoverChildren,
            VerticalAlignment = VerticalAlignment.Center,
            DoNotAcceptEvents = true,
            Text = options.Text,
            Brush = BuildBrush(context, options.Color, options.FontSize, options.FontName),
        };
        return widget;
    }

    private static Brush BuildBrush(UIContext context, Color color, int fontSize, string fontName)
    {
        var brush = new Brush
        {
            Font = (!string.IsNullOrEmpty(fontName) ? context.FontFactory.GetFont(fontName) : null)
                         ?? context.FontFactory.DefaultFont,
            FontColor = color,
            FontSize = fontSize,
        };
        return brush;
    }
}
