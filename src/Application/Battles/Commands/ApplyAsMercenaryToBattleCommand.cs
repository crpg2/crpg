using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record ApplyAsMercenaryToBattleCommand : IMediatorRequest<BattleMercenaryApplicationViewModel>
{
    [JsonIgnore]
    public int UserId { get; init; }
    public int CharacterId { get; init; }
    [JsonIgnore]
    public int BattleId { get; init; }
    public BattleSide Side { get; init; }
    public int Wage { get; init; }
    public string Note { get; init; } = string.Empty;

    public class Validator : AbstractValidator<ApplyAsMercenaryToBattleCommand>
    {
        public Validator(Constants constants)
        {
            RuleFor(a => a.Side).IsInEnum();
            RuleFor(a => a.Wage).InclusiveBetween(0, constants.StrategusMercenaryMaxWage);
            RuleFor(a => a.Note).MaximumLength(constants.StrategusMercenaryNoteMaxLength);
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService) : IMediatorRequestHandler<ApplyAsMercenaryToBattleCommand, BattleMercenaryApplicationViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ApplyAsMercenaryToBattleCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IActivityLogService _activityLogService = activityLogService;

        public async Task<Result<BattleMercenaryApplicationViewModel>> Handle(
            ApplyAsMercenaryToBattleCommand req,
            CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            var battle = await _db.Battles.FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            var battleParticipant = await _db.BattleParticipants.FirstOrDefaultAsync(bp =>
                  bp.BattleId == battle.Id &&
                  bp.CharacterId == req.CharacterId,
                  cancellationToken);

            // User cannot apply as a mercenary in battle they are fighting.
            if (battleParticipant?.Type == BattleParticipantType.Party)
            {
                return new(CommonErrors.PartyFighter(character.UserId, battle.Id));
            }

            // Check for existing applications
            var existingApplications = (await _db.BattleMercenaryApplications
                    .Where(a =>
                        a.CharacterId == req.CharacterId &&
                        a.BattleId == req.BattleId &&
                        (a.Status == BattleMercenaryApplicationStatus.Pending || a.Status == BattleMercenaryApplicationStatus.Accepted))
                    .ToListAsync(cancellationToken))
                .GroupBy(a => a.Side)
                .ToList();

            if (existingApplications.Count != 0)
            {
                BattleSide oppositeSide = req.Side == BattleSide.Attacker ? BattleSide.Defender : BattleSide.Attacker;

                var currentSideApplications = existingApplications.FirstOrDefault(g => g.Key == req.Side);
                var oppositeSideApplications = existingApplications.FirstOrDefault(g => g.Key == oppositeSide);

                int? activeApplicationId = battleParticipant?.MercenaryApplicationId;

                if (currentSideApplications != null)
                {
                    /*
                        We don't allow applications to be edited, as there may be various controversial situations
                        Ex.: a mercenary changed their wage, but the battle commander did not update the mercenary applications table and accepted the mercenary at the old wage.
                        For change the wage or the note of the application:
                        1/ delete the current (Pending) application
                        2/ create a new one
                    */
                    var currentSidePendingApplication = currentSideApplications.FirstOrDefault(a => a.Status == BattleMercenaryApplicationStatus.Pending);
                    if (currentSidePendingApplication != null)
                    {
                        return new(CommonErrors.ApplicationAlreadyExist(currentSidePendingApplication.Id));
                    }

                    /*
                        There can be many applications with the status “Accepted”.
                        If a battle participant left the battle or was kicked out, and then reapplied and was accepted.
                    */
                    var currentSideAcceptedApplication = currentSideApplications.FirstOrDefault(a => a.Status == BattleMercenaryApplicationStatus.Accepted && a.Id == activeApplicationId);
                    if (currentSideAcceptedApplication != null)
                    {
                        return new(CommonErrors.ApplicationAlreadyExist(currentSideAcceptedApplication.Id));
                    }
                }

                // D'ont allow creating a application if there is already an accepted application for the opposite side
                if (oppositeSideApplications != null
                    && oppositeSideApplications.Any(a => a.Status == BattleMercenaryApplicationStatus.Accepted && a.Id == activeApplicationId))
                {
                    return new(CommonErrors.BattleMercenaryAlreadyExist(req.Side, oppositeSide));
                }
            }

            var newApplication = new BattleMercenaryApplication
            {
                Battle = battle,
                Character = character,
                Side = req.Side,
                Wage = req.Wage,
                Note = req.Note,
                Status = BattleMercenaryApplicationStatus.Pending,
            };
            _db.BattleMercenaryApplications.Add(newApplication);
            _db.ActivityLogs.Add(_activityLogService.CreateApplyAsMercenaryToBattleLog(battle.Id, req.Side, req.UserId, character.Id));
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation(
                "User '{0}' applied as a mercenary to battle '{1}' for the side '{2}' with character '{3}'",
                character.UserId, battle.Id, req.Side, character.Id);

            return new(new BattleMercenaryApplicationViewModel
            {
                Id = newApplication.Id,
                User = _mapper.Map<UserPublicViewModel>(character.User),
                Character = _mapper.Map<CharacterPublicViewModel>(character),
                Side = newApplication.Side,
                Wage = newApplication.Wage,
                Note = newApplication.Note,
                Status = newApplication.Status,
            });
        }
    }
}
