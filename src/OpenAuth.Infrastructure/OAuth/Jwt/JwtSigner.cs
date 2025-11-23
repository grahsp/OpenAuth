using System.IdentityModel.Tokens.Jwt;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Infrastructure.OAuth.Jwt;

public class JwtSigner : IJwtSigner
{
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    
    public JwtSigner(ISigningKeyQueryService keyService, ISigningCredentialsFactory credentialsFactory)
    {
        _keyService = keyService;
        _credentialsFactory = credentialsFactory;
    }


    public async Task<string> Create(JwtDescriptor descriptor, CancellationToken ct = default)
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

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}