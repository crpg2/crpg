using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Crpg.Application.Users.Commands;
using Mediator;
using Microsoft.Extensions.Hosting;

namespace Crpg.WebApi.Workers;

internal class DonorSynchronizerWorker : BackgroundService
{
    private const int MinPatreonAmountCentsForRewards = 500;
    private const decimal MinAfdianAmountYuanPerMonth = 25m;
    private static readonly TimeSpan PatreonTokenRefreshInterval = TimeSpan.FromDays(14);

    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<DonorSynchronizerWorker>();
    private static readonly Regex SteamIdRegex = new(@"76561198\d{9}", RegexOptions.Compiled);

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _patreonTokenFilePath;

    private string? _patreonAccessToken;
    private string? _patreonRefreshToken;
    private DateTime _nextPatreonTokenRefreshAt = DateTime.MinValue;

    public DonorSynchronizerWorker(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IHostEnvironment hostEnvironment)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _patreonTokenFilePath = Path.Combine(hostEnvironment.ContentRootPath, "patreon_tokens.json");
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        LoadPersistedPatreonTokens();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                await TryRefreshPatreonTokenAsync(cancellationToken);

                var patreonDonors = await GetPatreonDonorsAsync(cancellationToken);
                var afdianDonors = await GetAfdianDonorsAsync(cancellationToken);

