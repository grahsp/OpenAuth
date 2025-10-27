using OpenAuth.Application.Shared.Models;
using OpenAuth.Domain.Users;

namespace OpenAuth.Application.Users.Services;

public interface IUserService
{
    Task<Result<User>> RegisterUserAsync(string username, string email, string password, CancellationToken ct = default);
}