using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record UpdateBattleSideBriefingCommand : IMediatorRequest<BattleSideBriefingViewModel>
{
    [JsonIgnore]
    public int BattleId { get; init; }
    public BattleSide Side { get; init; }
    public string Note { get; init; } = string.Empty;

    [JsonIgnore]
    public int PartyId { get; init; }

    public class Validator : AbstractValidator<UpdateBattleSideBriefingCommand>
    {
        public Validator(Constants constants)
        {
            RuleFor(c => c.Side).IsInEnum();
            RuleFor(c => c.Note).MaximumLength(constants.StrategusBattleSideBriefingNoteMaxLength);
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<UpdateBattleSideBriefingCommand, BattleSideBriefingViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateBattleSideBriefingCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<BattleSideBriefingViewModel>> Handle(UpdateBattleSideBriefingCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var battle = await _db.Battles
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Party)
                .Include(b => b.SideBriefings)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);

            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            var partyFighter = battle.Fighters.FirstOrDefault(f => f.PartyId == req.PartyId);

            if (partyFighter == null)
            {
                return new(CommonErrors.PartyNotAFighter(party.Id, req.BattleId));
            }

            if (!partyFighter.Commander)
            {
                return new(CommonErrors.FighterNotACommander(partyFighter.Id, req.BattleId));
            }

            if (partyFighter.Side != req.Side)
            {
                return new(CommonErrors.PartiesNotOnTheSameSide(party.Id, 0, req.BattleId));
            }

            var briefing = battle.SideBriefings.FirstOrDefault(b => b.Side == req.Side);

            if (briefing == null)
            {
                briefing = new BattleSideBriefing
                {
                    BattleId = battle.Id,
                    Side = req.Side,
                    Note = req.Note,
                };

                _db.BattleSideBriefings.Add(briefing);
            }
            else
            {
                briefing.Note = req.Note;
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Battle side briefing updated. BattleId='{0}', Side='{1}', UserId='{2}'", req.BattleId, req.Side, req.PartyId);

            return new(_mapper.Map<BattleSideBriefingViewModel>(briefing));
        }
    }
}
