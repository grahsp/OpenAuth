using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.AuthorizationGrants;

public class AuthorizationGrant
{
    public GrantType GrantType { get; }
    public ClientId ClientId { get; }
    public string Subject { get; }
    public RedirectUri RedirectUri { get; }
    public AudienceName? Audience { get; }
    public ScopeCollection? Scopes { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    
    public string Code { get; }
    public Pkce? Pkce { get; }
    
    public bool Consumed { get; private set; }

    private AuthorizationGrant(
        string code,
        GrantType grantType,
        ClientId clientId,
        string subject,
        RedirectUri redirectUri,
        AudienceName? audience,
        ScopeCollection? scopes,
        Pkce? pkce,
        DateTimeOffset utcNow)
    {
        GrantType = grantType;
        ClientId = clientId;
        Subject = subject;
        
        RedirectUri = redirectUri;
        Audience = audience;
        Scopes = scopes;
        
        Code = code;
        Pkce = pkce;
        
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(10);
    }

    public static AuthorizationGrant Create(
        string code,
        GrantType grantType,
        string subject,
        ClientId clientId,
        RedirectUri redirectUri,
        AudienceName? audience,
        ScopeCollection? scopes,
        Pkce? pkce,
        DateTimeOffset utcNow)
    {
        return new AuthorizationGrant(
            code,
            grantType,
            clientId,
            subject,
            redirectUri,
            audience,
            scopes,
            pkce,
            utcNow
        );
    }

    public void Consume() => Consumed = true;
    public bool IsExpired(DateTimeOffset utcNow) => utcNow >= ExpiresAt;
}