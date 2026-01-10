using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RespondToBattleMercenaryApplicationCommand : IMediatorRequest<BattleMercenaryApplicationViewModel>
{
    [JsonIgnore]
    public int PartyId { get; init; }
    [JsonIgnore]
    public int MercenaryApplicationId { get; init; }
    public bool Accept { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IBattleService battleService, IActivityLogService activityLogService, IUserNotificationService userNotificationService) : IMediatorRequestHandler<RespondToBattleMercenaryApplicationCommand, BattleMercenaryApplicationViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespondToBattleMercenaryApplicationCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IBattleService _battleService = battleService;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;

        public async Task<Result<BattleMercenaryApplicationViewModel>> Handle(RespondToBattleMercenaryApplicationCommand req,
            CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var application = await _db.BattleMercenaryApplications
                .AsSplitQuery()
                .Include(a => a.Battle!)
                    .ThenInclude(b => b.Fighters)
                .Include(a => a.Battle!)
                    .ThenInclude(b => b.Participants)
                .Include(a => a.Character!)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == req.MercenaryApplicationId, cancellationToken);
            if (application == null)
            {
                return new(CommonErrors.ApplicationNotFound(req.MercenaryApplicationId));
            }

            var partyFighter = application.Battle!.Fighters.FirstOrDefault(f => f.PartyId == req.PartyId);
            if (partyFighter == null)
            {
                return new(CommonErrors.PartyNotAFighter(party.Id, application.BattleId));
            }

            if (partyFighter.Side != application.Side)
            {
                return new(CommonErrors.PartiesNotOnTheSameSide(party.Id, 0, application.BattleId));
            }

            if (application.Battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(application.BattleId, application.Battle.Phase));
            }

            if (application.Status != BattleMercenaryApplicationStatus.Pending)
            {
                return new(CommonErrors.ApplicationClosed(application.Id));
            }

            int totalParticipantSlots = _battleService.CalculateTotalParticipantSlots(application.Battle, application.Side);
            int currentParticipantsCount = application.Battle.Participants.Count(p => p.Side == application.Side);

            if (req.Accept)
            {
                if (currentParticipantsCount >= totalParticipantSlots)
                {
                    return new(CommonErrors.BattleParticipantSlotsExceeded(application.BattleId, application.Side, totalParticipantSlots));
                }

                application.Status = BattleMercenaryApplicationStatus.Accepted;
                BattleParticipant newParticipant = new()
                {
                    Side = application.Side,
                    Character = application.Character,
                    Battle = application.Battle,
                    CaptainFighter = partyFighter,
                    Type = BattleParticipantType.Mercenary,
                    MercenaryApplication = application,
                };
                _db.BattleParticipants.Add(newParticipant);

                // Delete all other applying party pending applications for this battle.
                var otherApplications = await _db.BattleMercenaryApplications
                    .Where(a => a.Id != application.Id
                                && a.BattleId == application.BattleId
                                && a.Character!.UserId == application.Character!.UserId
                                && a.Status == BattleMercenaryApplicationStatus.Pending)
                    .ToArrayAsync(cancellationToken);
                _db.BattleMercenaryApplications.RemoveRange(otherApplications);
            }
            else
            {
                application.Status = BattleMercenaryApplicationStatus.Declined;
            }

            _db.ActivityLogs.Add(_activityLogService.CreateRespondToBattleMercenaryApplicationLog(application.Battle.Id, req.MercenaryApplicationId, req.PartyId, req.Accept));
            _db.UserNotifications.Add(_userNotificationService.CreateBattleMercenaryApplicationRespondedNotification(application.Character!.UserId, application.Battle.Id, req.Accept));
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Party '{0}' {1} application '{2}' from character '{3}' to join battle '{4}' as a mercenary",
                req.PartyId, req.Accept ? "accepted" : "declined", req.MercenaryApplicationId, application.CharacterId, application.BattleId);
            return new(new BattleMercenaryApplicationViewModel
            {
                Id = application.Id,
                User = _mapper.Map<UserPublicViewModel>(application.Character!.User),
                Character = _mapper.Map<CharacterPublicViewModel>(application.Character),
                Wage = application.Wage,
                Note = application.Note,
                Side = application.Side,
                Status = application.Status,
            });
        }
    }
}
