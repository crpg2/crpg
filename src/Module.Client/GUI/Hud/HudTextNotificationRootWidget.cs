using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.Canvas;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Chat;   // MissionGauntletChat
using TaleWorlds.MountAndBlade.View.MissionViews;    // MissionView

public class HudTextNotificationRootWidget : Widget
{
    private readonly ListPanel _leftPanel;
    private readonly ListPanel _rightPanel;
    private readonly Widget _customPanel;
    public HudTextNotificationRootWidget(UIContext context)
        : base(context)
    {
        WidthSizePolicy = SizePolicy.StretchToParent;
        HeightSizePolicy = SizePolicy.StretchToParent;

        // Left Panel
        _leftPanel = new ListPanel(context)
        {
            WidthSizePolicy = SizePolicy.CoverChildren,
            HeightSizePolicy = SizePolicy.CoverChildren,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            MarginLeft = 25f,
            MarginBottom = 340f,
        };

        _leftPanel.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;

        // Right Panel
        _rightPanel = new ListPanel(context)
        {
            WidthSizePolicy = SizePolicy.CoverChildren,
            HeightSizePolicy = SizePolicy.CoverChildren,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            MarginBottom = 190f,
            MarginRight = 15f,
        };

        _rightPanel.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;

        // Custom Panel
        _customPanel = new Widget(context)
        {
            WidthSizePolicy = SizePolicy.StretchToParent,
            HeightSizePolicy = SizePolicy.StretchToParent,
        };

        try
        {
            AddChild(_leftPanel);
            AddChild(_rightPanel);
            AddChild(_customPanel);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"Failed to initialize HudTextNotificationRootWidget: {ex.Message}"));
            Debug.Print($"HudTextNotificationRootWidget initialization error: {ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
    }

    public HudTextNotificationWidget AddRightNotificationWidget(string text, string style = "Default", float lifetime = 10f)
    {
        var notif = new HudTextNotificationWidget(Context);
        notif.Initialize(text, style, lifetime);
        notif.SetBrush("CrpgHUD.NotificationText.Right");
        _rightPanel.AddChild(notif);
        return notif;
    }

    public HudTextNotificationWidget AddLeftNotificationWidget(string text, string style = "Default", float lifetime = 10f)
    {
        var notif = new HudTextNotificationWidget(Context);
        notif.Initialize(text, style, lifetime);
        notif.SetBrush("CrpgHUD.NotificationText.Left");
        _leftPanel.AddChild(notif);
        return notif;
    }

    public HudTextNotificationWidget AddCustomNotificationWidget(string text, Vec2 position, string style = "Default", float lifetime = 10f)
    {
        var notif = new HudTextNotificationWidget(Context);
        notif.Initialize(text, style, lifetime);
        notif.SetBrush("CrpgHUD.NotificationText.Custom");

        notif.HorizontalAlignment = HorizontalAlignment.Left;
        notif.VerticalAlignment = VerticalAlignment.Top;
        notif.PositionXOffset = position.X;
        notif.PositionYOffset = position.Y;

        _customPanel.AddChild(notif);
        return notif;
    }

    public void RemoveNotificationWidget(HudTextNotificationWidget widget)
    {
        if (widget == null)
        {
            return;
        }

        if (_leftPanel.Children.Contains(widget))
        {
            _leftPanel.RemoveChild(widget);
        }
        else if (_rightPanel.Children.Contains(widget))
        {
            _rightPanel.RemoveChild(widget);
        }
        else if (_customPanel.Children.Contains(widget))
        {
            _customPanel.RemoveChild(widget);
        }
    }

    public void Update(float dt)
    {
        // Update and clean expired notifications from left panel
        for (int i = _leftPanel.ChildCount - 1; i >= 0; i--)
        {
            if (_leftPanel.GetChild(i) is HudTextNotificationWidget child)
            {
                child.Tick(dt);
                if (child.IsExpired)
                {
                    _leftPanel.RemoveChild(child);
                }
            }
        }

        // Update and clean expired notifications from right panel
        for (int i = _rightPanel.ChildCount - 1; i >= 0; i--)
        {
            if (_rightPanel.GetChild(i) is HudTextNotificationWidget child)
            {
                child.Tick(dt);
                if (child.IsExpired)
                {
                    _rightPanel.RemoveChild(child);
                }
            }
        }

        // Update and clean expired notifications from custom canvas
        for (int i = _customPanel.ChildCount - 1; i >= 0; i--)
        {
            if (_customPanel.GetChild(i) is HudTextNotificationWidget child)
            {
                child.Tick(dt);
                if (child.IsExpired)
                {
                    _customPanel.RemoveChild(child);
                }
            }
        }
    }
}
