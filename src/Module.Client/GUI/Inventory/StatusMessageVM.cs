using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class StatusMessageVM : ViewModel
{
    private float _timer;

    public StatusMessageVM(string message, bool isError = false, float duration = 5f)
    {
        Message = message;
        IsError = isError;
        _timer = duration;
    }

    [DataSourceProperty]
    public string Message { get; set => SetField(ref field, value, nameof(Message)); } = string.Empty;

    [DataSourceProperty]
    public bool IsExpired { get; set => SetField(ref field, value, nameof(IsExpired)); }

    [DataSourceProperty]
    public bool IsError { get; set => SetField(ref field, value, nameof(IsError)); }

    internal void Tick(float dt)
    {
        _timer -= dt;
        if (_timer <= 0)
        {
            IsExpired = true;
        }
    }
}
