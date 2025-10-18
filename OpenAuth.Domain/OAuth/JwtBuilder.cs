using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.OAuth;

public class JwtBuilder
{
    private readonly string _issuer;
    private TimeSpan? _lifetime;
    private readonly List<Claim> _claims = [];

    public JwtBuilder(string issuer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        _issuer = issuer;
    }

    public JwtBuilder WithClient(ClientId id)
    {
        if (_claims.Any(c => c.Type == OAuthClaimTypes.ClientId))
            throw new InvalidOperationException("Client ID claim already set.");
        
        WithClaim(OAuthClaimTypes.ClientId, id.ToString());
        return this;
    }

    public JwtBuilder WithSubject(string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        
        if (_claims.Any(c => c.Type == OAuthClaimTypes.Sub))
            throw new InvalidOperationException("Subject claim already set.");
        
        WithClaim(OAuthClaimTypes.Sub, subject);
        return this;
    }

    public JwtBuilder WithAudience(AudienceName audience)
    {
        ArgumentNullException.ThrowIfNull(audience);
        
        if (_claims.Any(c => c.Type == OAuthClaimTypes.Aud))
            throw new InvalidOperationException("Audience claim already set.");
        
        WithClaim(OAuthClaimTypes.Aud, audience.Value);
        return this;
    }

    public JwtBuilder WithScopes(params Scope[] scopes)
    {
        foreach (var scope in scopes)
        {
            ArgumentNullException.ThrowIfNull(scope);
            WithClaim(OAuthClaimTypes.Scope, scope.Value);
        }
        
        return this;
    }

    public JwtBuilder WithClaim(string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        
        _claims.Add(new Claim(type, value));
        return this;
    }

    public JwtBuilder WithLifetime(TimeSpan lifetime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero, nameof(lifetime));
        
        if (_lifetime is not null)
            throw new InvalidOperationException("Lifetime already set.");
        
        _lifetime = lifetime;
        return this;
    }

    public JwtDescriptor Build(TimeProvider timeProvider)
    {
        var now = timeProvider.GetUtcNow();
        
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
        
        ValidateSingletonClaim(claims, OAuthClaimTypes.ClientId, "Client ID");
        ValidateSingletonClaim(claims, OAuthClaimTypes.Aud, "Audience");
        ValidateSingletonClaim(claims, OAuthClaimTypes.Sub, "Subject");
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