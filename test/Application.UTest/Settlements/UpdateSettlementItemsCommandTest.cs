using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class UpdateSettlementItemsCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotFound()
    {
        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = 1,
            SettlementId = 1,
            Items = [],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyNotInASettlement()
    {
        Party party = new() { Status = PartyStatus.Idle, User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = 1,
            Items = [],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
    }

    [Test]
    public async Task ShouldReturnErrorIfItemNotFound()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "non_existent_item", Count = 5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    // ============ SCENARIO 1: Party gives partial to empty settlement ============
    [Test]
    public async Task PartyGivesPartialToEmptySettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        PartyItem partyItem = new() { Item = sword, Count = 10, Party = party };
        ArrangeDb.PartyItems.Add(partyItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = 5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Has.Length.EqualTo(1));
        Assert.That(res.Data![0].Count, Is.EqualTo(5));

        var assertPartyItem = await AssertDb.PartyItems
            .FirstOrDefaultAsync(pi => pi.PartyId == party.Id && pi.ItemId == "sword");
        Assert.That(assertPartyItem, Is.Not.Null);
        Assert.That(assertPartyItem!.Count, Is.EqualTo(5));

        var assertSettlementItem = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "sword");
        Assert.That(assertSettlementItem, Is.Not.Null);
        Assert.That(assertSettlementItem!.Count, Is.EqualTo(5));
    }

    // ============ SCENARIO 2: Party gives all to empty settlement ============
    [Test]
    public async Task PartyGivesAllToEmptySettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        PartyItem partyItem = new() { Item = sword, Count = 5, Party = party };
        ArrangeDb.PartyItems.Add(partyItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = 5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);

        var assertPartyItem = await AssertDb.PartyItems
            .FirstOrDefaultAsync(pi => pi.PartyId == party.Id && pi.ItemId == "sword");
        Assert.That(assertPartyItem, Is.Null);

        var assertSettlementItem = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "sword");
        Assert.That(assertSettlementItem, Is.Not.Null);
        Assert.That(assertSettlementItem!.Count, Is.EqualTo(5));
    }

    // ============ SCENARIO 3: Party gives to existing settlement item ============
    [Test]
    public async Task PartyGivesToExistingSettlementItem()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        PartyItem partyItem = new() { Item = sword, Count = 10, Party = party };
        ArrangeDb.PartyItems.Add(partyItem);

        SettlementItem settlementItem = new() { Item = sword, Count = 10, Settlement = settlement };
        ArrangeDb.SettlementItems.Add(settlementItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = 5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data![0].Count, Is.EqualTo(15));

        var assertPartyItem = await AssertDb.PartyItems
            .FirstOrDefaultAsync(pi => pi.PartyId == party.Id && pi.ItemId == "sword");
        Assert.That(assertPartyItem!.Count, Is.EqualTo(5));

        var assertSettlementItem = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "sword");
        Assert.That(assertSettlementItem!.Count, Is.EqualTo(15));
    }

    // ============ SCENARIO 5: Party doesn't have enough items ============
    [Test]
    public async Task PartyDoesntHaveEnoughItems()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        PartyItem partyItem = new() { Item = sword, Count = 3, Party = party };
        ArrangeDb.PartyItems.Add(partyItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = 5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotEnoughItems));
    }

    // ============ SCENARIO 7: Party takes from settlement ============
    [Test]
    public async Task PartyTakesFromSettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        settlement.Owner = party;
        ArrangeDb.Parties.Add(party);

        SettlementItem settlementItem = new() { Item = sword, Count = 10, Settlement = settlement };
        ArrangeDb.SettlementItems.Add(settlementItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = -5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data![0].Count, Is.EqualTo(5));

        var assertPartyItem = await AssertDb.PartyItems
            .FirstOrDefaultAsync(pi => pi.PartyId == party.Id && pi.ItemId == "sword");
        Assert.That(assertPartyItem, Is.Not.Null);
        Assert.That(assertPartyItem!.Count, Is.EqualTo(5));

        var assertSettlementItem = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "sword");
        Assert.That(assertSettlementItem!.Count, Is.EqualTo(5));
    }

    // ============ SCENARIO 9: Party takes all from settlement ============
    [Test]
    public async Task PartyTakesAllFromSettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        settlement.Owner = party;
        ArrangeDb.Parties.Add(party);

        SettlementItem settlementItem = new() { Item = sword, Count = 5, Settlement = settlement };
        ArrangeDb.SettlementItems.Add(settlementItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = -5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Has.Length.EqualTo(0));

        var assertPartyItem = await AssertDb.PartyItems
            .FirstOrDefaultAsync(pi => pi.PartyId == party.Id && pi.ItemId == "sword");
        Assert.That(assertPartyItem, Is.Not.Null);
        Assert.That(assertPartyItem!.Count, Is.EqualTo(5));

        var assertSettlementItem = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "sword");
        Assert.That(assertSettlementItem, Is.Null);
    }

    // ============ SCENARIO 11: Settlement doesn't have enough items ============
    [Test]
    public async Task SettlementDoesntHaveEnoughItems()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        settlement.Owner = party;
        ArrangeDb.Parties.Add(party);

        SettlementItem settlementItem = new() { Item = sword, Count = 3, Settlement = settlement };
        ArrangeDb.SettlementItems.Add(settlementItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = -5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.SettlementNotEnoughItems));
    }

    // ============ SCENARIO 13: Party doesn't own settlement ============
    [Test]
    public async Task PartyDoesntOwnSettlement()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        ArrangeDb.Items.Add(sword);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        SettlementItem settlementItem = new() { Item = sword, Count = 10, Settlement = settlement };
        ArrangeDb.SettlementItems.Add(settlementItem);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [new ItemStackUpdate { ItemId = "sword", Count = -5 }],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotSettlementOwner));
    }

    // ============ Multiple items in one transfer ============
    [Test]
    public async Task ShouldHandleMultipleItemsInOneTransfer()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Item sword = new() { Id = "sword" };
        Item shield = new() { Id = "shield" };
        ArrangeDb.Items.AddRange(sword, shield);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        PartyItem partyItem1 = new() { Item = sword, Count = 10, Party = party };
        PartyItem partyItem2 = new() { Item = shield, Count = 8, Party = party };
        ArrangeDb.PartyItems.AddRange(partyItem1, partyItem2);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items =
            [
                new ItemStackUpdate { ItemId = "sword", Count = 5 },
                new ItemStackUpdate { ItemId = "shield", Count = 3 },
            ],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Has.Length.EqualTo(2));

        var swordInSettlement = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "sword");
        Assert.That(swordInSettlement!.Count, Is.EqualTo(5));

        var shieldInSettlement = await AssertDb.SettlementItems
            .FirstOrDefaultAsync(si => si.SettlementId == settlement.Id && si.ItemId == "shield");
        Assert.That(shieldInSettlement!.Count, Is.EqualTo(3));
    }

    // ============ Empty items list ============
    [Test]
    public async Task ShouldReturnEmptyResultForEmptyItemsList()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new UpdateSettlementItemsCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Items = [],
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Has.Length.EqualTo(0));
    }
}
