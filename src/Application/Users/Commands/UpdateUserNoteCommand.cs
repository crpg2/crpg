using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record UpdateUserNoteCommand : IMediatorRequest<UserPrivateViewModel>
{
    [JsonIgnore]
    public int UserId { get; init; }
    public string Note { get; init; } = default!;

    internal class Handler : IMediatorRequestHandler<UpdateUserNoteCommand, UserPrivateViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateUserNoteCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<UserPrivateViewModel>> Handle(UpdateUserNoteCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.Note != req.Note)
            {
                user.Note = req.Note;
            }

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' updated note", req.UserId);
            return new Result<UserPrivateViewModel>(_mapper.Map<UserPrivateViewModel>(user));
        }
    }
}
