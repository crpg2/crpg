using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.UTest.Clans.Armory;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ClanServiceTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };

    [Test]
    public async Task GetClanMemberShouldReturnErrorIfUserNotFound()
    {
        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanMember(ActDb, 1, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task GetClanMemberShouldReturnErrorIfUserIsNotInAClan()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanMember(ActDb, user.Id, 2, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotInAClan));
    }

    [Test]
    public async Task GetClanMemberShouldReturnErrorIfUserIsNotAClanMember()
    {
        User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanMember(ActDb, user.Id, 3, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotAClanMember));
    }

    [Test]
    public async Task GetClanMemberShouldNotReturnErrorIfUserIsAClanMember()
    {
        User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanMember(ActDb, user.Id, user.ClanMembership.ClanId, CancellationToken.None);
        Assert.That(res.Errors, Is.Null);
    }

    [Test]
    public async Task GetClanLeaderShouldReturnErrorIfClanHasNotALeader()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User user = new() { ClanMembership = new() { Clan = clan } };
        ArrangeDb.Users.Add(user);

        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanLeader(ActDb, clan.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanLeaderFound));
    }

    [Test]
    public async Task GetClanLeaderShouldNotReturnErrorIfClanHasALeader()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User user = new() { ClanMembership = new() { Clan = clan, Role = ClanMemberRole.Leader } };
        ArrangeDb.Users.Add(user);

        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanLeader(ActDb, clan.Id, CancellationToken.None);
        Assert.That(res.Errors, Is.Null);
    }

    [Test]
    public async Task GetClanOfficersShouldNotEmptyIfClanHasSomeOfficer()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        User user1 = new() { ClanMembership = new() { Clan = clan, Role = ClanMemberRole.Leader } };
        User user2 = new() { ClanMembership = new() { Clan = clan, Role = ClanMemberRole.Officer } };
        User user3 = new() { ClanMembership = new() { Clan = clan, Role = ClanMemberRole.Member } };

        ArrangeDb.Users.Add(user1);
        ArrangeDb.Users.Add(user2);
        ArrangeDb.Users.Add(user3);

        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var res = await clanService.GetClanOfficers(ActDb, clan.Id, CancellationToken.None);
        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.Count, Is.EqualTo(2));
        Assert.That(res.Errors, Is.Null);
    }

    [Test]
    public async Task JoinClanShouldDeleteInvitationRequestsAndDeclineInvitationOffers()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        ClanInvitation[] invitations =
        {
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Pending,
            },
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = new User(),
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Declined,
            },
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = user,
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Pending,
            },
            new()
            {
                Clan = new Clan(),
                Invitee = user,
                Inviter = new User(),
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Accepted,
            },
        };
        ArrangeDb.ClanInvitations.AddRange(invitations);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var u = await ActDb.Users.FirstAsync(u => u.Id == user.Id);
        var res = await clanService.JoinClan(ActDb, u, clan.Id, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Null);
        Assert.That(AssertDb.ClanInvitations, Has.Exactly(2)
            .Matches<ClanInvitation>(ci => ci.Type == ClanInvitationType.Offer && ci.Status == ClanInvitationStatus.Declined));
        Assert.That(AssertDb.ClanInvitations, Has.Exactly(0)
            .Matches<ClanInvitation>(ci => ci.Type == ClanInvitationType.Request && ci.Status == ClanInvitationStatus.Pending));
        Assert.That(AssertDb.ClanInvitations, Has.Exactly(1)
            .Matches<ClanInvitation>(ci => ci.Type == ClanInvitationType.Request && ci.Status == ClanInvitationStatus.Accepted));
    }

    [Test]
    public async Task LeaveClanShouldReturnErrorIfMemberLeaderAndNotLastMember()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new()
        {
            Members =
            {
                new ClanMember { User = user, Role = ClanMemberRole.Leader },
                new ClanMember { User = new User(), Role = ClanMemberRole.Member },
            },
        };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
        var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanNeedLeader));
    }

    [Test]
    public async Task LeaveClanShouldLeaveClanIfMemberLeaderButLastMember()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new() { Members = { new ClanMember { User = user, Role = ClanMemberRole.Leader } } };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
        var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Null);
        Assert.That(AssertDb.Clans, Has.Exactly(0).Matches<Clan>(c => c.Id == clan.Id));
        Assert.That(AssertDb.ClanMembers, Has.Exactly(0).Matches<ClanMember>(cm => cm.UserId == user.Id));
    }

    [Test]
    public async Task LeaveClanShouldWork()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        Clan clan = new() { Members = { new ClanMember { User = user, Role = ClanMemberRole.Member } } };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var member = await ActDb.ClanMembers.FirstAsync(cm => cm.UserId == user.Id);
        var res = await clanService.LeaveClan(ActDb, member, CancellationToken.None);
        await ActDb.SaveChangesAsync();

        Assert.That(res.Errors, Is.Null);
        Assert.That(AssertDb.Clans, Has.Exactly(1).Matches<Clan>(c => c.Id == clan.Id));
        Assert.That(AssertDb.ClanMembers, Has.Exactly(0).Matches<ClanMember>(cm => cm.UserId == user.Id));
    }

    [Test]
    public async Task AddArmoryItemShouldWork()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .Include(u => u.Items)
            .Where(u => u.Name == "user0")
            .FirstAsync();

        var clan = await ActDb.Clans
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var item = user.Items.First();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var result = await clanService.AddArmoryItem(ActDb, clan, user, item.Id);
        Assert.That(result.Errors, Is.Null.Or.Empty);
        Assert.That(result.Data, Is.Not.Null);

        await ActDb.SaveChangesAsync();

        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Where(u => u.Id == user.Id)
            .FirstAsync();
        Assert.That(user.ClanMembership!.ArmoryItems.Count, Is.EqualTo(1));
        Assert.That(user.ClanMembership.ArmoryBorrowedItems.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveArmoryItemShouldWork()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");

        var user = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items).ThenInclude(ui => ui.ClanArmoryItem)
            .Where(u => u.Name == "user0")
            .FirstAsync();

        var clan = await ActDb.Clans
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var item = user.Items.First(ui => ui.ClanArmoryItem != null);

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var result = await clanService.RemoveArmoryItem(ActDb, clan, user, item.Id);
        Assert.That(result.Errors, Is.Null.Or.Empty);

        await ActDb.SaveChangesAsync();

        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(0));

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Where(u => u.Id == user.Id)
            .FirstAsync();
        Assert.That(user.ClanMembership!.ArmoryItems.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task BorrowArmoryItemShouldWork()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");

        var user = await ActDb.Users
            .Include(u => u.ClanMembership)
            .Where(u => u.Name == "user1")
            .FirstAsync();

        var clan = await ActDb.Clans
            .Include(c => c.Members).ThenInclude(cm => cm.ArmoryItems)
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var item = clan.Members.First(cm => cm.ArmoryItems.Count > 0).ArmoryItems.First();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var result = await clanService.BorrowArmoryItem(ActDb, clan, user, item.UserItemId);
        Assert.That(result.Errors, Is.Null.Or.Empty);

        await ActDb.SaveChangesAsync();

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(1));
        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Where(u => u.Id == user.Id)
            .FirstAsync();
        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ReturnArmoryItemShouldWork()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(1));
        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));

        var user = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items)
            .Where(u => u.Name == "user1")
            .FirstAsync();

        var clan = await ActDb.Clans
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var item = user.ClanMembership!.ArmoryBorrowedItems.First();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var result = await clanService.ReturnArmoryItem(ActDb, clan, user, item.UserItemId);
        Assert.That(result.Errors, Is.Null.Or.Empty);

        await ActDb.SaveChangesAsync();

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(0));
        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Where(u => u.Id == user.Id)
            .FirstAsync();
        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
        Assert.That(user.ClanMembership.ArmoryItems.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ReturnArmoryItemByNotBorrowerUserShouldNotWork()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(1));
        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));

        var user1 = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items)
            .Where(u => u.Name == "user1")
            .FirstAsync();

        var user2 = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items)
            .Where(u => u.Name == "user2")
            .FirstAsync();

        var clan = await ActDb.Clans
            .Where(c => c.Id == user1.ClanMembership!.ClanId)
            .FirstAsync();

        var item = user1.ClanMembership!.ArmoryBorrowedItems.First();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var result = await clanService.ReturnArmoryItem(ActDb, clan, user2, item.UserItemId);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task ReturnArmoryItemByClanLeaderShouldWork()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0");
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user1");

        var user = await ActDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Include(u => u.Items)
            .Where(u => u.Name == "user1")
            .FirstAsync();

        var clan = await ActDb.Clans
            .Where(c => c.Id == user.ClanMembership!.ClanId)
            .FirstAsync();

        var clanLeader = new User
        {
            Name = "clanLeader",
            ClanMembership = new() { Clan = clan, Role = ClanMemberRole.Leader },
        };
        ActDb.Users.Add(clanLeader);
        await ActDb.SaveChangesAsync();

        var item = user.ClanMembership!.ArmoryBorrowedItems.First();

        ClanService clanService = new(ActivityLogService.Object, UserNotificationService.Object);
        var result = await clanService.ReturnArmoryItem(ActDb, clan, clanLeader, item.UserItemId);

        Assert.That(result.Errors, Is.Null.Or.Empty);

        await ActDb.SaveChangesAsync();

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(0));
        Assert.That(AssertDb.ClanArmoryItems.Count(), Is.EqualTo(1));

        user = await AssertDb.Users
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryItems)
            .Include(u => u.ClanMembership!).ThenInclude(cm => cm.ArmoryBorrowedItems)
            .Where(u => u.Id == user.Id)
            .FirstAsync();
        Assert.That(user.ClanMembership!.ArmoryBorrowedItems.Count, Is.EqualTo(0));
        Assert.That(user.ClanMembership.ArmoryItems.Count, Is.EqualTo(0));
    }
}
