using Crpg.Application.ActivityLogs.Models;

public record ActivityLogWithDictViewModel
{
    public IList<ActivityLogViewModel> ActivityLogs { get; init; } = Array.Empty<ActivityLogViewModel>();
    public ActivityLogMetadataEnrichedViewModel Dict { get; init; } = new();
}
