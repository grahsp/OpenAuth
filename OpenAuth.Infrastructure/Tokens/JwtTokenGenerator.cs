using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Domain.Configurations;

namespace OpenAuth.Infrastructure.Tokens;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly Auth _config;
    private readonly ISigningCredentialsFactory _factory;
    private readonly TimeProvider _time;
    
    public JwtTokenGenerator(IOptions<Auth> config, ISigningCredentialsFactory factory, TimeProvider time)
    {
        _config = config.Value;
        _factory = factory;
        _time = time;
    }

    public string GenerateToken(TokenGenerationRequest request)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        
        var signingCredentials = _factory.Create(request.KeyData);
        var expires = now.Add(request.TokenLifetime);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.ClientId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString()),
            new(JwtRegisteredClaimNames.Aud, request.AudienceName.Value)
        };
        
        claims.AddRange(request.Scopes.Select(scope => new Claim("scope", scope.Value)));

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