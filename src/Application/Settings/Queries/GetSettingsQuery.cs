using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settings.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settings.Queries;

public record GetSettingsQuery : IMediatorRequest<SettingsViewModel>
{
    internal class Handler : IMediatorRequestHandler<GetSettingsQuery, SettingsViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<SettingsViewModel>> Handle(GetSettingsQuery req,
            CancellationToken cancellationToken)
        {
            var settings = await _db.Settings.FirstOrDefaultAsync(cancellationToken);

            return settings == null
                ? new(CommonErrors.SettingsNotFound(1))
                : new(_mapper.Map<SettingsViewModel>(settings));
        }
    }
}
