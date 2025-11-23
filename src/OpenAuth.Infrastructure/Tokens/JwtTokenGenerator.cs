using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Interfaces;
using TokenContext = OpenAuth.Application.Tokens.Dtos.TokenContext;

namespace OpenAuth.Infrastructure.Tokens;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _config;
    private readonly ISigningCredentialsFactory _factory;
    private readonly TimeProvider _time;
    
    public JwtTokenGenerator(IOptions<JwtOptions> config, ISigningCredentialsFactory factory, TimeProvider time)
    {
        _config = config.Value;
        _factory = factory;
        _time = time;
    }

    public string GenerateToken(TokenContext context, ClientTokenData tokenData, SigningKeyData keyData)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        
        var signingCredentials = _factory.Create(keyData);
        var expires = now.Add(tokenData.TokenLifetime);

        var claims = new List<Claim>
        {
            new("client_id", context.ClientId.ToString()),
            new(JwtRegisteredClaimNames.Sub, context.Subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString()),
        };

        if (context.RequestedAudience is not null)
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, context.RequestedAudience.Value));
            
        if (context.RequestedScopes is not null)
            claims.AddRange(context.RequestedScopes.Select(scope => new Claim("scope", scope.Value)));

        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: signingCredentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}