using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class RespondToPartyTransferOfferCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyANotFound()
    {
        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = 99,
            TransferOfferId = 1,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfTransferOfferNotFound()
    {
        Party partyA = new() { User = new User() };
        ArrangeDb.Parties.Add(partyA);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = partyA.Id,
            TransferOfferId = 99,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.TransferOfferNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfOfferNotForThisParty()
    {
        Party partyA = new() { User = new User() };
        Party partyB = new() { User = new User() };
        Party partyC = new() { User = new User() };
        ArrangeDb.Parties.AddRange(partyA, partyB, partyC);

        PartyTransferOffer offer = new()
        {
            PartyId = partyB.Id,
            TargetPartyId = partyC.Id, // Offer is for partyC, not partyA
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = partyA.Id,
            TransferOfferId = offer.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.TransferOfferNotAllowed));
    }

    [Test]
    public async Task ShouldReturnErrorIfOfferNotPending()
    {
        Party partyA = new() { User = new User() };
        Party partyB = new() { User = new User() };
        ArrangeDb.Parties.AddRange(partyA, partyB);

        PartyTransferOffer offer = new()
        {
            PartyId = partyB.Id,
            TargetPartyId = partyA.Id,
            Status = PartyTransferOfferStatus.Intent, // Not Pending
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = partyA.Id,
            TransferOfferId = offer.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.TransferOfferInvalidStatus));
    }

    [Test]
    public async Task ShouldAcceptFullOffer()
    {
        Party respondingParty = new() { User = new User(), Gold = 100, Troops = 20 };
        Party offeringParty = new() { User = new User(), Gold = 200, Troops = 30 };
        ArrangeDb.Parties.AddRange(respondingParty, offeringParty);

        PartyTransferOffer offer = new()
        {
            PartyId = offeringParty.Id,
            TargetPartyId = respondingParty.Id,
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = respondingParty.Id,
            TransferOfferId = offer.Id,
            Accept = true,
            Accepted = new RespondToPartyTransferOfferCommand.TransferOffer
            {
                Gold = 100,
                Troops = 10,
                Items = [],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Value, Is.Not.Null);

        // Verify state changes
        var updatedRespondingParty = ActDb.Parties.Single(p => p.Id == respondingParty.Id);
        var updatedOfferingParty = ActDb.Parties.Single(p => p.Id == offeringParty.Id);

        Assert.That(updatedRespondingParty.Gold, Is.EqualTo(200)); // 100 + 100
        Assert.That(updatedRespondingParty.Troops, Is.EqualTo(30)); // 20 + 10
        Assert.That(updatedOfferingParty.Gold, Is.EqualTo(100)); // 200 - 100
        Assert.That(updatedOfferingParty.Troops, Is.EqualTo(20)); // 30 - 10

        // Verify offer is deleted
        Assert.That(ActDb.PartyTransferOffers.FirstOrDefault(o => o.Id == offer.Id), Is.Null);
    }

    [Test]
    public async Task ShouldAcceptPartialOffer()
    {
        Party respondingParty = new() { User = new User(), Gold = 100, Troops = 20 };
        Party offeringParty = new() { User = new User(), Gold = 200, Troops = 30 };
        ArrangeDb.Parties.AddRange(respondingParty, offeringParty);

        PartyTransferOffer offer = new()
        {
            PartyId = offeringParty.Id,
            TargetPartyId = respondingParty.Id,
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = respondingParty.Id,
            TransferOfferId = offer.Id,
            Accept = true,
            Accepted = new RespondToPartyTransferOfferCommand.TransferOffer
            {
                Gold = 25, // Less than offered
                Troops = 3, // Less than offered
                Items = [],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Value, Is.Not.Null);

        // Verify state changes
        var updatedRespondingParty = ActDb.Parties.Single(p => p.Id == respondingParty.Id);
        var updatedOfferingParty = ActDb.Parties.Single(p => p.Id == offeringParty.Id);

        Assert.That(updatedRespondingParty.Gold, Is.EqualTo(125)); // 100 + 25
        Assert.That(updatedRespondingParty.Troops, Is.EqualTo(23)); // 20 + 3
        Assert.That(updatedOfferingParty.Gold, Is.EqualTo(175)); // 200 - 25
        Assert.That(updatedOfferingParty.Troops, Is.EqualTo(27)); // 30 - 3
    }

    [Test]
    public async Task ShouldDeclineOffer()
    {
        Party respondingParty = new() { User = new User() };
        Party offeringParty = new() { User = new User() };
        ArrangeDb.Parties.AddRange(respondingParty, offeringParty);

        PartyTransferOffer offer = new()
        {
            PartyId = offeringParty.Id,
            TargetPartyId = respondingParty.Id,
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = respondingParty.Id,
            TransferOfferId = offer.Id,
            Accept = false,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Value, Is.Not.Null);

        // Verify offer is deleted
        Assert.That(ActDb.PartyTransferOffers.FirstOrDefault(o => o.Id == offer.Id), Is.Null);

        // Verify no state changes to parties
        var respondingPartyCheck = ActDb.Parties.Single(p => p.Id == respondingParty.Id);
        var offeringPartyCheck = ActDb.Parties.Single(p => p.Id == offeringParty.Id);
        Assert.That(respondingPartyCheck.Gold, Is.EqualTo(0));
        Assert.That(offeringPartyCheck.Gold, Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldReturnErrorIfAcceptedGoldExceedsOffered()
    {
        Party respondingParty = new() { User = new User(), Gold = 100, Troops = 20 };
        Party offeringParty = new() { User = new User(), Gold = 200, Troops = 30 };
        ArrangeDb.Parties.AddRange(respondingParty, offeringParty);

        PartyTransferOffer offer = new()
        {
            PartyId = offeringParty.Id,
            TargetPartyId = respondingParty.Id,
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = respondingParty.Id,
            TransferOfferId = offer.Id,
            Accept = true,
            Accepted = new RespondToPartyTransferOfferCommand.TransferOffer
            {
                Gold = 150, // More than offered
                Troops = 5,
                Items = [],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.TransferOfferInvalidAmount));
    }

    [Test]
    public async Task ShouldReturnErrorIfOfferingPartyNotEnoughGold()
    {
        Party respondingParty = new() { User = new User(), Gold = 100, Troops = 20 };
        Party offeringParty = new() { User = new User(), Gold = 50, Troops = 30 }; // Not enough gold
        ArrangeDb.Parties.AddRange(respondingParty, offeringParty);

        PartyTransferOffer offer = new()
        {
            PartyId = offeringParty.Id,
            TargetPartyId = respondingParty.Id,
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = respondingParty.Id,
            TransferOfferId = offer.Id,
            Accept = true,
            Accepted = new RespondToPartyTransferOfferCommand.TransferOffer
            {
                Gold = 50,
                Troops = 5,
                Items = [],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughGold));
    }

    [Test]
    public async Task ShouldReturnErrorIfAcceptedTroopsExceedsOffered()
    {
        Party respondingParty = new() { User = new User(), Gold = 100, Troops = 20 };
        Party offeringParty = new() { User = new User(), Gold = 200, Troops = 30 };
        ArrangeDb.Parties.AddRange(respondingParty, offeringParty);

        PartyTransferOffer offer = new()
        {
            PartyId = offeringParty.Id,
            TargetPartyId = respondingParty.Id,
            Status = PartyTransferOfferStatus.Pending,
            Gold = 100,
            Troops = 10,
        };
        ArrangeDb.PartyTransferOffers.Add(offer);
        await ArrangeDb.SaveChangesAsync();

        RespondToPartyTransferOfferCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToPartyTransferOfferCommand
        {
            PartyId = respondingParty.Id,
            TransferOfferId = offer.Id,
            Accept = true,
            Accepted = new RespondToPartyTransferOfferCommand.TransferOffer
            {
                Gold = 50,
                Troops = 15, // More than offered
                Items = [],
            },
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.TransferOfferInvalidAmount));
    }
}
