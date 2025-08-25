using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Tokens;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _issuer;
    private readonly ISigningCredentialsFactory _factory;
    
    public JwtTokenGenerator(string issuer, ISigningCredentialsFactory factory)
    {
        _issuer = issuer;
        _factory = factory;
    }

    public string GenerateToken(Client client, IEnumerable<Scope> scopes, SigningKey signingKey)
    {
        if (!signingKey.IsActive())
            throw new InvalidOperationException("Signing key is expired or revoked.");
        
        var signingCredentials = _factory.Create(signingKey);
        var now = DateTime.UtcNow;
        var expires = now.Add(client.TokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString()),
        };
        
        foreach (var aud in client.GetAudiences())
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud.Value));
        
        foreach (var scope in scopes)
            claims.Add(new Claim("scope", scope.Value));

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