using Crpg.Application.ActivityLogs.Models;

public record ActivityLogWithDictViewModel
{
    public IList<ActivityLogViewModel> ActivityLogs { get; init; } = Array.Empty<ActivityLogViewModel>();
    public ActivityLogMetadataEntitiesDictViewModel Dict { get; init; } = new();
}