                string[] steamIds = patreonDonors.Concat(afdianDonors).Distinct().ToArray();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new UpdateUserDonorsCommand { PlatformUserIds = steamIds }, cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while updating donors");
            }

            await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
        }
    }

    private void LoadPersistedPatreonTokens()
    {
        if (!File.Exists(_patreonTokenFilePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(_patreonTokenFilePath);
            var stored = JsonSerializer.Deserialize<PatreonTokenStore>(json);
            if (stored == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(stored.AccessToken))
            {
                _patreonAccessToken = stored.AccessToken;
            }

            if (!string.IsNullOrEmpty(stored.RefreshToken))
            {
                _patreonRefreshToken = stored.RefreshToken;
            }

            if (stored.LastRefreshedAt != default)
            {
                _nextPatreonTokenRefreshAt = stored.LastRefreshedAt + PatreonTokenRefreshInterval;
            }

            Logger.LogInformation("Loaded persisted Patreon tokens; next refresh at {NextRefreshAt}", _nextPatreonTokenRefreshAt);
        }
        catch (Exception e)
        {
            Logger.LogWarning(e, "Failed to load persisted Patreon tokens from {FilePath}", _patreonTokenFilePath);
        }
    }

    private async Task TryRefreshPatreonTokenAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow < _nextPatreonTokenRefreshAt)
        {
            return;
        }

        string? refreshToken = _patreonRefreshToken ?? _configuration.GetValue<string>("Patreon:RefreshToken");
        string? clientId = _configuration.GetValue<string>("Patreon:ClientId");
        string? clientSecret = _configuration.GetValue<string>("Patreon:ClientSecret");

        if (refreshToken == null || clientId == null || clientSecret == null)
        {
            Logger.LogInformation("Patreon refresh token, client ID, or client secret not configured. Skipping token refresh");
            _nextPatreonTokenRefreshAt = DateTime.UtcNow + PatreonTokenRefreshInterval;
            return;
        }

        try
        {
            using HttpClient client = new();
            var response = await client.PostAsync(
                "https://www.patreon.com/api/oauth2/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken,
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                }),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<PatreonTokenResponse>(cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                Logger.LogError("Patreon token refresh returned an empty or null response");
                return;
            }

            _patreonAccessToken = tokenResponse.AccessToken;
            _patreonRefreshToken = tokenResponse.RefreshToken;
            _nextPatreonTokenRefreshAt = DateTime.UtcNow + PatreonTokenRefreshInterval;

            var tokenStore = new PatreonTokenStore
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                LastRefreshedAt = DateTime.UtcNow,
            };
            await File.WriteAllTextAsync(
                _patreonTokenFilePath,
                JsonSerializer.Serialize(tokenStore, new JsonSerializerOptions { WriteIndented = true }),
                cancellationToken);

            Logger.LogInformation("Patreon access token refreshed successfully; next refresh at {NextRefreshAt}", _nextPatreonTokenRefreshAt);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to refresh Patreon access token");
        }
    }

    private async Task<IEnumerable<string>> GetPatreonDonorsAsync(CancellationToken cancellationToken)
    {
        string? patreonAccessToken = _patreonAccessToken ?? _configuration.GetValue<string>("Patreon:AccessToken");
        if (patreonAccessToken == null)
        {
            Logger.LogInformation("No Patreon access token was provided. Skipping the donor synchronization");
            return [];
        }

        int campaignId = _configuration.GetValue<int>("Patreon:CampaignId");
        using HttpClient client = new();
        client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", patreonAccessToken);

        string uri = $"campaigns/{campaignId}/members?fields%5Bmember%5D=currently_entitled_amount_cents,note&page%5Bcount%5D=1000";
        var res = await client.GetFromJsonAsync<PatreonResponse<PatreonCampaignMember>>(uri, cancellationToken);

        List<string> steamIds = [];
        foreach (var member in res!.Data)
        {
            if (member.Attributes.CurrentlyEntitledAmountCents < MinPatreonAmountCentsForRewards * 0.85) // Allow a little margin.
            {
                continue;
            }

            string[] noteLines = member.Attributes.Note.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string noteLine in noteLines)
            {
                string[] noteParts = noteLine.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (noteParts.Length == 2 && noteParts[0] == "steam")
                {
                    steamIds.Add(noteParts[1]);
                }
            }
        }

        return steamIds;
    }

    private async Task<IEnumerable<string>> GetAfdianDonorsAsync(CancellationToken cancellationToken)
    {
        string? afdianToken = _configuration.GetValue<string>("Afdian:Token");
        if (afdianToken == null)
        {
            Logger.LogInformation("No Afdian token was provided. Skipping Afdian donor synchronization");
            return [];
        }

        string afdianUserId = _configuration.GetValue<string>("Afdian:UserId")!;
        using HttpClient client = new();
        client.BaseAddress = new Uri("https://afdian.com/api/open/");

        // Fetch all orders.
        List<AfdianOrder> allOrders = [];
        int orderPage = 1;
        int orderTotalPages;
        do
        {
            var res = await AfdianPostAsync<AfdianOrder>(client, "query-order", orderPage, afdianToken, afdianUserId,
                cancellationToken);
            allOrders.AddRange(res.List);
            orderTotalPages = res.TotalPage;
            orderPage++;
        }
        while (orderPage <= orderTotalPages);

        // Extract steam IDs from paid orders that meet the per-month threshold and are still active.
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<string> steamIds = [];
        foreach (var order in allOrders)
        {
            if (order.Status != AfdianOrderStatus.Paid)
            {
                continue;
            }

            if (!decimal.TryParse(order.TotalAmount, out decimal totalAmount))
            {
                continue;
            }

            int effectiveMonths = Math.Max(order.Month, 1);
            decimal perMonthAmount = totalAmount / effectiveMonths;
            if (perMonthAmount < MinAfdianAmountYuanPerMonth * 0.85m) // Allow a little margin.
            {
                continue;
            }

            // Check if the order is still active.
            DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(order.CreateTime).AddMonths(effectiveMonths);
            if (now > expiresAt)
            {
                continue;
            }

            var match = SteamIdRegex.Match(order.Remark);
            if (match.Success)
            {
                steamIds.Add(match.Value);
            }
        }

        return steamIds;
    }

    private static async Task<AfdianResponseData<T>> AfdianPostAsync<T>(
        HttpClient client, string endpoint, int page, string token, string userId, CancellationToken cancellationToken)
    {
        string paramsJson = JsonSerializer.Serialize(new { page });
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string signInput = $"{token}params{paramsJson}ts{ts}user_id{userId}";
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(signInput));
        string sign = Convert.ToHexStringLower(hash);

        AfdianRequestBody body = new() { UserId = userId, Params = paramsJson, Ts = ts, Sign = sign };
        JsonContent content = JsonContent.Create(body);
        var response = await client.PostAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AfdianResponse<T>>(cancellationToken))!.Data;
    }

    private class PatreonCampaignMember
    {
        [JsonPropertyName("currently_entitled_amount_cents")]
        public int CurrentlyEntitledAmountCents { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    private class PatreonResponse<T>
    {
        public PatreonResponseData<T>[] Data { get; set; } = [];
    }

    private class PatreonResponseData<T>
    {
        public Guid Id { get; set; }
        public T Attributes { get; set; } = default!;
        public string Type { get; set; } = string.Empty;
    }

    private class PatreonTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private class PatreonTokenStore
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime LastRefreshedAt { get; set; }
    }

    private class AfdianResponse<T>
    {
        public int Ec { get; set; }
        public string Em { get; set; } = string.Empty;
        public AfdianResponseData<T> Data { get; set; } = null!;
    }

    private class AfdianResponseData<T>
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
        [JsonPropertyName("total_page")]
        public int TotalPage { get; set; }
        public T[] List { get; set; } = [];
    }

    private class AfdianRequestBody
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
        public string Params { get; set; } = string.Empty;
        public long Ts { get; set; }
        public string Sign { get; set; } = string.Empty;
    }

    private enum AfdianOrderStatus
    {
        Paid = 2,
    }

    private class AfdianOrder
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
        [JsonPropertyName("total_amount")]
        public string TotalAmount { get; set; } = "0";
        public int Month { get; set; }
        public AfdianOrderStatus Status { get; set; }
        [JsonPropertyName("create_time")]
        public long CreateTime { get; set; }
    }
}
