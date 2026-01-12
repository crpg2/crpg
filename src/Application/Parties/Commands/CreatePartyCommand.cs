using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record CreatePartyCommand : IMediatorRequest<PartyViewModel>
{
    [JsonIgnore]
    public int UserId { get; set; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap, Constants constants) : IMediatorRequestHandler<CreatePartyCommand, PartyViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreatePartyCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IStrategusMap _strategusMap = strategusMap;
        private readonly Constants _constants = constants;

        public async ValueTask<Result<PartyViewModel>> Handle(CreatePartyCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Party)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.Party != null)
            {
                return new(CommonErrors.UserAlreadyRegisteredToStrategus(req.UserId));
            }

            user.Party = new Party
            {
                Gold = 0,
                Troops = _constants.StrategusMinPartyTroops,
                Position = _strategusMap.GetSpawnPosition(user.Region),
                Status = PartyStatus.Idle,
                CurrentPartyId = null,
                CurrentSettlementId = null,
                CurrentBattleId = null,
                Orders = [],
            };

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' registered to Strategus", req.UserId);
            return new(_mapper.Map<PartyViewModel>(user.Party));
        }
    }
}
