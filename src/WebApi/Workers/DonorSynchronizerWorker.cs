using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crpg.Application.Users.Commands;
using Mediator;

namespace Crpg.WebApi.Workers;

internal class DonorSynchronizerWorker : BackgroundService
{
    private const int MinPatreonAmountCentsForRewards = 500;

    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<DonorSynchronizerWorker>();

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

                string[] steamIds = patreonDonors.ToArray();

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
            return Array.Empty<string>();
        }

        int campaignId = _configuration.GetValue<int>("Patreon:CampaignId");
        using HttpClient client = new() { BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/") };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", patreonAccessToken);

        string uri = $"campaigns/{campaignId}/members?fields%5Bmember%5D=currently_entitled_amount_cents,note&page%5Bcount%5D=1000";
        var res = await client.GetFromJsonAsync<PatreonResponse<PatreonCampaignMember>>(uri, cancellationToken);

        List<string> steamIds = new();
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

    private class PatreonCampaignMember
    {
        [JsonPropertyName("currently_entitled_amount_cents")]
        public int CurrentlyEntitledAmountCents { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    private class PatreonResponse<T>
    {
        public PatreonResponseData<T>[] Data { get; set; } = Array.Empty<PatreonResponseData<T>>();
    }

    private class PatreonResponseData<T>
    {
        public Guid Id { get; set; }
        public T Attributes { get; set; } = default!;
        public string Type { get; set; } = string.Empty;
    }
}
