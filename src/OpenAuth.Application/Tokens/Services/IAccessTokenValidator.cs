using System.Security.Claims;

namespace OpenAuth.Application.Tokens.Services;

public interface IAccessTokenValidator
{
    Task<ClaimsPrincipal?> ValidateAsync(string token);
}