using System.Text.Json;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Common.Services;

internal interface IMetadataService
{
    EntitiesFromMetadata ExtractEntitiesFromMetadata(IEnumerable<KeyValuePair<string, string>> metadata);

    MetadataItem? ConvertItemToMetadataItem(Item? item);

    List<IMetadata> ConvertMarketplaceOfferToMetadata(MarketplaceOfferAsset offered, MarketplaceOfferAsset requested);
}

internal interface IMetadata
{
    string Key { get; }
    string Value { get; }
}

internal record Metadata(string Key, string Value) : IMetadata;

internal class MetadataItem
{
    public string Id { get; set; } = string.Empty;
    public string BaseId { get; set; } = string.Empty;
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal readonly record struct EntitiesFromMetadata
{
    public EntitiesFromMetadata()
    {
        ClansIds = [];
        UsersIds = [];
        CharactersIds = [];
    }

    public IList<int> ClansIds { get; init; }
    public IList<int> UsersIds { get; init; }
    public IList<int> CharactersIds { get; init; }
}

internal class MetadataService : IMetadataService
{
    public EntitiesFromMetadata ExtractEntitiesFromMetadata(IEnumerable<KeyValuePair<string, string>> metadata)
    {
        var output = new EntitiesFromMetadata();

        foreach (var md in metadata)
        {
            if (md.Key == "clanId" && int.TryParse(md.Value, out int clanId))
            {
                if (!output.ClansIds.Contains(clanId))
                {
                    output.ClansIds.Add(clanId);
                }
            }

            if ((md.Key == "userId" || md.Key == "actorUserId" || md.Key == "targetUserId" || md.Key == "sellerId" || md.Key == "buyerId")
                && int.TryParse(md.Value, out int userId))
            {
                if (!output.UsersIds.Contains(userId))
                {
                    output.UsersIds.Add(userId);
                }
            }

            if (md.Key == "characterId" && int.TryParse(md.Value, out int characterId))
            {
                if (!output.CharactersIds.Contains(characterId))
                {
                    output.CharactersIds.Add(characterId);
                }
            }
        }

        return output;
    }

    public List<IMetadata> ConvertMarketplaceOfferToMetadata(MarketplaceOfferAsset offered, MarketplaceOfferAsset requested)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return
        [
            new Metadata("offered", JsonSerializer.Serialize(new
            {
                offered.Gold,
                offered.HeirloomPoints,
                Item = ConvertItemToMetadataItem(offered.UserItem?.Item),
            }, options)),
            new Metadata("requested", JsonSerializer.Serialize(new
            {
                requested.Gold,
                requested.HeirloomPoints,
                Item = ConvertItemToMetadataItem(requested.Item),
            }, options)),
        ];
    }

    public MetadataItem? ConvertItemToMetadataItem(Item? item)
    {
        if (item == null)
        {
            return null;
        }

        return new MetadataItem
        {
            Id = item.Id,
            BaseId = item.BaseId,
            Name = item.Name,
            Rank = item.Rank,
        };
    }
}
