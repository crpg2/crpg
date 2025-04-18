using System.Text.RegularExpressions;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands;

public record CreateClanCommand : IMediatorRequest<ClanViewModel>
{
    public int UserId { get; init; }
    public string Tag { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public uint PrimaryColor { get; init; }
    public uint SecondaryColor { get; init; }
    public string BannerKey { get; init; } = string.Empty;
    public Region Region { get; init; }
    public IList<Languages> Languages { get; init; } = new List<Languages>();
    public Uri? Discord { get; init; }
    public TimeSpan ArmoryTimeout { get; init; }

    public class Validator : AbstractValidator<CreateClanCommand>
    {
        public Validator(Constants constants)
        {
            RuleFor(c => c.Tag)
                .MinimumLength(constants.ClanTagMinLength)
                .MaximumLength(constants.ClanTagMaxLength)
                .Matches(new Regex(constants.ClanTagRegex, RegexOptions.Compiled));

            RuleFor(c => c.PrimaryColor)
                .GreaterThanOrEqualTo(constants.ClanColorMinValue);

            RuleFor(c => c.SecondaryColor)
                .GreaterThanOrEqualTo(constants.ClanColorMinValue);

            RuleFor(c => c.Name)
                .MinimumLength(constants.ClanNameMinLength)
                .MaximumLength(constants.ClanNameMaxLength);

            RuleFor(c => c.Description)
                .MinimumLength(constants.ClanDescriptionMinLength)
                .MaximumLength(constants.ClanDescriptionMaxLength);

            RuleFor(c => c.BannerKey)
                .MinimumLength(0)
                .MaximumLength(constants.ClanBannerKeyMaxLength)
                .Matches(new Regex(constants.ClanBannerKeyRegex, RegexOptions.Compiled));

            RuleFor(cmd => cmd.Region).IsInEnum();

            RuleFor(c => c.Discord)
                .Must(u => u == null || u.Host == "discord.gg");

            RuleFor(c => c.ArmoryTimeout)
                .GreaterThanOrEqualTo(TimeSpan.FromDays(1));
        }
    }

    internal class Handler : IMediatorRequestHandler<CreateClanCommand, ClanViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateClanCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        public async Task<Result<ClanViewModel>> Handle(CreateClanCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.ClanMembership != null)
            {
                return new(CommonErrors.UserAlreadyInAClan(req.UserId));
            }

            var conflictingClan = await _db.Clans
                .FirstOrDefaultAsync(c => c.Tag == req.Tag || c.Name == req.Name, cancellationToken);
            if (conflictingClan != null)
            {
                return conflictingClan.Tag == req.Tag
                    ? new(CommonErrors.ClanTagAlreadyUsed(conflictingClan.Tag))
                    : new(CommonErrors.ClanNameAlreadyUsed(conflictingClan.Name));
            }

            var clan = new Clan
            {
                Tag = req.Tag,
                PrimaryColor = req.PrimaryColor,
                SecondaryColor = req.SecondaryColor,
                Name = req.Name,
                Description = req.Description,
                BannerKey = req.BannerKey,
                Region = req.Region,
                Languages = req.Languages,
                Discord = req.Discord,
                ArmoryTimeout = req.ArmoryTimeout,
                Members =
                {
                    new ClanMember
                    {
                        User = user,
                        Role = ClanMemberRole.Leader,
                    },
                },
            };

            _db.Clans.Add(clan);
            _db.ActivityLogs.Add(_activityLogService.CreateClanCreatedLog(req.UserId, clan.Id));
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' created clan '[{1}] {2}' ({3})", req.UserId, req.Tag, req.Name, clan.Id);
            return new(_mapper.Map<ClanViewModel>(clan));
        }
    }
}
