using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

public class HudTextNotificationWidget : Widget
{
    private const float FadeInDuration = 0.5f;
    private const float FadeOutDuration = 1f;
    private readonly TextWidget _textWidget;
    private float _timeCreated;
    private float _lifetime = 10f;

    public bool IsExpired { get; private set; }

    public HudTextNotificationWidget(UIContext context)
        : base(context)
    {
        WidthSizePolicy = SizePolicy.CoverChildren;
        HeightSizePolicy = SizePolicy.CoverChildren;

        _textWidget = new TextWidget(context)
        {
            WidthSizePolicy = SizePolicy.CoverChildren,
            HeightSizePolicy = SizePolicy.CoverChildren,
            Brush = Context.GetBrush("CrpgHUD.NotificationText.Default"),
        };

        AddChild(_textWidget);
        AlphaFactor = 0f; // Start transparent for fade-in
    }

    public void Initialize(string text, string style = "Default", float lifetime = 10f)
    {
        _textWidget.Text = text;
        _textWidget.SetState(style);
        _timeCreated = (float)MissionTime.Now.ToSeconds;
        _lifetime = lifetime;
        IsExpired = false;
    }

    public void Tick(float dt)
    {
        float currentTime = (float)MissionTime.Now.ToSeconds;
        float elapsed = currentTime - _timeCreated;

        if (_lifetime > 0f)
        {
            float timeLeft = _lifetime - elapsed;

            if (timeLeft <= 0f)
            {
                IsExpired = true;
                return;
            }

            // Fade-in
            if (elapsed < FadeInDuration)
            {
                AlphaFactor = MathF.Clamp(elapsed / FadeInDuration, 0f, 1f);
            }

            // Fade-out
            else if (timeLeft < FadeOutDuration)
            {
                AlphaFactor = MathF.Clamp(timeLeft / FadeOutDuration, 0f, 1f);
            }

            // Fully visible
            else
            {
                AlphaFactor = 1f;
            }
        }
    }

    public void SetBrush(string brushName)
    {
        var brush = Context.GetBrush(brushName);
        if (brush != null)
        {
            _textWidget.Brush = brush;
        }
    }
}
