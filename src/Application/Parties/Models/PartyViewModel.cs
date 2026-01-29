using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Settlements.Models;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Parties;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

public record PartyViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public int Gold { get; init; }
    public int Troops { get; init; }
    public Point Position { get; init; } = default!;
    public PartyStatus Status { get; init; }
    public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
    [JsonRequired]
    public PartyVisibleViewModel? TargetedParty { get; init; }
    [JsonRequired]
    public SettlementPublicViewModel? TargetedSettlement { get; init; }
    [JsonRequired]
    public BattleViewModel? TargetedBattle { get; init; }
    public List<BattleJoinIntentViewModel> BattleJoinIntents { get; init; } = [];
    public UserPublicViewModel User { get; init; } = default!;

    public List<PartyOrderViewModel> Orders { get; set; } = [];

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, PartyViewModel>()
            .ForMember(h => h.Troops, opt => opt.MapFrom(u => (int)u.Troops));
    }
}
