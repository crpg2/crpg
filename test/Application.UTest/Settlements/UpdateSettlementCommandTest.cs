using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Commands;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements;

public class UpdateSettlementCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        StrategusMinPartyTroops = 1,
    };

    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotFound()
    {
        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = 1,
            SettlementId = 2,
            Troops = 0,
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

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = 1,
            Troops = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyNotInTheSpecifiedSettlement()
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

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = 99,
            Troops = 0,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsGivingTroopTheyDontHave()
    {
        Settlement settlement = new();
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 5,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 6,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotEnoughTroops));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsTakingTroopsFromANotOwnedSettlement()
    {
        Settlement settlement = new() { Troops = 10 };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 5,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 5,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotSettlementOwner));
    }

    [Test]
    public async Task ShouldGiveTroopsToSettlement()
    {
        Settlement settlement = new()
        {
            Troops = 20,
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.RecruitingInSettlement,
            Troops = 11,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 30,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var settlementVm = res.Data!;
        Assert.That(settlementVm.Id, Is.EqualTo(settlement.Id));
        Assert.That(AssertDb.Settlements.Find(settlement.Id)!.Troops, Is.EqualTo(30));
    }

    [Test]
    public async Task ShouldTakeTroopsFromSettlement()
    {
        Settlement settlement = new()
        {
            Troops = 20,
        };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 10,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        settlement.Owner = party;
        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 10,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var settlementVm = res.Data!;
        Assert.That(settlementVm.Id, Is.EqualTo(settlement.Id));
        Assert.That(AssertDb.Parties.Find(party.Id)!.Troops, Is.EqualTo(20));
        Assert.That(AssertDb.Settlements.Find(settlement.Id)!.Troops, Is.EqualTo(10));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyDoesntHaveEnoughTroopsToKeepMinimum()
    {
        Settlement settlement = new() { Troops = 10 };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 5,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 15,  // Would require giving 5 troops but party needs to keep minimum 1
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotEnoughTroops));
    }

    [Test]
    public async Task ShouldReturnErrorIfSettlementDoesntHaveEnoughTroopsToKeepMinimum()
    {
        Settlement settlement = new() { Troops = 5 };
        ArrangeDb.Settlements.Add(settlement);

        Party party = new()
        {
            Status = PartyStatus.IdleInSettlement,
            Troops = 10,
            CurrentSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);

        settlement.Owner = party;
        await ArrangeDb.SaveChangesAsync();

        UpdateSettlementCommand.Handler handler = new(ActDb, Mapper, Constants);
        var res = await handler.Handle(new UpdateSettlementCommand
        {
            PartyId = party.Id,
            SettlementId = settlement.Id,
            Troops = 0,  // Would require taking 5 troops but settlement needs to keep minimum 1
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.SettlementNotEnoughTroops));
    }
}
