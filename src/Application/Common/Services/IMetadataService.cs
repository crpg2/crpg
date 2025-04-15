namespace Crpg.Application.Common.Services;

internal interface IMetadataService
{
    EntitiesFromMetadata ExtractEntitiesFromMetadata(IEnumerable<KeyValuePair<string, string>> metadata);
}

internal record struct EntitiesFromMetadata
{
    public EntitiesFromMetadata()
    {
        ClansIds = new List<int>();
        UsersIds = new List<int>();
        CharactersIds = new List<int>();
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

            if ((md.Key == "userId" || md.Key == "actorUserId" || md.Key == "targetUserId")
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
}
