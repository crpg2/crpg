using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetUserItemPresetQuery : IMediatorRequest<UserItemPresetViewModel>
{
    public int UserItemPresetId { get; init; }
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetUserItemPresetQuery, UserItemPresetViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<UserItemPresetViewModel>> Handle(GetUserItemPresetQuery req, CancellationToken cancellationToken)
        {
            var preset = await _db.UserItemPresets
                .AsNoTracking()
                .Include(p => p.Slots)
                .FirstOrDefaultAsync(p => p.Id == req.UserItemPresetId && p.UserId == req.UserId, cancellationToken);

            if (preset == null)
            {
                return new(CommonErrors.UserItemPresetNotFound(req.UserId, req.UserItemPresetId));
            }

            return new(_mapper.Map<UserItemPresetViewModel>(preset));
        }
    }
}
