using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public record GameUserItemViewModel : IMapFrom<UserItem>
{
    public int Id { get; init; }
    public string ItemId { get; init; } = default!;
    public int Rank { get; init; }
    public bool IsBroken { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserItem, GameUserItemViewModel>()
            .ForMember(d => d.Rank, opt => opt.MapFrom(s => s.Item != null ? s.Item.Rank : 0));
    }
}
