using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record GameConvertCharacterCharacteristicsCommand : IMediatorRequest<CharacterCharacteristicsViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }
    public CharacterCharacteristicConversion Conversion { get; init; }

    public static implicit operator CharacterCharacteristicConversion(GameConvertCharacterCharacteristicsCommand v) => throw new NotImplementedException();

    public class Validator : AbstractValidator<GameConvertCharacterCharacteristicsCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Conversion).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<GameConvertCharacterCharacteristicsCommand, CharacterCharacteristicsViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<GameConvertCharacterCharacteristicsCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<CharacterCharacteristicsViewModel>> Handle(GameConvertCharacterCharacteristicsCommand req,
            CancellationToken cancellationToken)
        {
            var character = await _db.Characters.FirstOrDefaultAsync(c =>
                c.UserId == req.UserId && c.Id == req.CharacterId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (req.Conversion == CharacterCharacteristicConversion.AttributesToSkills)
            {
                if (character.Characteristics.Attributes.Points < 1)
                {
                    return new(CommonErrors.NotEnoughAttributePoints(1, character.Characteristics.Attributes.Points));
                }

                character.Characteristics.Attributes.Points -= 1;
                character.Characteristics.Skills.Points += 2;
            }
            else if (req.Conversion == CharacterCharacteristicConversion.SkillsToAttributes)
            {
                if (character.Characteristics.Skills.Points < 2)
                {
                    return new(CommonErrors.NotEnoughSkillPoints(1, character.Characteristics.Skills.Points));
                }

                character.Characteristics.Skills.Points -= 2;
                character.Characteristics.Attributes.Points += 1;
            }

            await _db.SaveChangesAsync(cancellationToken);

            (string from, string to) = req.Conversion == CharacterCharacteristicConversion.AttributesToSkills
                ? ("attributes", "skills")
                : ("skills", "attributes");
            Logger.LogInformation("User '{0}' converted characteristic points of character '{1}' from {2} to {3}",
                req.UserId, req.CharacterId, from, to);

            return new(_mapper.Map<CharacterCharacteristicsViewModel>(character.Characteristics));
        }
    }
}
