// using System.Text.Json.Serialization;
// using AutoMapper;
// using Crpg.Application.Battles.Models;
// using Crpg.Application.Common.Interfaces;
// using Crpg.Application.Common.Mediator;
// using Crpg.Application.Common.Results;
// using Crpg.Application.Common.Services;
// using Crpg.Application.Parties.Models;
// using Crpg.Domain.Entities.Battles;
// using Crpg.Domain.Entities.Parties;
// using FluentValidation;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using NetTopologySuite.Geometries;
// using LoggerFactory = Crpg.Logging.LoggerFactory;

// namespace Crpg.Application.Parties.Commands;

// public record UpdatePartyStatusCommand : IMediatorRequest<PartyViewModel>
// {
//     [JsonIgnore]
//     public int PartyId { get; set; }
//     public PartyStatus Status { get; init; }
//     public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
//     public int TargetedPartyId { get; init; }
//     public int TargetedSettlementId { get; init; }
//     public int TargetedBattletId { get; init; }
//     public BattleJoinIntentViewModel[] BattleJoinIntents { get; init; } = [];

//     public class Validator : AbstractValidator<UpdatePartyStatusCommand>
//     {
//         public Validator()
//         {
//             RuleFor(m => m.Status).IsInEnum();
//             RuleForEach(m => m.BattleJoinIntents).ChildRules(intent =>
//             {
//                 intent.RuleFor(i => i.Side).IsInEnum();
//             });
//         }
//     }

//     internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap) : IMediatorRequestHandler<UpdatePartyStatusCommand, PartyViewModel>
//     {
//         private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdatePartyStatusCommand>();

//         private readonly ICrpgDbContext _db = db;
//         private readonly IMapper _mapper = mapper;
//         private readonly IStrategusMap _strategusMap = strategusMap;

//         public async Task<Result<PartyViewModel>> Handle(UpdatePartyStatusCommand req, CancellationToken cancellationToken)
//         {
//             var party = await _db.Parties
//                 .Include(h => h.User)
//                 .Include(h => h.TargetedSettlement)
//                 .Include(h => h.BattleJoinIntents)
//                 .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
//             if (party == null)
//             {
//                 return new(CommonErrors.PartyNotFound(req.PartyId));
//             }

//             if (party.Status == PartyStatus.InBattle || party.Status == PartyStatus.AwaitingBattleJoinDecision)
//             {
//                 return new(CommonErrors.PartyInBattle(req.PartyId));
//             }

//             if (req.Status == PartyStatus.IdleInSettlement || req.Status == PartyStatus.RecruitingInSettlement)
//             {
//                 var result = StartStopRecruiting(req.Status == PartyStatus.RecruitingInSettlement, party);
//                 if (result.Errors != null)
//                 {
//                     return new(result.Errors);
//                 }
//             }
//             else
//             {
//                 var result = await UpdatePartyMovement(party, req, cancellationToken);
//                 if (result.Errors != null)
//                 {
//                     return new(result.Errors);
//                 }
//             }

//             await _db.SaveChangesAsync(cancellationToken);
//             Logger.LogInformation("Party '{0}' updated their status", req.PartyId);
//             return new(_mapper.Map<PartyViewModel>(party));
//         }

//         private static Result StartStopRecruiting(bool start, Party party)
//         {
//             if (start)
//             {
//                 if (party.Status != PartyStatus.IdleInSettlement)
//                 {
//                     return new(CommonErrors.PartyNotInASettlement(party.Id));
//                 }
//             }
//             else
//             {
//                 if (party.Status != PartyStatus.RecruitingInSettlement)
//                 {
//                     return new(CommonErrors.PartyNotInASettlement(party.Id));
//                 }
//             }

//             party.Status = start ? PartyStatus.RecruitingInSettlement : PartyStatus.IdleInSettlement;
//             return Result.NoErrors;
//         }

