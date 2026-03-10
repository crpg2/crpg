using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record RefundItemCommand : IMediatorRequest
{
    public string ItemId { get; init; } = string.Empty;
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<RefundItemCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RefundItemCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async ValueTask<Result> Handle(RefundItemCommand req, CancellationToken cancellationToken)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            var userItems = await _db.UserItems
                .Include(ui => ui.User)
                .Include(ui => ui.Item)
                .Where(ui => ui.ItemId == item.Id)
                .ToArrayAsync(cancellationToken);
            foreach (var userItem in userItems)
            {
                userItem.User!.Gold += userItem.Item!.Price;
                // Trick to avoid UpdatedAt to be updated.
                userItem.User.UpdatedAt = userItem.User.UpdatedAt;
                if (userItem.Item!.Rank > 0)
                {
                    userItem.User.HeirloomPoints += userItem.Item!.Rank;
                }

                _db.UserItems.Remove(userItem);
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' refunded item '{2}'", req.UserId, req.ItemId);
            return Result.NoErrors;
        }
    }
}
