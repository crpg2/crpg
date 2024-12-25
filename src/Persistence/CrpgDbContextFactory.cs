using Crpg.Domain.Entities;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.GameServers;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Crpg.Persistence;

/// <summary>
/// Provides options for "dotnet ef" tool.
/// </summary>
public class CrpgDbContextFactory : IDesignTimeDbContextFactory<CrpgDbContext>
{
    public CrpgDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CrpgDbContext>()
            .UseNpgsql("Host=localhost;Database=crpg;Username=postgres;Password=root",
                options =>
                    options
                        .UseNetTopologySuite()
                        // enums
                        .MapEnum<Platform>()
                        .MapEnum<Role>()
                        .MapEnum<CharacterClass>()
                        .MapEnum<RestrictionType>()
                        .MapEnum<Culture>()
                        .MapEnum<ItemType>()
                        .MapEnum<ItemSlot>()
                        .MapEnum<DamageType>()
                        .MapEnum<WeaponClass>()
                        .MapEnum<ClanMemberRole>()
                        .MapEnum<ClanInvitationType>()
                        .MapEnum<ClanInvitationStatus>()
                        .MapEnum<PartyStatus>()
                        .MapEnum<SettlementType>()
                        .MapEnum<BattlePhase>()
                        .MapEnum<BattleSide>()
                        .MapEnum<BattleFighterApplicationStatus>()
                        .MapEnum<BattleMercenaryApplicationStatus>()
                        .MapEnum<Region>()
                        .MapEnum<Languages>()
                        .MapEnum<GameMode>()
                        .MapEnum<ActivityLogType>()
                        .MapEnum<UserUpdateStatus>())
            .UseSnakeCaseNamingConvention()
            .Options;
        return new CrpgDbContext(options);
    }
}
