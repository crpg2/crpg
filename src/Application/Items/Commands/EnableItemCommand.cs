using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record EnableItemCommand : IMediatorRequest
{
    public string ItemId { get; init; } = string.Empty;
    public bool Enable { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<EnableItemCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<EnableItemCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async ValueTask<Result> Handle(EnableItemCommand req, CancellationToken cancellationToken)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            if (req.Enable)
            {
                item.Enabled = true;
            }
            else
            {
                item.Enabled = false;
                await _db.EquippedItems
                    .RemoveRangeAsync(ei => ei.UserItem!.ItemId == req.ItemId, cancellationToken);

                await _db.ClanArmoryBorrowedItems
                    .RemoveRangeAsync(bi => bi.UserItem!.ItemId == req.ItemId, cancellationToken);

                await _db.ClanArmoryItems
                    .RemoveRangeAsync(ci => ci.UserItem!.ItemId == req.ItemId, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' {1} item '{2}'", req.UserId,
                req.Enable ? "enabled" : "disabled", req.ItemId);
            return Result.NoErrors;
        }
    }
}
