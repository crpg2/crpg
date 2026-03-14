using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetUserItemPresetsQuery : IMediatorRequest<IList<UserItemPresetViewModel>>
{
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetUserItemPresetsQuery, IList<UserItemPresetViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<IList<UserItemPresetViewModel>>> Handle(GetUserItemPresetsQuery req, CancellationToken cancellationToken)
        {
            var presets = await _db.UserItemPresets
                .AsSplitQuery()
                .Include(p => p.Slots)
                    .ThenInclude(s => s.Item)
                .Where(p => p.UserId == req.UserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToArrayAsync(cancellationToken);

            return new(_mapper.Map<IList<UserItemPresetViewModel>>(presets));
        }
    }
}
