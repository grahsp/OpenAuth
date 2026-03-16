using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Infrastructure.OAuth.Jwt;

public class JwtSigner : IJwtSigner
{
    private readonly string _issuer;
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    private readonly TimeProvider _time;
    
    public JwtSigner(IOptions<JwtOptions> options, ISigningKeyQueryService keyService, ISigningCredentialsFactory credentialsFactory, TimeProvider time)
    {
        _issuer = options.Value.Issuer;
        _keyService = keyService;
        _credentialsFactory = credentialsFactory;
        _time = time;
    }


    public async Task<string> Create(JwtDescriptor descriptor, CancellationToken ct = default)
    {
        var key = await _keyService.GetCurrentKeyDataAsync(ct)
                  ?? throw new InvalidOperationException("No active signing key found.");
        
        var signingCredentials = _credentialsFactory.Create(key);
        
        var now = _time.GetUtcNow().UtcDateTime;

        var payload = new JwtPayload(
            issuer: _issuer,
            audience: descriptor.Audience,
            claims: null,
            notBefore: now,
            expires: now.AddSeconds(descriptor.LifetimeInSeconds),
            issuedAt: now
        );

        foreach (var (k, v) in descriptor.Claims)
            payload[k] = v;
        
        payload[JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString();
        payload[JwtRegisteredClaimNames.Sub] = descriptor.Subject;

        var jwt = new JwtSecurityToken(
            header: new JwtHeader(signingCredentials),
            payload: payload
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}