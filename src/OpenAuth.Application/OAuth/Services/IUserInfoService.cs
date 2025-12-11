using System.Security.Claims;

namespace OpenAuth.Application.OAuth.Services;

public interface IUserInfoService
{
    Task<IReadOnlyCollection<Claim>> GetUserClaimsAsync(string token);
}