using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public class ClanArmoryItemViewModel : IMapFrom<ClanArmoryItem>
{
    public int UserItemId { get; set; }
    public int UserId { get; set; }
    public int BorrowerUserId { get; set; } // 0 - empty
    public ItemViewModel Item { get; init; } = default!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ClanArmoryItem, ClanArmoryItemViewModel>()
            .ForMember(cai => cai.UserId, opt => opt.MapFrom(src => src.LenderUserId))
            .ForMember(cai => cai.BorrowerUserId, opt => opt.MapFrom(src => src.BorrowedItem != null ? src.BorrowedItem.BorrowerUserId : 0))
            .ForMember(cai => cai.Item, opt => opt.MapFrom(src => src.UserItem != null ? src.UserItem.Item : null));
    }
}
