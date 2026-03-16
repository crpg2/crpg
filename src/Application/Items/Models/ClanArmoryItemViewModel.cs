using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Items.Models;

public class ClanArmoryItemViewModel : IMapFrom<ClanArmoryItem>
{
    public int UserItemId { get; set; }
    public UserPublicViewModel Lender { get; init; } = default!;
    [JsonRequired]
    public UserPublicViewModel? Borrower { get; init; }
    public ItemViewModel Item { get; init; } = default!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ClanArmoryItem, ClanArmoryItemViewModel>()
            .ForMember(cai => cai.Lender, opt => opt.MapFrom(src => src.Lender!.User))
            .ForMember(cai => cai.Borrower, opt => opt.MapFrom(src => src.BorrowedItem != null && src.BorrowedItem.Borrower != null ? src.BorrowedItem.Borrower.User : null))
            .ForMember(cai => cai.Item, opt => opt.MapFrom(src => src.UserItem != null ? src.UserItem.Item : null));
    }
}
