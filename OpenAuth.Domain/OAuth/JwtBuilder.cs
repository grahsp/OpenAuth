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

    public JwtBuilder WithAudiences(params AudienceName[] audiences)
    {
        foreach (var audience in audiences)
            WithClaim(OAuthClaimTypes.Aud, audience.Value);

        return this;
    }

    public JwtBuilder WithScopes(params Scope[] scopes)
    {
        foreach (var scope in scopes)
            WithClaim(OAuthClaimTypes.Scope, scope.Value);
        
        return this;
    }

    public JwtBuilder WithClaim(string type, string value)
    {
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

        return new JwtDescriptor(
            _issuer,
            _claims, 
            now, 
            now.Add(_lifetime.Value),
            now
        );
    }
}