using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands;

public record CreateUserItemPresetCommand : IMediatorRequest<UserItemPresetViewModel>
{
    public string Name { get; init; } = string.Empty;
    public IList<UserItemPresetSlotInputModel> Slots { get; init; } = Array.Empty<UserItemPresetSlotInputModel>();

    [JsonIgnore]
    public int UserId { get; init; }

    public class Validator : AbstractValidator<CreateUserItemPresetCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(300);
            RuleFor(x => x.Slots)
                .NotEmpty();
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<CreateUserItemPresetCommand, UserItemPresetViewModel>
    {
        private static readonly HashSet<ItemSlot> AllSlots = [.. Enum.GetValues<ItemSlot>()];

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<UserItemPresetViewModel>> Handle(CreateUserItemPresetCommand req, CancellationToken cancellationToken)
        {
            if (!IsFullSlotSet(req.Slots))
            {
                return new(CommonErrors.UserItemPresetBadSlots());
            }

            Result? itemValidationError = await ValidateItemIds(req.Slots, cancellationToken);
            if (itemValidationError != null)
            {
                return new(itemValidationError.Errors!);
            }

            var preset = new UserItemPreset
            {
                UserId = req.UserId,
                Name = req.Name,
                Slots = [.. req.Slots
                    .Select(s => new UserItemPresetSlot
                    {
                        Slot = s.Slot,
                        ItemId = s.ItemId,
                    })],
            };

            _db.UserItemPresets.Add(preset);
            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<UserItemPresetViewModel>(preset));
        }

        private async ValueTask<Result?> ValidateItemIds(IList<UserItemPresetSlotInputModel> slots, CancellationToken cancellationToken)
        {
            string[] requestedItemIds = [.. slots
                .Select(s => s.ItemId)
                .Where(i => !string.IsNullOrEmpty(i))
                .Distinct()
                .Select(i => i!)];

            if (requestedItemIds.Length == 0)
            {
                return null;
            }

            string[] existingItemIds = await _db.Items
                .Where(i => requestedItemIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToArrayAsync(cancellationToken);

            if (existingItemIds.Length == requestedItemIds.Length)
            {
                return null;
            }

            string missingItemId = requestedItemIds.Except(existingItemIds).First();
            return new Result(CommonErrors.ItemNotFound(missingItemId));
        }

        private static bool IsFullSlotSet(IList<UserItemPresetSlotInputModel> slots)
        {
            if (slots.Count != AllSlots.Count)
            {
                return false;
            }

            HashSet<ItemSlot> slotSet = [.. slots.Select(s => s.Slot)];
            return slotSet.Count == AllSlots.Count && slotSet.SetEquals(AllSlots);
        }
    }
}
