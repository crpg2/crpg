using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Settlements;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Settlements.Models;

public record SettlementPublicViewModel : IMapFrom<Settlement>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public SettlementType Type { get; init; }
    public Point Position { get; init; } = default!;
    public Culture Culture { get; init; }
    public Region Region { get; init; }
    public int Troops { get; init; }
    [JsonRequired]
    public UserPublicViewModel? Owner { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Settlement, SettlementPublicViewModel>()
            .ForMember(s => s.Owner, opt => opt.MapFrom(s => s.Owner!.User));
    }
}
