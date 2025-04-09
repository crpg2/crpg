using Crpg.Application.Characters.Models;
using Crpg.Application.Clans.Models;
using Crpg.Application.Users.Models;

namespace Crpg.Application.Notifications.Models;

public record UserNotificationMetadataEntitiesDictViewModel
{
    public IList<ClanPublicViewModel> Clans { get; init; } = Array.Empty<ClanPublicViewModel>();
    public IList<UserPublicViewModel> Users { get; init; } = Array.Empty<UserPublicViewModel>();
    public IList<CharacterPublicViewModel> Characters { get; init; } = Array.Empty<CharacterPublicViewModel>();
}
