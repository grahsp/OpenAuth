using System.IdentityModel.Tokens.Jwt;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Infrastructure.OAuth.Jwt;

public interface IJwtFactory
{
    Task<AccessTokenResult> Create(JwtDescriptor descriptor);
}

public class JwtFactory : IJwtFactory
{
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    
    public JwtFactory(ISigningKeyQueryService keyService, ISigningCredentialsFactory credentialsFactory)
    {
        _keyService = keyService;
        _credentialsFactory = credentialsFactory;
    }


    public async Task<AccessTokenResult> Create(JwtDescriptor descriptor)
    {
        var key = await _keyService.GetCurrentKeyDataAsync()
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
        return new AccessTokenResult(token, TokenType.Bearer, descriptor.Lifetime.Seconds);
    }
}