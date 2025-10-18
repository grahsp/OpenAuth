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
        WithClaim(OAuthClaimTypes.ClientId, id.ToString());
        return this;
    }

    public JwtBuilder WithSubject(string subject)
    {
        WithClaim(OAuthClaimTypes.Sub, subject);
        return this;
    }

    public JwtBuilder WithAudience(AudienceName audience)
    {
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
            new(OAuthClaimTypes.Iat, EpochTime.GetIntDate(now.UtcDateTime).ToString()),
            new(OAuthClaimTypes.Jti, Guid.NewGuid().ToString())
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
        if (!claims.Any(c => c.Type == OAuthClaimTypes.ClientId))
            throw new InvalidOperationException("Client ID is required.");
        
        if (!claims.Any(c => c.Type == OAuthClaimTypes.Sub))
            throw new InvalidOperationException("Subject claim is required.");
        
        if (!claims.Any(c => c.Type == OAuthClaimTypes.Aud))
            throw new InvalidOperationException("At least one audience is required.");
    }
}