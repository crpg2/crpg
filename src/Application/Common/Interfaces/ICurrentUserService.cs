using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Common.Interfaces;

public interface ICurrentUserService
{
    public UserClaims? User { get; }
}

public class UserClaims(int id, Role role)
{
    public int Id { get; } = id;
    public Role Role { get; } = role;
}
