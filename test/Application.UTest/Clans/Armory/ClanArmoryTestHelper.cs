using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;
public static class ClanArmoryTestHelper
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IClanService ClanService = new ClanService(ActivityLogService.Object, UserNotificationService.Object);

    public static async Task CommonSetUp(ICrpgDbContext db, int usersCount = 4, int itemsPerUser = 4, int armoryTimeout = 3)
    {
        var items = Enumerable.Range(0, usersCount * itemsPerUser).Select(idx => new Item
        {
            Id = $"{idx}",
            Name = $"item{idx}",
            Enabled = true,
        }).ToList();
        var clan = new Clan { ArmoryTimeout = TimeSpan.FromDays(armoryTimeout) };

        var users = Enumerable.Range(0, usersCount).Select(idx => new User
        {
            Name = $"user{idx}",
            ClanMembership = new() { Clan = clan },
            Characters = Enumerable.Range(0, 2).Select(i => new Character()).ToList(),
            Items = items.GetRange(idx * itemsPerUser, itemsPerUser).Select(i => new UserItem { Item = i }).ToList(),
        });

        db.Users.AddRange(users);
        db.Items.AddRange(items);
        db.Clans.AddRange(clan);
        await db.SaveChangesAsync();

        Assert.That(db.Users.Count(), Is.EqualTo(users.Count()));
        Assert.That(db.Items.Count(), Is.EqualTo(items.Count()));
        Assert.That(db.Clans.Count(), Is.EqualTo(1));
        Assert.That(db.ClanMembers.Count(), Is.EqualTo(users.Count()));
        Assert.That(db.UserItems.Count(), Is.EqualTo(users.Count() * itemsPerUser));
    }

    public static async Task<IList<ClanArmoryItem>> AddItems(ICrpgDbContext db, string userName, int count = 1)
    {
        var user = await db.Users
            .Include(u => u.Items)
            .Include(u => u.ClanMembership)
            .Where(u => u.Name == userName)
            .FirstAsync();

        var clan = await db.Clans
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var list = new List<ClanArmoryItem>();
        foreach (var item in user.Items.Take(count))
        {
            var result = await ClanService.AddArmoryItem(db, clan, user, item.Id);
            Assert.That(result.Errors, Is.Null.Or.Empty);
            await db.SaveChangesAsync();

            list.Add(result.Data!);
        }

        return list;
    }

    public static async Task<IList<ClanArmoryBorrowedItem>> BorrowItems(ICrpgDbContext db, string userName, int count = 1)
    {
        var user = await db.Users
            .Include(u => u.Items)
            .Include(u => u.ClanMembership)
            .Where(u => u.Name == userName)
            .FirstAsync();

        var clan = await db.Clans
            .Include(c => c.Members).ThenInclude(cm => cm.ArmoryItems)
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var items = clan.Members
            .SelectMany(cm => cm.ArmoryItems)
            .Where(ci => ci.BorrowedItem == null)
            .Take(count);
        Assert.That(items.Count, Is.GreaterThanOrEqualTo(count));

        var list = new List<ClanArmoryBorrowedItem>();
        foreach (var item in items)
        {
            var result = await ClanService.BorrowArmoryItem(db, clan, user, item.UserItemId);
            Assert.That(result.Errors, Is.Null.Or.Empty);
            await db.SaveChangesAsync();

            list.Add(result.Data!);
        }

        return list;
    }
}
