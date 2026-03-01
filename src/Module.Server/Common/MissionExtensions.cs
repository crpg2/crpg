using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal static class MissionExtensions
{
    public static T GetRequiredMissionBehavior<T>(this Mission mission) where T : class, IMissionBehavior
    {
        var behavior = mission.GetMissionBehavior<T>();
        if (behavior == null)
        {
            throw new ArgumentException($"{typeof(T).Name} was not found");
        }

        return behavior;
    }
}
