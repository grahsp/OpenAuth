using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.OAuth;

namespace OpenAuth.Application.Tokens.Builders;

public class JwtBuilder
{
    private readonly string _issuer;
    private readonly List<Claim> _claims = [];
    private TimeSpan? _lifetime;
    
    private readonly TimeProvider _time;

    public JwtBuilder(string issuer, TimeProvider time)
    {
        _issuer = issuer;
        _time = time;
    }

    public JwtBuilder AddClaim(string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return AddOptionalClaim(type, value);
    }

    public JwtBuilder AddOptionalClaim(string type, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            _claims.Add(new Claim(type, value));
        
        return this;
    }

    public JwtBuilder AddClaim(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        
        _claims.Add(claim);
        return this;
    }
    
    public JwtBuilder AddClaims(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);
        
        _claims.AddRange(claims);
        return this;
    }

    public JwtBuilder WithLifetime(TimeSpan lifetime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);
        
        _lifetime = lifetime;
        return this;
    }

    public JwtDescriptor Build()
    {
        var now = _time.GetUtcNow();
        
        _lifetime ??= TimeSpan.FromMinutes(30);

        var claims = new List<Claim>(_claims)
        {
            new(OAuthClaimTypes.Iss, _issuer),
            new(OAuthClaimTypes.Jti, Guid.NewGuid().ToString()),
            new(OAuthClaimTypes.Iat, EpochTime.GetIntDate(now.UtcDateTime).ToString()),
            new(OAuthClaimTypes.Exp, EpochTime.GetIntDate(now.UtcDateTime.Add(_lifetime.Value)).ToString()),
            new(OAuthClaimTypes.Nbf, EpochTime.GetIntDate(now.UtcDateTime).ToString())
        };

        ValidateClaims(claims);
        
        return new JwtDescriptor(
            _issuer,
            claims, 
            now, 
            now.Add(_lifetime.Value),
            now
        );
    }

    private static void ValidateClaims(IReadOnlyCollection<Claim> claims)
    {
        ValidateSingletonClaim(claims, OAuthClaimTypes.Iss, "Issuer");
        ValidateSingletonClaim(claims, OAuthClaimTypes.Jti, "JWT ID");
        ValidateSingletonClaim(claims, OAuthClaimTypes.Iat, "Issued At");
        ValidateSingletonClaim(claims, OAuthClaimTypes.Exp, "Expiration");
        ValidateSingletonClaim(claims, OAuthClaimTypes.Nbf, "Not Before");
    }

    private static void ValidateSingletonClaim(IReadOnlyCollection<Claim> claims, string claimType, string claimName)
    {
        var count = claims.Count(c => c.Type == claimType);

        var message = count switch
        {
            0 => $"Claim '{claimName}' is required.",
            > 1 => $"Only one '{claimName}' claim is allowed.",
            _ => null
        };
        
        if (message is not null)
            throw new InvalidOperationException(message);
    }
}