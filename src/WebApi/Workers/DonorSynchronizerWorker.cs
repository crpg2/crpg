using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Crpg.Application.Users.Commands;
using Mediator;

namespace Crpg.WebApi.Workers;

internal class DonorSynchronizerWorker : BackgroundService
{
    private const int MinPatreonAmountCentsForRewards = 500;
    private const decimal MinAfdianAmountYuanPerMonth = 25m;

    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<DonorSynchronizerWorker>();
    private static readonly Regex SteamIdRegex = new(@"76561198\d{9}", RegexOptions.Compiled);

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DonorSynchronizerWorker(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

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

    private async Task<IEnumerable<string>> GetPatreonDonorsAsync(CancellationToken cancellationToken)
    {
        string? patreonAccessToken = _configuration.GetValue<string>("Patreon:AccessToken");
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
