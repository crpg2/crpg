using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Hud;
// Need this dummy ViewModel to use code only HudTextNotificationRootWidget and system.
public class CrpgHudTextNotificationVm : ViewModel
{
    public CrpgHudTextNotificationVm(Mission mission)
    {
    }

    public void OnMissionScreenTick(float dt)
    {
    }

    public sealed override void RefreshValues()
    {
        base.RefreshValues();
    }
}
