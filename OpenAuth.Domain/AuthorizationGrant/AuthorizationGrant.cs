using OpenAuth.Domain.AuthorizationGrant.ValueObjects;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.AuthorizationGrant;

public class AuthorizationGrant
{
    public GrantType GrantType { get; }
    public ClientId ClientId { get; }
    public RedirectUri RedirectUri { get; }
    public AudienceName Audience { get; }
    public Scope[] Scopes { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    
    public string Code { get; }
    public Pkce? Pkce { get; }
    
    public bool Consumed { get; private set; }

    private AuthorizationGrant(
        string code,
        GrantType grantType,
        ClientId clientId,
        RedirectUri redirectUri,
        AudienceName audience,
        IEnumerable<Scope> scopes,
        DateTimeOffset utcNow,
        Pkce? pkce = null)
    {
        GrantType = grantType;
        ClientId = clientId;
        RedirectUri = redirectUri;
        Audience = audience;
        Scopes = scopes.ToArray();
        
        Code = code;
        Pkce = pkce;
        
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(10);
    }

    public static AuthorizationGrant Create(
        string code,
        GrantType grantType,
        ClientId clientId,
        RedirectUri redirectUri,
        AudienceName audience,
        IEnumerable<Scope> scopes,
        DateTimeOffset utcNow,
        Pkce? pkce = null)
    {
        return new AuthorizationGrant(
            code,
            grantType,
            clientId,
            redirectUri,
            audience,
            scopes,
            utcNow,
            pkce
        );
    }
    

    public void Consume() => Consumed = true;

    public bool IsExpired(DateTimeOffset utcNow) => utcNow >= ExpiresAt;
}