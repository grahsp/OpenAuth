using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Tokens;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _issuer;
    private readonly ISigningCredentialsFactory _factory;
    
    public JwtTokenGenerator(string issuer, ISigningCredentialsFactory factory)
    {
        _issuer = issuer;
        _factory = factory;
    }

    public string GenerateToken(Client client, Audience audience, IEnumerable<Scope> scopes, SigningKey signingKey)
    {
        if (!signingKey.IsActive())
            throw new InvalidOperationException("Signing key is expired or revoked.");

        if (!client.GetAudiences().Contains(audience))
            throw new InvalidOperationException("Audience doesn't exist.");
        
        var signingCredentials = _factory.Create(signingKey);
        var now = DateTime.UtcNow;
        var expires = now.Add(client.TokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString()),
            new(JwtRegisteredClaimNames.Aud, audience.ToString())
        };

        var allowedScopes = client.GetAllowedScopes(audience).ToImmutableHashSet();
        var invalidScopes = new List<string>();
        foreach (var scope in scopes)
        {
            if (!allowedScopes.Contains(scope))
                invalidScopes.Add(scope.Value);
            else
                claims.Add(new Claim("scope", scope.Value));
        }

        if (invalidScopes.Count > 0)
            throw new InvalidOperationException($"Invalid scopes requested: { string.Join(", ", invalidScopes) }.");

        var token = new JwtSecurityToken(
            issuer: _issuer,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: signingCredentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}