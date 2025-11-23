using System.IdentityModel.Tokens.Jwt;
using OpenAuth.Application.OAuth;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Infrastructure.OAuth.Jwt;

public class JwtFactory : IJwtFactory
{
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    
    public JwtFactory(ISigningKeyQueryService keyService, ISigningCredentialsFactory credentialsFactory)
    {
        _keyService = keyService;
        _credentialsFactory = credentialsFactory;
    }


    public async Task<AccessTokenResult> Create(JwtDescriptor descriptor, CancellationToken ct = default)
    {
        var key = await _keyService.GetCurrentKeyDataAsync(ct)
            ?? throw new InvalidOperationException("No active signing key found.");
        
        var signingCredentials = _credentialsFactory.Create(key);
        
        var jwt = new JwtSecurityToken(
            issuer: descriptor.Issuer,
            claims: descriptor.Claims,
            notBefore: descriptor.NotBefore.DateTime,
            expires: descriptor.ExpiresAt.DateTime,
            signingCredentials: signingCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new AccessTokenResult(token, TokenType.Bearer, (int)descriptor.Lifetime.TotalSeconds);
    }
}