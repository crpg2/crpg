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

    private bool _isFlashing = false;
    private float _flashFrequency = 4f; // flashes per second
    private float _flashElapsed = 0f;

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
        _isFlashing = false;
        _flashElapsed = 0f;
        AlphaFactor = 0f; // reset alpha
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

        // Flashing logic (overrides alpha smoothly)
        if (_isFlashing)
        {
            _flashElapsed += dt;
            float flashCycle = 1f / _flashFrequency;
            float flashAlpha = (MathF.Sin(_flashElapsed * flashCycle * 2 * MathF.PI) + 1f) / 2f; // oscillates 0 to 1

            // Combine base alpha with flash alpha
            AlphaFactor *= flashAlpha;
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

    // Toggle flashing on/off, frequency in flashes per second
    public void SetFlashing(bool flashing, float frequency = 4f)
    {
        _isFlashing = flashing;
        _flashFrequency = frequency;
        if (!flashing)
        {
            // Reset alpha if flashing stops
            AlphaFactor = 1f;
            _flashElapsed = 0f;
        }
    }

}
