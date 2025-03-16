using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Crpg.GameServerManager.Api.Exceptions;
using Crpg.GameServerManager.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Crpg.GameServerManager.Api;

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
                new StringEnumConverter(),
            },
        };
    }

    public Task<CrpgResult> GetUpcomingStrategusBattles(CrpgRegion region, CancellationToken cancellationToken = default) => throw new NotImplementedException();

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

    private async Task<CrpgResult<TResponse>> Send<TResponse>(HttpRequestMessage msg, CancellationToken cancellationToken) where TResponse : class
    {
        for (int retry = 0; retry < 2; retry += 1)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                await RefreshAccessToken();
            }

            Console.WriteLine($"Sending {msg.Method} {msg.RequestUri}");
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
            Console.WriteLine("Refreshing access token");
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
            Console.WriteLine("Access token successfully refreshed");
        }
    }
}
