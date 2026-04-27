using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record GameUserItemExtendedViewModel : IMapFrom<UserItem>
{
    public int Id { get; init; }
    public int UserId { get; init; }
    // public ItemViewModel Item { get; init; } = default!;
    public string ItemId { get; init; } = default!;
    public int Rank { get; init; }
    public bool IsBroken { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsArmoryItem { get; init; }
    public bool IsPersonal { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserItem, GameUserItemExtendedViewModel>()
            // .ForMember(ui => ui.ItemId, config => config.MapFrom(src => src.Item != null ? src.Item.Id : "not found"));
            .ForMember(ui => ui.Rank, config => config.MapFrom(src => src.Item != null ? src.Item.Rank : 0))
            .ForMember(ui => ui.IsArmoryItem, config => config.MapFrom(src => src.ClanArmoryItem != null))
            .ForMember(ui => ui.IsPersonal, config => config.MapFrom(src => src.PersonalItem != null));
    }
}
