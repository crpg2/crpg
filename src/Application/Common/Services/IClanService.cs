﻿using System.Security.Claims;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Crpg.Application.Common.Services;

internal interface IClanService
{
    Task<Result<User>> GetClanMember(ICrpgDbContext db, int userId, int clanId, CancellationToken cancellationToken);
    Error? CheckClanMembership(User user, int clanId);
    Task<Result<ClanMember>> JoinClan(ICrpgDbContext db, User user, int clanId, CancellationToken cancellationToken);
    Task<Result> LeaveClan(ICrpgDbContext db, ClanMember member, CancellationToken cancellationToken);

    Task<Result<ClanArmoryItem>> AddArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default);
    Task<Result> RemoveArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default);
    Task<Result<ClanArmoryBorrowedItem>> BorrowArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default);
    Task<Result> ReturnArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default);
}

internal class ClanService : IClanService
{
    public async Task<Result<User>> GetClanMember(ICrpgDbContext db, int userId, int clanId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.ClanMembership)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return new(CommonErrors.UserNotFound(userId));
        }

        var error = CheckClanMembership(user, clanId);
        return error != null ? new(error) : new(user);
    }

    public Error? CheckClanMembership(User user, int clanId)
    {
        if (user.ClanMembership == null)
        {
            return CommonErrors.UserNotInAClan(user.Id);
        }

        if (user.ClanMembership.ClanId != clanId)
        {
            return CommonErrors.UserNotAClanMember(user.Id, clanId);
        }

        return null;
    }

    public async Task<Result<ClanMember>> JoinClan(ICrpgDbContext db, User user, int clanId, CancellationToken cancellationToken)
    {
        user.ClanMembership = new ClanMember
        {
            UserId = user.Id,
            ClanId = clanId,
            Role = ClanMemberRole.Member,
        };

        // Joining a clan declines all pending invitations and delete pending requests to join.
        var invitations = await db.ClanInvitations
            .Where(i => i.InviteeId == user.Id && i.Status == ClanInvitationStatus.Pending)
            .ToArrayAsync(cancellationToken);
        foreach (var invitation in invitations)
        {
            if (invitation.Type == ClanInvitationType.Request)
            {
                db.ClanInvitations.Remove(invitation);
            }
            else if (invitation.Type == ClanInvitationType.Offer)
            {
                invitation.Status = ClanInvitationStatus.Declined;
            }
        }

        return new(user.ClanMembership);
    }

    public async Task<Result> LeaveClan(ICrpgDbContext db, ClanMember member, CancellationToken cancellationToken)
    {
        // If user is leader and wants to leave, he needs to be the last member or have designated a new leader first.
        if (member.Role == ClanMemberRole.Leader)
        {
            await db.Entry(member)
                .Reference(m => m.Clan!)
                .Query()
                .Include(c => c.Members)
                .LoadAsync(cancellationToken);

            if (member.Clan!.Members.Count > 1)
            {
                return new Result(CommonErrors.ClanNeedLeader(member.ClanId));
            }

            db.Clans.Remove(member.Clan);
        }

        await db.Entry(member)
            .Collection(e => e.ArmoryItems)
            .Query().Include(e => e.BorrowedItem!).ThenInclude(e => e.UserItem!).ThenInclude(e => e.EquippedItems)
            .LoadAsync();

        await db.Entry(member)
            .Collection(e => e.ArmoryBorrowedItems)
            .Query().Include(e => e.UserItem!).ThenInclude(e => e.EquippedItems)
            .LoadAsync();

        db.EquippedItems.RemoveRange(member.ArmoryItems.SelectMany(e => e.BorrowedItem != null ? e.BorrowedItem.UserItem!.EquippedItems : new()));
        db.EquippedItems.RemoveRange(member.ArmoryBorrowedItems.SelectMany(e => e.UserItem!.EquippedItems));
        db.ClanArmoryBorrowedItems.RemoveRange(member.ArmoryItems.Select(e => e.BorrowedItem).OfType<ClanArmoryBorrowedItem>());
        db.ClanArmoryBorrowedItems.RemoveRange(member.ArmoryBorrowedItems);
        db.ClanArmoryItems.RemoveRange(member.ArmoryItems);

        db.ClanMembers.Remove(member);
        return Result.NoErrors;
    }

    public async Task<Result<ClanArmoryItem>> AddArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default)
    {
        await db.Entry(user)
            .Reference(e => e.ClanMembership)
            .LoadAsync(cancellationToken);

        var errors = CheckClanMembership(user, clan.Id);
        if (errors != null)
        {
            return new(errors);
        }

        var userItem = await db.UserItems
                .Where(e => e.UserId == user.Id && e.Id == userItemId)
                .Where(e => e.Item!.Enabled && e.Item.Type != ItemType.Banner)
                .Include(e => e.Item)
                .Include(e => e.ClanArmoryItem)
                .Include(e => e.EquippedItems)
                .FirstOrDefaultAsync(cancellationToken);
        if (userItem == null)
        {
            return new(CommonErrors.UserItemNotFound(userItemId));
        }

        if (userItem.EquippedItems.Any())
        {
            return new(CommonErrors.UserItemInUse(userItemId));
        }

        if (userItem.ClanArmoryItem != null)
        {
            return new(CommonErrors.UserItemInUse(userItemId));
        }

        var armoryItem = new ClanArmoryItem { LenderClanId = clan.Id, UserItemId = userItem.Id, LenderUserId = user.Id };
        db.ClanArmoryItems.Add(armoryItem);

        return new(armoryItem);
    }

    public async Task<Result> RemoveArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default)
    {
        await db.Entry(user)
            .Reference(e => e.ClanMembership)
            .LoadAsync(cancellationToken);

        var errors = CheckClanMembership(user, clan.Id);
        if (errors != null)
        {
            return new(errors);
        }

        var userItem = await db.UserItems
            .Where(e => e.Id == userItemId && e.UserId == user.Id)
            .Include(e => e.ClanArmoryItem)
            .Include(e => e.ClanArmoryBorrowedItem)
            .Include(e => e.EquippedItems)
            .FirstOrDefaultAsync(cancellationToken);
        if (userItem == null || userItem.ClanArmoryItem == null)
        {
            return new(CommonErrors.UserItemNotFound(userItemId));
        }

        db.EquippedItems.RemoveRange(userItem.EquippedItems);
        if (userItem.ClanArmoryBorrowedItem != null)
        {
            db.ClanArmoryBorrowedItems.Remove(userItem.ClanArmoryBorrowedItem);
        }

        db.ClanArmoryItems.Remove(userItem.ClanArmoryItem);

        return Result.NoErrors;
    }

    public async Task<Result<ClanArmoryBorrowedItem>> BorrowArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default)
    {
        await db.Entry(user)
            .Reference(e => e.ClanMembership)
            .LoadAsync();

        await db.Entry(user)
            .Collection(e => e.Items)
            .LoadAsync();

        var errors = CheckClanMembership(user, clan.Id);
        if (errors != null)
        {
            return new(errors);
        }

        var armoryItem = await db.ClanArmoryItems
            .Where(e => e.UserItemId == userItemId && e.LenderClanId == clan.Id && e.LenderUserId != user.Id)
            .Include(e => e.UserItem)
            .Include(e => e.BorrowedItem)
            .FirstOrDefaultAsync(cancellationToken);
        if (armoryItem == null)
        {
            return new(CommonErrors.UserItemNotFound(userItemId));
        }

        if (armoryItem.BorrowedItem != null)
        {
            return new(CommonErrors.UserItemInUse(userItemId));
        }

        if (user.Items.Any(e => e.ItemId == armoryItem.UserItem!.ItemId))
        {
            return new(CommonErrors.ItemAlreadyOwned(armoryItem.UserItem!.ItemId));
        }

        var borrowedItem = new ClanArmoryBorrowedItem { BorrowerClanId = clan.Id, UserItemId = armoryItem.UserItemId, BorrowerUserId = user.Id };
        db.ClanArmoryBorrowedItems.Add(borrowedItem);

        return new(borrowedItem);
    }

    public async Task<Result> ReturnArmoryItem(ICrpgDbContext db, Clan clan, User user, int userItemId, CancellationToken cancellationToken = default)
    {
        await db.Entry(user)
            .Reference(e => e.ClanMembership)
            .LoadAsync();

        var errors = CheckClanMembership(user, clan.Id);
        if (errors != null)
        {
            return new(errors);
        }

        var borrowedItem = await db.ClanArmoryBorrowedItems
            .Where(e => e.UserItemId == userItemId && e.BorrowerUserId == user.Id && e.BorrowerClanId == clan.Id)
            .Include(e => e.UserItem!).ThenInclude(e => e.EquippedItems)
            .FirstOrDefaultAsync(cancellationToken);
        if (borrowedItem == null)
        {
            return new(CommonErrors.UserItemNotFound(userItemId));
        }

        db.EquippedItems.RemoveRange(borrowedItem.UserItem!.EquippedItems);
        db.ClanArmoryBorrowedItems.Remove(borrowedItem);

        return Result.NoErrors;
    }
}
