using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands;

public record RespondClanInvitationCommand : IMediatorRequest<ClanInvitationViewModel>
{
    public int UserId { get; init; }
    public int ClanId { get; init; }
    public int ClanInvitationId { get; init; }
    public bool Accept { get; init; }

    internal class Handler : IMediatorRequestHandler<RespondClanInvitationCommand, ClanInvitationViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespondClanInvitationCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IClanService _clanService;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserNotificationService _userNotificationService;

        public Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService, IActivityLogService activityLogService, IUserNotificationService userNotificationService)
        {
            _db = db;
            _mapper = mapper;
            _clanService = clanService;
            _activityLogService = activityLogService;
            _userNotificationService = userNotificationService;
        }

        public async Task<Result<ClanInvitationViewModel>> Handle(RespondClanInvitationCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var invitation = await _db.ClanInvitations.FirstOrDefaultAsync(ci =>
                ci.Id == req.ClanInvitationId && ci.ClanId == req.ClanId, cancellationToken);
            if (invitation == null)
            {
                return new(CommonErrors.ClanInvitationNotFound(req.ClanInvitationId));
            }

            if (invitation.Status != ClanInvitationStatus.Pending)
            {
                return new(CommonErrors.ClanInvitationClosed(invitation.Id, invitation.Status));
            }

            if ((invitation.Type != ClanInvitationType.Offer || invitation.InviteeId != user.Id)
                && invitation.Type != ClanInvitationType.Request)
            {
                // Too lazy to return proper errors.
                return new(new Error(ErrorType.InternalError, ErrorCode.InternalError));
            }

            User invitee;
            User inviter;
            if (invitation.Type == ClanInvitationType.Offer) // User responds to an invitation offer.
            {
                invitee = user;
                inviter = await _db.Users.FirstAsync(u => u.Id == invitation.InviterId, cancellationToken);
            }
            else // User responds to request to join a clan.
            {
                invitee = await _db.Users
                    .Include(u => u.ClanMembership)
                    .FirstAsync(u => u.Id == invitation.InviteeId, cancellationToken);
                inviter = user;

                var error = _clanService.CheckClanMembership(inviter, invitation.ClanId);
                if (error != null)
                {
                    return new(error);
                }

                if (inviter.ClanMembership!.Role != ClanMemberRole.Officer
                    && inviter.ClanMembership.Role != ClanMemberRole.Leader)
                {
                    return new(CommonErrors.ClanMemberRoleNotMet(inviter.Id,
                        ClanMemberRole.Officer, inviter.ClanMembership.Role));
                }
            }

            if (!req.Accept)
            {
                invitation.Status = ClanInvitationStatus.Declined;
                invitation.InviterId = inviter.Id; // If invitation was a request, invited == invitee.

                if (invitation.Type == ClanInvitationType.Offer) // TODO: implement offer ui
                {
                    Logger.LogInformation("User '{0}' declined invitation '{1}' from user '{2}' to join clan '{3}'",
                        invitee.Id, invitation.Id, inviter.Id, invitation.ClanId);
                }
                else // Request
                {
                    _db.ActivityLogs.Add(_activityLogService.CreateClanApplicationDeclinedLog(user.Id, invitation.ClanId));
                    Logger.LogInformation("User '{0}' declined request to join '{1}' from user '{2}' to join clan '{3}'",
                        inviter.Id, invitation.Id, invitee.Id, invitation.ClanId);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return new(_mapper.Map<ClanInvitationViewModel>(invitation));
            }

            int? oldClanId = null;
            if (invitee.ClanMembership != null)
            {
                oldClanId = invitee.ClanMembership.ClanId;
                var clanLeaveRes = await _clanService.LeaveClan(_db, invitee.ClanMembership, cancellationToken);
                if (clanLeaveRes.Errors != null)
                {
                    return new(clanLeaveRes.Errors);
                }
            }

            var clanJoinRes = await _clanService.JoinClan(_db, invitee, invitation.ClanId, cancellationToken);
            if (clanJoinRes.Errors != null)
            {
                return new(clanJoinRes.Errors);
            }

            invitation.Status = ClanInvitationStatus.Accepted;

            if (invitation.Type == ClanInvitationType.Offer) // TODO: implement offer ui
            {
                if (oldClanId == null)
                {
                    Logger.LogInformation("User '{0}' accepted invitation '{1}' from user '{2}' to join clan '{3}'", invitee.Id, invitation.Id, inviter.Id, invitation.ClanId);
                }
                else
                {
                    Logger.LogInformation("User '{0}' left clan '{1}' and accepted invitation '{2}' from user '{3}' to join clan '{4}'", invitee.Id, oldClanId, invitation.Id, inviter.Id, invitation.ClanId);
                }
            }
            else // Request
            {
                if (oldClanId == null)
                {
                    Logger.LogInformation("User '{0}' accepted request '{1}' from user '{2}' to join clan '{3}'", inviter.Id, invitation.Id, invitee.Id, invitation.ClanId);
                    _db.ActivityLogs.Add(_activityLogService.CreateClanApplicationAcceptedLog(user.Id, invitation.ClanId));
                    _db.UserNotifications.Add(_userNotificationService.CreateClanApplicationAcceptedToUserNotification(invitee.Id, invitation.ClanId));
                }
                else
                {
                    Logger.LogInformation("User '{0}' accepted request '{1}' from user '{2}' to join left clan '{3}' for clan '{4}'", inviter.Id, invitation.Id, invitee.Id, oldClanId, invitation.ClanId);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<ClanInvitationViewModel>(invitation));
        }
    }
}