//         private async Task<Result> UpdatePartyMovement(Party party, UpdatePartyStatusCommand req,
//             CancellationToken cancellationToken)
//         {
//             // Reset movement.
//             party.Status = PartyStatus.Idle;
//             party.Waypoints = MultiPoint.Empty;
//             party.TargetedPartyId = null;
//             party.TargetedSettlementId = null;
//             party.TargetedBattleId = null;

//             // TODO: FIXME: SPEC:
//             // Remove old battle intents if leaving previous targets
//             if (party.BattleJoinIntents.Count != 0)
//             {
//                 _db.BattleJoinIntents.RemoveRange(party.BattleJoinIntents);
//                 party.BattleJoinIntents.Clear();
//             }

//             // Remove old pending battle fighter applications - TODO: проверить
//             var pendingBattleFighterApplications = await _db.BattleFighterApplications
//                 .Where(a => a.PartyId == party.Id && a.Status == BattleFighterApplicationStatus.Pending)
//                 .ToArrayAsync(cancellationToken);
//             _db.BattleFighterApplications.RemoveRange(pendingBattleFighterApplications);

//             // TODO: FIXME:

//             if (req.Status == PartyStatus.MovingToPoint)
//             {
//                 if (!req.Waypoints.IsEmpty)
//                 {
//                     party.Status = req.Status;
//                     party.Waypoints = req.Waypoints;
//                 }
//             }
//             else if (req.Status == PartyStatus.FollowingParty
//                      || req.Status == PartyStatus.MovingToAttackParty)
//             {
//                 var targetParty = await _db.Parties
//                     .Include(h => h.User)
//                     .FirstOrDefaultAsync(h => h.Id == req.TargetedPartyId, cancellationToken);
//                 if (targetParty == null)
//                 {
//                     return new Result(CommonErrors.UserNotFound(req.TargetedPartyId));
//                 }

//                 if (!party.Position.IsWithinDistance(targetParty.Position, _strategusMap.ViewDistance))
//                 {
//                     return new Result(CommonErrors.PartyNotInSight(req.TargetedPartyId));
//                 }

//                 party.Status = req.Status;
//                 // Need to be set manually because it was set to null above and it can confuse EF Core.
//                 party.TargetedPartyId = targetParty.Id;
//                 party.TargetedParty = targetParty;
//             }
//             else if (req.Status == PartyStatus.MovingToSettlement
//                      || req.Status == PartyStatus.MovingToAttackSettlement)
//             {
//                 var targetSettlement = await _db.Settlements
//                     .Include(s => s.Owner!.User)
//                     .FirstOrDefaultAsync(s => s.Id == req.TargetedSettlementId, cancellationToken);
//                 if (targetSettlement == null)
//                 {
//                     return new Result(CommonErrors.SettlementNotFound(req.TargetedSettlementId));
//                 }

//                 party.Status = req.Status;
//                 // Need to be set manually because it was set to null above and it can confuse EF Core.
//                 party.TargetedSettlementId = targetSettlement.Id;
//                 party.TargetedSettlement = targetSettlement;
//             }

//             // TODO: FIXME: SPEC:
//             else if (req.Status == PartyStatus.MovingToBattle)
//             {
//                 var targetBattle = await _db.Battles
//                     .FirstOrDefaultAsync(b => b.Id == req.TargetedBattletId, cancellationToken);
//                 if (targetBattle == null)
//                 {
//                     return new Result(CommonErrors.BattleNotFound(req.TargetedBattletId));
//                 }

//                 if (req.BattleJoinIntents.Length == 0)
//                 {
//                     return new Result(CommonErrors.BattleJoinIntentsIsEmpty());
//                 }

//                 foreach (var intent in req.BattleJoinIntents)
//                 {
//                     party.BattleJoinIntents.Add(new BattleJoinIntent
//                     {
//                         BattleId = intent.BattleId,
//                         Side = intent.Side,
//                         Party = party,
//                     });
//                 }

//                 party.Status = PartyStatus.MovingToBattle;

//                 // Need to be set manually because it was set to null above and it can confuse EF Core.
//                 party.TargetedBattleId = targetBattle.Id;
//                 party.TargetedBattle = targetBattle;
//             }

//             return Result.NoErrors;
//         }
//     }
// }
