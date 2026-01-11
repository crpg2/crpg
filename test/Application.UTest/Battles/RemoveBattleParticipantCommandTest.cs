using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RemoveBattleParticipantCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        RemoveBattleParticipantCommand.Handler handler = new(ActDb, Mock.Of<IActivityLogService>(), Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = 99,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, Mock.Of<IActivityLogService>(), Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotInHiringPhase()
    {
        Party party = new() { User = new User() };
        Battle battle = new();
        ArrangeDb.Battles.Add(battle);
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, Mock.Of<IActivityLogService>(), Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleParticipantNotFound()
    {
        Party party = new() { User = new User() };
        Battle battle = new() { Phase = BattlePhase.Hiring };
        ArrangeDb.Battles.Add(battle);
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, Mock.Of<IActivityLogService>(), Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            RemovedParticipantId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleParticipantNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfParticipantIsPartyFighter()
    {
        User user = new();
        Party party = new() { User = user };
        Character character = new() { User = user };
        BattleParticipant participant = new() { Type = BattleParticipantType.Party, Character = character };
        Battle battle = new() { Phase = BattlePhase.Hiring, Participants = { participant } };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Battles.Add(battle);
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, Mock.Of<IActivityLogService>(), Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            RemovedParticipantId = participant.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyFighter));
    }

    [Test]
    public async Task ShouldReturnErrorIfUserNotPartyFighter()
    {
        User user = new();
        Party party = new() { User = user };
        BattleParticipant participant = new() { Type = BattleParticipantType.Mercenary, Character = new() };
        Battle battle = new() { Phase = BattlePhase.Hiring, Participants = { participant } };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Parties.Add(party);
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, Mock.Of<IActivityLogService>(), Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            RemovedParticipantId = participant.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.FighterNotFound));
    }

    [Test]
    public async Task ShouldRemoveBattleParticipantIfParticipantLeaved()
    {
        User user = new();
        Party party = new() { User = user };
        Character character = new() { User = user };
        BattleParticipant participant = new() { Type = BattleParticipantType.Mercenary, Character = character };
        Battle battle = new() { Phase = BattlePhase.Hiring, Participants = { participant } };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Parties.Add(party);
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogService = new();
        activityLogService.Setup(s => s.CreateBattleParticipantLeavedLog(It.IsAny<int>(), It.IsAny<int>())).Returns(new ActivityLog());

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, activityLogService.Object, Mock.Of<IUserNotificationService>());
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            RemovedParticipantId = participant.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(await AssertDb.BattleParticipants.CountAsync(), Is.EqualTo(0));

        activityLogService.Verify(s => s.CreateBattleParticipantLeavedLog(battle.Id, party.Id), Times.Once);
    }

    [Test]
    public async Task ShouldRemoveBattleParticipantIfKickedByCaptain()
    {
        User captainUser = new();
        Party captainParty = new() { User = captainUser };
        Character captainCharacter = new() { User = captainUser };
        BattleFighter captainBattleFighter = new() { Party = captainParty, Commander = true, Side = BattleSide.Attacker };
        BattleParticipant captainParticipant = new() { Type = BattleParticipantType.Party, Character = captainCharacter, Side = BattleSide.Attacker };

        User mercenaryUser = new();
        Party mercenaryParty = new() { User = mercenaryUser };
        Character mercenaryCharacter = new() { User = mercenaryUser };
        BattleParticipant mercenaryParticipant = new() { Type = BattleParticipantType.Mercenary, Character = mercenaryCharacter, Side = BattleSide.Attacker };

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Participants = { captainParticipant, mercenaryParticipant },
            Fighters = { captainBattleFighter },
        };

        ArrangeDb.Users.AddRange([captainUser, mercenaryUser]);
        ArrangeDb.Parties.AddRange([captainParty, mercenaryParty]);
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogService = new();
        activityLogService.Setup(s => s.CreateBattleParticipantKickedLog(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new ActivityLog());

        Mock<IUserNotificationService> userNotificationService = new();
        userNotificationService.Setup(s => s.CreateBattleParticipantKickedToExParticipantNotification(It.IsAny<int>(), It.IsAny<int>())).Returns(new UserNotification());

        RemoveBattleParticipantCommand.Handler handler = new(ActDb, activityLogService.Object, userNotificationService.Object);
        var res = await handler.Handle(new RemoveBattleParticipantCommand
        {
            PartyId = captainParty.Id,
            BattleId = battle.Id,
            RemovedParticipantId = mercenaryParticipant.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(await AssertDb.BattleParticipants.CountAsync(), Is.EqualTo(1));

        activityLogService.Verify(s => s.CreateBattleParticipantKickedLog(battle.Id, mercenaryParticipant.Character.UserId, captainParty.Id), Times.Once);
        userNotificationService.Verify(s => s.CreateBattleParticipantKickedToExParticipantNotification(mercenaryParticipant.Character.UserId, battle.Id), Times.Once);
    }
}
