using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;

public class HudTextNotificationManager
{
    private static HudTextNotificationManager? _instance;
    public static HudTextNotificationManager Instance => _instance ??= new HudTextNotificationManager();

    public static void Cleanup()
    {
        _instance = null;
    }

    public MBBindingList<HudTextLineVm> LeftLines { get; } = new();
    public MBBindingList<HudTextLineVm> RightLines { get; } = new();

    private HudTextNotificationManager()
    {
    }

    public HudTextLineVm AddLeft(string text, string style = "Default", float lifetime = 10f)
    {
        var line = new HudTextLineVm(text, style, lifetime);
        RightLines.Add(line);
        return line;
    }

    public HudTextLineVm AddLeft(TextObject textObj, string style = "Default", float lifetime = 10f)
    {
        var line = new HudTextLineVm(textObj, style, lifetime);
        LeftLines.Add(line);
        return line;
    }

    public HudTextLineVm AddRight(string text, string style = "Default", float lifetime = 10f)
    {
        var line = new HudTextLineVm(text, style, lifetime);
        RightLines.Add(line);
        return line;
    }

    public HudTextLineVm AddRight(TextObject textObj, string style = "Default", float lifetime = 10f)
    {
        var line = new HudTextLineVm(textObj, style, lifetime);
        RightLines.Add(line);
        return line;
    }

    public void ClearAll()
    {
        LeftLines.Clear();
        RightLines.Clear();
    }

    public void ClearLeft()
    {
        LeftLines.Clear();
    }

    public void ClearRight()
    {
        RightLines.Clear();
    }

    public void Update(float dt)
    {
        float now = (float)MissionTime.Now.ToSeconds;
        for (int i = LeftLines.Count - 1; i >= 0; i--)
        {
            if (LeftLines[i].IsExpired(now))
            {
                LeftLines.RemoveAt(i);
            }
        }

        for (int i = RightLines.Count - 1; i >= 0; i--)
        {
            if (RightLines[i].IsExpired(now))
            {
                RightLines.RemoveAt(i);
            }
        }
    }
}
