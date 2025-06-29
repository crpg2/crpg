using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;

public class HudTextNotificationPanelVm : ViewModel
{
    [DataSourceProperty]
    public MBBindingList<HudTextLineVm> LeftLines => HudTextNotificationManager.Instance.LeftLines;
    [DataSourceProperty]
    public MBBindingList<HudTextLineVm> RightLines => HudTextNotificationManager.Instance.RightLines;

    public void Update(float dt) => HudTextNotificationManager.Instance.Update(dt);
}
