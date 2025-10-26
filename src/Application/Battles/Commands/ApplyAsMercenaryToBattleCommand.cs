using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
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

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<ApplyAsMercenaryToBattleCommand, BattleMercenaryApplicationViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ApplyAsMercenaryToBattleCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

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

            // User cannot apply as a mercenary in battle they are fighting.
            bool isUserFighter = await _db.BattleFighters.AnyAsync(f =>
                f.BattleId == battle.Id && f.PartyId == character.UserId, cancellationToken);
            if (isUserFighter)
            {
                return new(CommonErrors.PartyFighter(character.UserId, battle.Id));
            }

            // Check for existing application.
            var application = await _db.BattleMercenaryApplications
                .Where(a => a.CharacterId == req.CharacterId
                            && a.BattleId == req.BattleId
                            && a.Side == req.Side // We allow applications for both sides. // TODO: spec
                            && (a.Status == BattleMercenaryApplicationStatus.Pending))
                .FirstOrDefaultAsync(cancellationToken);

            /*
                We don't allow applications to be edited, as there may be various controversial situations
                Ex.: a mercenary changed their wage, but the battle commander did not update the mercenary applications table and accepted the mercenary at the old wage.

                For change the wage or the note of the application:
                    1/ delete the current (Pending) application
                    2/ create a new one
             */
            if (application != null)
            {
                return new(CommonErrors.ApplicationAlreadyExist(application.Id));
            }

            application = new BattleMercenaryApplication
            {
                Side = req.Side,
                Wage = req.Wage,
                Note = req.Note,
                Status = BattleMercenaryApplicationStatus.Pending,
                Character = character,
            };
            battle.MercenaryApplications.Add(application);
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation(
                "User '{0}' applied as a mercenary to battle '{1}' for the side '{2}' with character '{3}'",
                character.UserId, battle.Id, req.Side, character.Id);

            return new(new BattleMercenaryApplicationViewModel
            {
                Id = application.Id,
                User = _mapper.Map<UserPublicViewModel>(character.User),
                Character = new CharacterPublicViewModel
                {
                    Id = character.Id,
                    Level = character.Level,
                    Class = character.Class,
                },
                Side = application.Side,
                Wage = application.Wage,
                Note = application.Note,
                Status = application.Status,
            });
        }
    }
}
