using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands;

public record DeleteUserItemPresetCommand : IMediatorRequest
{
    public int UserItemPresetId { get; init; }
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<DeleteUserItemPresetCommand>
    {
        private readonly ICrpgDbContext _db = db;

        public async ValueTask<Result> Handle(DeleteUserItemPresetCommand req, CancellationToken cancellationToken)
        {
            var preset = await _db.UserItemPresets
                .FirstOrDefaultAsync(p => p.Id == req.UserItemPresetId && p.UserId == req.UserId, cancellationToken);

            if (preset == null)
            {
                return new(CommonErrors.UserItemPresetNotFound(req.UserId, req.UserItemPresetId));
            }

            _db.UserItemPresets.Remove(preset);
            await _db.SaveChangesAsync(cancellationToken);
            return new Result();
        }
    }
}
