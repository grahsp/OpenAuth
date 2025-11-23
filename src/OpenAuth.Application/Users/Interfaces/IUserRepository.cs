using OpenAuth.Domain.Users;

namespace OpenAuth.Application.Users.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    void Add(User user);
    void Remove(User user);
    Task SaveChangesAsync(CancellationToken ct = default);
}