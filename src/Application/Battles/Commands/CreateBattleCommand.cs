using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Sdk.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record CreateBattleCommand : IMediatorRequest<BattleDetailedViewModel>
{
    public DateTime ScheduledFor { get; init; }
    public Region Region { get; init; }
    public int AttackerId { get; init; }
    public int DefenderId { get; init; }
    public int UserId { get; init; } = default!;
    public int AttackerTroops { get; init; } = default!;
    public int DefenderTroops { get; init; } = default!;
    public class Validator : AbstractValidator<CreateBattleCommand>
    {
        public Validator(IDateTime dateTime)
        {
            RuleFor(a => a.AttackerId != a.DefenderId);
            RuleFor(a => a.Region).IsInEnum();
            RuleFor(a => a.ScheduledFor > dateTime.UtcNow);
        }
    }

    internal class Handler : IMediatorRequestHandler<CreateBattleCommand, BattleDetailedViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateBattleCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime)
        {
            _db = db;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<Result<BattleDetailedViewModel>> Handle(CreateBattleCommand req, CancellationToken cancellationToken)
        {
            var attackerParty = await _db.Parties
                .FirstOrDefaultAsync(h => h.Id == req.AttackerId, cancellationToken);
            if (attackerParty == null)
            {
                return new(CommonErrors.PartyNotFound(req.AttackerId));
            }

            if (attackerParty.Status == PartyStatus.InBattle)
            {
                return new(CommonErrors.PartyInBattle(req.AttackerId));
            }

            var defenderParty = await _db.Parties
                .FirstOrDefaultAsync(h => h.Id == req.DefenderId, cancellationToken);
            if (defenderParty == null)
            {
                return new(CommonErrors.PartyNotFound(req.DefenderId));
            }

            if (defenderParty.Status == PartyStatus.InBattle)
            {
                return new(CommonErrors.PartyInBattle(req.DefenderId));
            }

            attackerParty.Troops = req.AttackerTroops;
            attackerParty.Status = PartyStatus.InBattle;
            defenderParty.Troops = req.DefenderTroops;
            defenderParty.Status = PartyStatus.InBattle;

            Battle newBattle = new()
            {
                Fighters = new()
                {
                    new() { Side = BattleSide.Attacker, Commander = true, Party = attackerParty },
                    new() { Side = BattleSide.Defender, Commander = true, Party = defenderParty },
                },
                Region = req.Region,
                Phase = BattlePhase.Scheduled,
                ScheduledFor = req.ScheduledFor,
                CreatedAt = _dateTime.UtcNow,
            };

            _db.Battles.Add(newBattle);

            Logger.LogInformation("Battle '{0}', between '{1}' and '{2}', created by '{3}'",
                newBattle.Id, req.AttackerId, req.DefenderId, req.UserId);

            await _db.SaveChangesAsync(cancellationToken);
            return new(_mapper.Map<BattleDetailedViewModel>(newBattle));
        }
    }
}
