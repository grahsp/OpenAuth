using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OpenAuth.Application.Extensions;
using OpenAuth.Application.OAuth.Exceptions;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Services;

public class UserInfoService : IUserInfoService
{
    private readonly IAccessTokenValidator _validator;
    private readonly IOidcUserClaimsProvider _claimsProvider;
    
    public UserInfoService(IAccessTokenValidator validator, IOidcUserClaimsProvider claimsProvider)
    {
        _validator = validator;
        _claimsProvider = claimsProvider;
    }

    public async Task<IReadOnlyCollection<Claim>> GetUserClaimsAsync(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        
        var principal = await _validator.ValidateAsync(token);

        var scope = GetScope(principal);
        var subject = GetSubject(principal);
        
        return await _claimsProvider.GetUserClaimsAsync(subject, scope);
    }

    private static ScopeCollection GetScope(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirst("scope")?.Value
            ?? principal.FindFirst("scp")?.Value;

        if (!ScopeCollection.TryParse(raw, out var scope))
            throw new UserInfoAccessDeniedException("invalid scope");
        
        if (!scope.ContainsOpenIdScope())
            throw new UserInfoAccessDeniedException("invalid scope");
        
        return scope;
    }

    private static string GetSubject(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(subject))
            throw new UserInfoAccessDeniedException("invalid subject");
        
        return subject;
    }
}