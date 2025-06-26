using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;

public class HudTextLineVm : ViewModel
{
    private const float LifetimeDefault = 10f;
    private string _text;
    private TextObject? _gameText;
    private string _style = "Default"; // fallback style

    private float NormalizeLifetime(float? input) =>
    input.HasValue ? (input.Value < 0f ? -1f : input.Value) : LifetimeDefault;

    [DataSourceProperty]
    public string Text
    {
        get => _text;
        set => SetField(ref _text, value, nameof(Text));
    }

    [DataSourceProperty]
    public string Style
    {
        get => _style;
        set => SetField(ref _style, value, nameof(Style));
    }

    public float Lifetime { get; } // -1 means no expiration
    public float TimeCreated { get; }

    public bool IsExpired(float currentTime)
    {
        return Lifetime >= 0f && (currentTime - TimeCreated) >= Lifetime;
    }

    public HudTextLineVm(string text, string style = "Default", float lifetime = LifetimeDefault)
    {
        _text = text;
        _style = style;
        Lifetime = NormalizeLifetime(lifetime); // -1 means "no expiration"
        TimeCreated = (float)MissionTime.Now.ToSeconds;
    }

    public HudTextLineVm(TextObject gameText, string style = "Default", float lifetime = LifetimeDefault)
    {
        _gameText = gameText;
        _text = gameText.ToString();
        _style = style;
        Lifetime = NormalizeLifetime(lifetime); // -1 means "no expiration"
        TimeCreated = (float)MissionTime.Now.ToSeconds;
    }

    public void SetGameTextVariable(string key, string value)
    {
        if (_gameText != null)
        {
            _gameText.SetTextVariable(key, value);
            Text = _gameText.ToString(); // update visible text
        }
    }

    public void SetText(string newText)
    {
        _gameText = null;
        Text = newText;
    }
}
