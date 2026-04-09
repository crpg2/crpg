// =============================================================================
// Clan-exclusive item tests — covers the three critical paths.
// Drop these into the appropriate test project directories alongside the
// existing PersonalItem tests.
// =============================================================================

// ---- RewardClanItemCommandTest.cs -------------------------------------------
using Crpg.Application.Clans.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Commands;

public class RewardClanItemCommandTest : TestBase
{
    [Test]
    public async Task ReturnsErrorIfClanItemAlreadyAssigned()
    {
        var item = new Item { Id = "crpg_exclusive_helm_h1", Enabled = false };
        var user = new User { Platform = Platform.Steam, PlatformUserId = "1", Name = "Leader" };
        var clan = new Clan { Tag = "TST", Name = "Test Clan" };
        var member = new ClanMember { User = user, Clan = clan, Role = ClanMemberRole.Leader };
        var existingUserItem = new UserItem { User = user, Item = item, ClanItem = new ClanItem { Clan = clan } };

        ArrangeDb.Items.Add(item);
        ArrangeDb.Clans.Add(clan);
        ArrangeDb.ClanMembers.Add(member);
        ArrangeDb.UserItems.Add(existingUserItem);
        await ArrangeDb.SaveChangesAsync();

        var result = await Mediator.Send(new RewardClanItemCommand
        {
            ActorUserId = 99,
            ClanId = clan.Id,
            ItemId = item.Id,
        });

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ClanItemAlreadyExist));
    }

    [Test]
    public async Task ProvisionsCopyToEachCurrentMember()
    {
        var item = new Item { Id = "crpg_exclusive_helm_h1", Enabled = false };
        var clan = new Clan { Tag = "TST", Name = "Test Clan" };
        var u1 = new User { Platform = Platform.Steam, PlatformUserId = "1", Name = "Leader" };
        var u2 = new User { Platform = Platform.Steam, PlatformUserId = "2", Name = "Member" };
        clan.Members = new List<ClanMember>
        {
            new() { User = u1, Clan = clan, Role = ClanMemberRole.Leader },
            new() { User = u2, Clan = clan, Role = ClanMemberRole.Member },
        };

        ArrangeDb.Items.Add(item);
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        var result = await Mediator.Send(new RewardClanItemCommand
        {
            ActorUserId = 99,
            ClanId = clan.Id,
            ItemId = item.Id,
        });

        Assert.That(result.Errors, Is.Null);
        var provisioned = ActDb.UserItems
            .Where(ui => ui.ClanItem != null && ui.ClanItem.ClanId == clan.Id)
            .ToList();
        Assert.That(provisioned, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ClanItemIsRemovedOnLeaveClan()
    {
        var item = new Item { Id = "crpg_exclusive_helm_h1", Enabled = false };
        var clan = new Clan { Tag = "TST", Name = "Test Clan" };
        var leader = new User { Platform = Platform.Steam, PlatformUserId = "1", Name = "Leader" };
        var leaver = new User { Platform = Platform.Steam, PlatformUserId = "2", Name = "Leaver" };

        var leaderMember = new ClanMember { User = leader, Clan = clan, Role = ClanMemberRole.Leader };
        var leaverMember = new ClanMember { User = leaver, Clan = clan, Role = ClanMemberRole.Member };

        // Clan item already in leaver's inventory.
        var clanUserItem = new UserItem { User = leaver, Item = item, ClanItem = new ClanItem { Clan = clan } };

        ArrangeDb.Items.Add(item);
        ArrangeDb.Clans.Add(clan);
        ArrangeDb.ClanMembers.Add(leaderMember);
        ArrangeDb.ClanMembers.Add(leaverMember);
        ArrangeDb.UserItems.Add(clanUserItem);
        await ArrangeDb.SaveChangesAsync();

        // Leaver kicks themselves (same userId == kickedUserId triggers LeaveClan).
        var result = await Mediator.Send(new KickClanMemberCommand
        {
            UserId = leaver.Id,
            ClanId = clan.Id,
            KickedUserId = leaver.Id,
        });

        Assert.That(result.Errors, Is.Null);

        // The leaver's clan-exclusive UserItem and its ClanItem marker should be gone.
        var remaining = ActDb.UserItems
            .Where(ui => ui.UserId == leaver.Id && ui.ClanItem != null)
            .ToList();
        Assert.That(remaining, Is.Empty);
    }
}
