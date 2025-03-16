namespace Crpg.GameServerManager.GameServers;
public class GameServerSchedule
{
    public List<SchedulePeriod>? TurnOffTimes { get; set; }
    public List<SchedulePeriod>? TurnOnTimes { get; set; }

    public bool ShouldTurnOff()
    {
        DateTime now = DateTime.UtcNow;
        string currentDay = now.DayOfWeek.ToString();
        string currentTime = now.ToString("HH:mm");

        if (TurnOffTimes != null && TurnOffTimes.Any(period => period.IsWithinSchedule(currentDay, currentTime)))
        {
            return true;
        }

        if (TurnOnTimes != null)
        {
            bool isInOnPeriod = TurnOnTimes.Any(period => period.IsWithinSchedule(currentDay, currentTime));
            return !isInOnPeriod;
        }

        return false;
    }

    public bool ShouldTurnOn()
    {
        DateTime now = DateTime.UtcNow;
        string currentDay = now.DayOfWeek.ToString();
        string currentTime = now.ToString("HH:mm");

        if (TurnOnTimes != null && TurnOnTimes.Any(period => period.IsWithinSchedule(currentDay, currentTime)))
        {
            return true;
        }

        if (TurnOffTimes != null)
        {
            bool isInOffPeriod = TurnOffTimes.Any(period => period.IsWithinSchedule(currentDay, currentTime));
            return !isInOffPeriod;
        }
        else if (TurnOnTimes != null)
        {
            return false;
        }

        return true;
    }
}

public class SchedulePeriod
{
    public string Day { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    public bool IsWithinSchedule(string currentDay, string currentTime)
    {
        if (!Day.Equals(currentDay, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        TimeSpan current = TimeSpan.Parse(currentTime);
        TimeSpan start = TimeSpan.Parse(StartTime);
        TimeSpan end = TimeSpan.Parse(EndTime);

        return start <= current && current <= end;
    }
}
