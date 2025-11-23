using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OpenAuth.Application.Oidc;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Users;

namespace OpenAuth.Infrastructure.Identity;

public class OidcUserClaimsProvider : IUserClaimsQueryService
{
    private readonly UserManager<User> _users;
    
    public OidcUserClaimsProvider(UserManager<User> users)
    {
        _users = users;
    }

    public async Task<IEnumerable<Claim>> GetUserClaimsAsync(string subjectId, ScopeCollection scopes)
    {
        var user = await _users.FindByIdAsync(subjectId)
                   ?? throw new InvalidOperationException("User not found.");

        var claims = new List<Claim>();

        if (scopes.Contains(OidcScopes.Profile))
        {
            AddClaim(claims, "preferred_username", user.UserName);
        }
        
        if (scopes.Contains(OidcScopes.Email))
        {
            AddClaim(claims, "email", user.Email);
            AddClaim(claims, "email_verified", user.EmailConfirmed);
        }
        
        if (scopes.Contains(OidcScopes.Phone))
        {
            AddClaim(claims, "phone_number", user.PhoneNumber);
            AddClaim(claims, "phone_number_verified", user.PhoneNumberConfirmed);
        }

        return claims;
    }

    private static void AddClaim(List<Claim> claims, string type, string? value)
    {
        ArgumentNullException.ThrowIfNull(type);
        
        if (!string.IsNullOrWhiteSpace(value))
            claims.Add(new Claim(type, value));
    }

    private static void AddClaim(List<Claim> claims, string type, bool value)
    {
        AddClaim(claims, type, value ? "true" : "false");
    }
}