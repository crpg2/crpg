using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Items;

public class UserItemPreset : AuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    public User? User { get; set; }
    public IList<UserItemPresetSlot> Slots { get; set; } = new List<UserItemPresetSlot>();
}
