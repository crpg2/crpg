using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands;

/// <summary>
/// Admin command to assign a clan-exclusive item to a clan.
/// Creates a <see cref="UserItem"/> with a <see cref="ClanItem"/> marker for every
/// current clan member. New members who join later will receive the item automatically
/// via <see cref="Crpg.Application.Common.Services.IClanService.JoinClan"/>.
/// </summary>
public record RewardClanItemCommand : IMediatorRequest
{
    [JsonIgnore]
    public int ActorUserId { get; init; }

    public int ClanId { get; init; }
    public string ItemId { get; init; } = string.Empty;

    internal class Handler : IMediatorRequestHandler<RewardClanItemCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RewardClanItemCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async ValueTask<Result> Handle(RewardClanItemCommand req, CancellationToken cancellationToken)
        {
            var clan = await _db.Clans
                .Include(c => c.Members)
                .FirstOrDefaultAsync(c => c.Id == req.ClanId, cancellationToken);

            if (clan == null)
            {
                return new(CommonErrors.ClanNotFound(req.ClanId));
            }

            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);

            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            // Prevent assigning the same exclusive item to a clan twice.
            bool alreadyExists = await _db.ClanItems
                .AnyAsync(
                    ci => ci.ClanId == req.ClanId && ci.UserItem!.ItemId == req.ItemId,
                    cancellationToken);

            if (alreadyExists)
            {
                return new(CommonErrors.ClanItemAlreadyExist(req.ClanId, req.ItemId));
            }

            // Provision a copy of the item (with the clan marker) to every current member.
            foreach (var member in clan.Members)
            {
                _db.UserItems.Add(new UserItem
                {
                    UserId = member.UserId,
                    Item = item,
                    ClanItem = new ClanItem { ClanId = req.ClanId },
                });
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation(
                "Clan '{ClanId}' rewarded with exclusive item '{ItemId}' by actor user '{ActorUserId}'",
                req.ClanId, req.ItemId, req.ActorUserId);

            return Result.NoErrors;
        }
    }
}
