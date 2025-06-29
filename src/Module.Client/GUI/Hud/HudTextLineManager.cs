using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;

public class HudTextLineManager
{
    public MBBindingList<HudTextLineVm> Lines { get; } = new();

    public HudTextLineVm AddLine(string text, string style = "Default", float lifetime = 10f)
    {
        var line = new HudTextLineVm(text, style, lifetime);
        Lines.Add(line);
        return line;
    }

    public HudTextLineVm AddLine(TextObject gameText, string style = "Default", float lifetime = 10f)
    {
        var line = new HudTextLineVm(gameText, style, lifetime);
        Lines.Add(line);
        return line;
    }

    public void RemoveLine(HudTextLineVm line)
    {
        Lines.Remove(line);
    }

    public void ClearAll()
    {
        Lines.Clear();
    }

    public void Update(float dt)
    {
        float now = (float)MissionTime.Now.ToSeconds;
        for (int i = Lines.Count - 1; i >= 0; i--)
        {
            if (Lines[i].IsExpired(now))
            {
                Lines.RemoveAt(i);
            }
        }
    }
}
