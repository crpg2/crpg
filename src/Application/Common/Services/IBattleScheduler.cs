using Crpg.Application.ActivityLogs.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Sdk.Abstractions;

namespace Crpg.Application.Common.Services;

internal interface IBattleScheduler
{
    Task ScheduleBattle(Battle battle);
    DateTime GetNextBattleDateFromHour(int hour);
}

internal class BattleScheduler : IBattleScheduler
{
    private readonly IRandom _random;
    private readonly IDateTime _dateTime;

    public BattleScheduler(IRandom random, IDateTime dateTime)
    {
        _random = random;
        _dateTime = dateTime;
    }

    public Task ScheduleBattle(Battle battle)
    {
        if (battle.ScheduledFor != null)
        {
            return Task.CompletedTask; // already scheduled
        }

        var defender = battle.Fighters.First(f => f.Side == BattleSide.Defender && f.Commander); // TODO

        if (defender == null)
        {
            return Task.CompletedTask; // error
        }

        int attackTime;
        if (defender.Party != null)
        {
            var times = defender.Party.VulnerabilityWindow.Get(battle.Region).Hours;
            attackTime = times[_random.Next(0, times.Count)];
        }
        else if (defender.Settlement?.Owner != null)
        {
            var times = defender.Settlement.Owner.VulnerabilityWindow.Get(battle.Region).Hours;
            attackTime = times[_random.Next(0, times.Count)];
        }
        else
        {
            return Task.CompletedTask; // error
        }

        battle.ScheduledFor = GetNextBattleDateFromHour(attackTime);

        return Task.CompletedTask;
    }

    public DateTime GetNextBattleDateFromHour(int hour)
    {
        if (hour < 0 || hour > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");
        }

        DateTime baseTime = _dateTime.UtcNow.Date.AddHours(hour);
        if (baseTime <= _dateTime.UtcNow || (baseTime - _dateTime.UtcNow).TotalHours < 12) // TODO: minimum schedule window based on battle size
        {
            baseTime.AddDays(1);
        }

        return baseTime;
    }
}
