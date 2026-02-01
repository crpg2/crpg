using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.GameServers;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crpg.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services,
        IConfiguration configuration,
        IApplicationEnvironment appEnv)
    {
        string? connectionString = configuration.GetConnectionString("Crpg");
        if (appEnv.Environment == HostingEnvironment.Development && connectionString == null)
        {
            services.AddDbContext<CrpgDbContext>(options =>
            {
                options.UseInMemoryDatabase("crpg");
            });
        }
        else
        {
            services.AddDbContext<CrpgDbContext>(options =>
            {
                options
                    .UseNpgsql(connectionString,
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
                                .MapEnum<PartyOrderType>()
                                .MapEnum<PartyTransferOfferStatus>()
                                .MapEnum<SettlementType>()
                                .MapEnum<BattlePhase>()
                                .MapEnum<BattleSide>()
                                .MapEnum<BattleParticipantType>()
                                .MapEnum<BattleFighterApplicationStatus>()
                                .MapEnum<BattleMercenaryApplicationStatus>()
                                .MapEnum<Region>()
                                .MapEnum<Languages>()
                                .MapEnum<GameMode>()
                                .MapEnum<ActivityLogType>()
                                .MapEnum<NotificationState>()
                                .MapEnum<NotificationType>()
                                .MapEnum<TerrainType>()
                                .MapEnum<UserUpdateStatus>())
                    .UseSnakeCaseNamingConvention();

                // TODO: FIXME: https://github.com/dotnet/efcore/issues/35110
                options.ConfigureWarnings(warnings =>
                {
                    warnings.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
                });

                if (appEnv.Environment == HostingEnvironment.Development)
                {
                    options
                        .EnableSensitiveDataLogging()
                        .EnableDetailedErrors();
                }
            });
        }

        services.AddScoped<ICrpgDbContext>(provider => provider.GetRequiredService<CrpgDbContext>());

        return services;
    }
}
