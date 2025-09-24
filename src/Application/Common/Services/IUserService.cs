using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Common logic for characters.
/// </summary>
internal interface IUserService
{
    void SetDefaultValuesForUser(User user);

    Task<bool> CheckIsRecentUser(ICrpgDbContext db, User user);
}

/// <inheritdoc />
internal class UserService : IUserService
{
    private readonly IDateTime _dateTime;
    private readonly Constants _constants;

    public UserService(IDateTime dateTime, Constants constants)
    {
        _dateTime = dateTime;
        _constants = constants;
    }

    public void SetDefaultValuesForUser(User user)
    {
        user.Gold = user.CreatedAt == default || user.CreatedAt + TimeSpan.FromDays(30) < _dateTime.UtcNow
            ? _constants.DefaultGold
            : Math.Min(_constants.DefaultGold, user.Gold);
        user.Role = Role.User;
        user.HeirloomPoints = _constants.DefaultHeirloomPoints;
        user.ExperienceMultiplier = _constants.DefaultExperienceMultiplier;
    }

    public async Task<bool> CheckIsRecentUser(ICrpgDbContext db, User user)
    {
        var characters = await db.Characters
            .Where(c => c.UserId == user.Id)
            .ToArrayAsync();

        bool hasHighLevelCharacter = characters.Any(c => c.Level > _constants.NewUserStartingCharacterLevel);
        double totalExperience = characters.Sum(c => c.Experience);
        bool wasRetired = user.ExperienceMultiplier > _constants.DefaultExperienceMultiplier;
        return
            !wasRetired &&
            !hasHighLevelCharacter &&
            totalExperience < 12_000_000; // protection against abusers of free re-specialization mechanics
    }
}
