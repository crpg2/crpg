using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Common;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.GameServers;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Settings;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Persistence;

public class CrpgDbContext : DbContext, ICrpgDbContext
{
    private readonly IDateTime? _dateTime;

    public CrpgDbContext(DbContextOptions<CrpgDbContext> options)
        : base(options)
    {
    }

    public CrpgDbContext(
        DbContextOptions<CrpgDbContext> options,
        IDateTime dateTime)
        : base(options)
    {
        _dateTime = dateTime;
    }

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Character> Characters { get; set; } = default!;
    public DbSet<Item> Items { get; set; } = default!;
    public DbSet<UserItem> UserItems { get; set; } = default!;
    public DbSet<PersonalItem> PersonalItems { get; set; } = default!;
    public DbSet<EquippedItem> EquippedItems { get; set; } = default!;
    public DbSet<CharacterLimitations> CharacterLimitations { get; set; } = default!;
    public DbSet<Restriction> Restrictions { get; set; } = default!;
    public DbSet<Clan> Clans { get; set; } = default!;
    public DbSet<ClanMember> ClanMembers { get; set; } = default!;
    public DbSet<ClanArmoryItem> ClanArmoryItems { get; set; } = default!;
    public DbSet<ClanArmoryBorrowedItem> ClanArmoryBorrowedItems { get; set; } = default!;
    public DbSet<ClanInvitation> ClanInvitations { get; set; } = default!;
    public DbSet<Party> Parties { get; set; } = default!;
    public DbSet<Settlement> Settlements { get; set; } = default!;
    public DbSet<SettlementItem> SettlementItems { get; set; } = default!;
    public DbSet<PartyItem> PartyItems { get; set; } = default!;
    public DbSet<Battle> Battles { get; set; } = default!;
    public DbSet<BattleFighter> BattleFighters { get; set; } = default!;
    public DbSet<BattleFighterApplication> BattleFighterApplications { get; set; } = default!;
    public DbSet<BattleMercenary> BattleMercenaries { get; set; } = default!;
    public DbSet<BattleMercenaryApplication> BattleMercenaryApplications { get; set; } = default!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = default!;
    public DbSet<ActivityLogMetadata> ActivityLogMetadata { get; set; } = default!;
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; } = default!;
    public DbSet<Setting> Settings { get; set; } = default!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // don't override the value if it was already set. Useful for tests
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = _dateTime!.UtcNow;
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = _dateTime!.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // don't override the value if it was already set. Useful for tests
                if (!entry.Property(e => e.UpdatedAt).IsModified)
                {
                    entry.Entity.UpdatedAt = _dateTime!.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrpgDbContext).Assembly);
        // Ensure that the PostGIS extension is installed.
        modelBuilder.HasPostgresExtension("postgis");
    }
}
