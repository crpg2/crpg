using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Crpg.Module.Api.Exceptions;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.ActivityLogs;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using Crpg.Module.Helpers.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TaleWorlds.Library;
using Platform = Crpg.Module.Api.Models.Users.Platform;

namespace Crpg.Module.Api;

/// <summary>
/// Client for Crpg.WebApi.Controllers.GamesController.
/// </summary>
internal class HttpCrpgClient : ICrpgClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly JsonSerializerSettings _serializerSettings;

    private Task? _refreshAccessTokenTask;

    public HttpCrpgClient(string apiUrl, string apiKey)
    {
        SocketsHttpHandler socketsHttpHandler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip,
            ConnectTimeout = TimeSpan.FromSeconds(30),
        };
        string version = typeof(HttpCrpgClient).Assembly.GetName().Version!.ToString();
        _httpClient = new HttpClient(socketsHttpHandler)
        {
            BaseAddress = new Uri(apiUrl),
            Timeout = TimeSpan.FromSeconds(10),
            DefaultRequestHeaders =
            {
                { "Accept", "application/json" },
                { "User-Agent",  "cRPG/" + version },
            },
        };

        _apiKey = apiKey;

        _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
            Converters = new JsonConverter[]
            {
                new TimeSpanConverter(),
                new ArrayStringEnumFlagsConverter(),
                new StringEnumConverter(),
            },
        };
    }

    public Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId, CrpgRegion region,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> queryParameters = new(StringComparer.Ordinal)
        {
            ["platform"] = platform.ToString(),
            ["platformUserId"] = platformUserId,
            ["region"] = region.ToString(),
            ["instance"] = CrpgServerConfiguration.Instance.ToString(),
        };
        return Get<CrpgUser>("games/users", queryParameters, cancellationToken);
    }

    public Task<CrpgResult<IList<CrpgUserItemExtended>>> GetUserItemsAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return Get<IList<CrpgUserItemExtended>>("games/users/" + userId + "/items", null, cancellationToken);
    }

    public Task<CrpgResult<CrpgCharacter>> GetUserCharacterBasicAsync(int userId, int characterId,
        CancellationToken cancellationToken = default)
    {
        return Get<CrpgCharacter>("games/users/" + userId + "/characters/" + characterId, null, cancellationToken);
    }

    public Task<CrpgResult<CrpgCharacterCharacteristics>> UpdateCharacterCharacteristicsAsync(int userId, int characterId, CrpgGameCharacterCharacteristicsUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameCharacterCharacteristicsUpdateRequest, CrpgCharacterCharacteristics>("games/users/" + userId + "/characters/" + characterId + "/characteristics", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgCharacterCharacteristics>> ConvertCharacterCharacteristicsAsync(int userId, int characterId, CrpgGameCharacteristicConversionRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameCharacteristicConversionRequest, CrpgCharacterCharacteristics>("games/users/" + userId + "/characters/" + characterId + "/characteristics/convert", req, cancellationToken);
    }

    public Task<CrpgResult<IList<CrpgEquippedItemExtended>>> GetCharacterEquippedItemsAsync(int userId, int characterId,
        CancellationToken cancellationToken = default)
    {
        return Get<IList<CrpgEquippedItemExtended>>("games/users/" + userId + "/characters/" + characterId + "/items", null, cancellationToken);
    }

    public Task<CrpgResult<IList<CrpgEquippedItemId>>> UpdateCharacterEquippedItemsAsync(int userId, int characterId, CrpgGameCharacterItemsUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameCharacterItemsUpdateRequest, IList<CrpgEquippedItemId>>("games/users/" + userId + "/characters/" + characterId + "/items", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgUser>> GetTournamentUserAsync(Platform platform, string platformUserId,
    CancellationToken cancellationToken = default)
    {
        Dictionary<string, string> queryParameters = new(StringComparer.Ordinal)
        {
            ["platform"] = platform.ToString(),
            ["platformUserId"] = platformUserId,
        };
        return Get<CrpgUser>("games/tournament-users", queryParameters, cancellationToken);
    }

    public Task CreateActivityLogsAsync(IList<CrpgActivityLog> activityLogs, CancellationToken cancellationToken = default)
    {
        return Post<IList<CrpgActivityLog>, object>("games/activity-logs", activityLogs, cancellationToken);
    }

    public Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default)
    {
        return Get<CrpgClan>("games/clans/" + clanId, null, cancellationToken);
    }

    public Task<CrpgResult<IList<CrpgClanArmoryItem>>> GetClanArmoryAsync(int clanId, int userId, CancellationToken cancellationToken = default)
    {
        var queryParameters = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["userId"] = userId.ToString(),
        };

        return Get<IList<CrpgClanArmoryItem>>("games/clans/" + clanId + "/armory", queryParameters, cancellationToken);
    }

    public Task<CrpgResult<CrpgClanArmoryItem>> ClanArmoryAddItemAsync(int clanId, CrpgGameClanArmoryAddItemRequest req, CancellationToken cancellationToken = default)
    {
        return Post<CrpgGameClanArmoryAddItemRequest, CrpgClanArmoryItem>("games/clans/" + clanId + "/armory", req, cancellationToken);
    }

    public Task<CrpgResult<object>> RemoveClanArmoryItemAsync(int clanId, int userItemId, int userId,
        CancellationToken cancellationToken = default)
    {
        var req = new { UserId = userId }; // anonymous type for JSON body
        return Delete<object, object>($"games/clans/{clanId}/armory/{userItemId}", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgClanArmoryBorrowedItem>> ClanArmoryBorrowItemAsync(int clanId, int userItemId, CrpgGameBorrowClanArmoryItemRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameBorrowClanArmoryItemRequest, CrpgClanArmoryBorrowedItem>($"games/clans/{clanId}/armory/{userItemId}/borrow", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgClanArmoryBorrowedItem>> ClanArmoryReturnItemAsync(int clanId, int userItemId, CrpgGameBorrowClanArmoryItemRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameBorrowClanArmoryItemRequest, CrpgClanArmoryBorrowedItem>($"games/clans/{clanId}/armory/{userItemId}/return", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Put<CrpgGameUsersUpdateRequest, CrpgUsersUpdateResponse>("games/users", req, cancellationToken);
    }

    public Task<CrpgResult<CrpgRestriction>> RestrictUserAsync(CrpgRestrictionRequest req, CancellationToken cancellationToken = default)
    {
        return Post<CrpgRestrictionRequest, CrpgRestriction>("games/restrictions", req, cancellationToken);
    }

    public void Dispose() => _httpClient.Dispose();

    private Task<CrpgResult<TResponse>> Get<TResponse>(string requestUri, Dictionary<string, string>? queryParameters,
        CancellationToken cancellationToken) where TResponse : class
    {
        if (queryParameters != null)
        {
            FormUrlEncodedContent urlEncodedContent = new(queryParameters);
            string query = urlEncodedContent.ReadAsStringAsync().Result;
            requestUri += '?' + query;
        }

        HttpRequestMessage msg = new(HttpMethod.Get, requestUri);
        return Send<TResponse>(msg, cancellationToken);
    }

    private Task<CrpgResult<TResponse>> Put<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
    {
        HttpRequestMessage msg = new(HttpMethod.Put, requestUri)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload, _serializerSettings), Encoding.UTF8, "application/json"),
        };

        return Send<TResponse>(msg, cancellationToken);
    }

    private Task<CrpgResult<TResponse>> Post<TRequest, TResponse>(string requestUri, TRequest payload, CancellationToken cancellationToken) where TResponse : class
    {
        HttpRequestMessage msg = new(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonConvert.SerializeObject(payload, _serializerSettings), Encoding.UTF8, "application/json"),
        };

        return Send<TResponse>(msg, cancellationToken);
    }

    private Task<CrpgResult<TResponse>> Delete<TRequest, TResponse>(string requestUri, TRequest? payload,
        CancellationToken cancellationToken) where TResponse : class
    {
        HttpRequestMessage msg = new(HttpMethod.Delete, requestUri);

        if (payload != null)
        {
            string json = JsonConvert.SerializeObject(payload, _serializerSettings);
            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return Send<TResponse>(msg, cancellationToken);
    }

    private async Task<CrpgResult<TResponse>> Send<TResponse>(HttpRequestMessage msg, CancellationToken cancellationToken) where TResponse : class
    {
        for (int retry = 0; retry < 2; retry += 1)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                await RefreshAccessToken();
            }

            Debug.Print($"Sending {msg.Method} {msg.RequestUri}");
            var res = await _httpClient.SendAsync(msg, cancellationToken);
            string json = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                continue;
            }

            if (!res.IsSuccessStatusCode)
            {
                CrpgResult crpgRes;
                try
                {
                    crpgRes = JsonConvert.DeserializeObject<CrpgResult>(json, _serializerSettings)!;
                }
                catch
                {
                    throw new Exception($"{res.StatusCode}: {json}");
                }

                throw new CrpgClientException(crpgRes);
            }

            return JsonConvert.DeserializeObject<CrpgResult<TResponse>>(json, _serializerSettings)!;
        }

        throw new Exception("Couldn't send request even after refreshing access token");
    }

    private Task RefreshAccessToken()
    {
        if (_refreshAccessTokenTask == null || _refreshAccessTokenTask.IsCompleted)
        {
            _refreshAccessTokenTask = DoRefreshAccessToken();
        }

        return _refreshAccessTokenTask;

        async Task DoRefreshAccessToken()
        {
            Debug.Print("Refreshing access token");
            var tokenRequest = new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "game_api"),
                new KeyValuePair<string, string>("client_id", "crpg-game-server"),
                new KeyValuePair<string, string>("client_secret", _apiKey),
            };

            var tokenResponse = await _httpClient.PostAsync("connect/token", new FormUrlEncodedContent(tokenRequest));
            string tokenResponseBody = await tokenResponse.Content.ReadAsStringAsync();
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new Exception("Couldn't get token: " + tokenResponseBody);
            }

            string accessToken = JObject.Parse(tokenResponseBody)["access_token"]!.ToString();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            Debug.Print("Access token successfully refreshed");
        }
    }
}
