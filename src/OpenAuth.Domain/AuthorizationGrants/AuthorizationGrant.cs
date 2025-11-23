using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.AuthorizationGrants;

public class AuthorizationGrant
{
    public GrantType GrantType { get; }
    public ClientId ClientId { get; }
    public string Subject { get; }
    public RedirectUri RedirectUri { get; }
    public ScopeCollection GrantedScopes { get; }
    
    public string Code { get; }
    public Pkce? Pkce { get; }
    
    public string? Nonce { get; }
    
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset ExpiresAt { get; }

    public bool Consumed { get; private set; }

    private AuthorizationGrant(
        string code,
        GrantType grantType,
        ClientId clientId,
        string subject,
        RedirectUri redirectUri,
        ScopeCollection grantedScopes,
        Pkce? pkce,
        string? nonce,
        DateTimeOffset utcNow)
    {
        GrantType = grantType;
        ClientId = clientId;
        Subject = subject;
        
        RedirectUri = redirectUri;
        GrantedScopes = grantedScopes;
        
        Code = code;
        Pkce = pkce;
        Nonce = nonce;
        
        CreatedAt = utcNow;
        ExpiresAt = utcNow.AddMinutes(10);
    }

    public static AuthorizationGrant Create(
        string code,
        GrantType grantType,
        string subject,
        ClientId clientId,
        RedirectUri redirectUri,
        ScopeCollection scopes,
        Pkce? pkce,
        string? nonce,
        DateTimeOffset utcNow)
    {
        return new AuthorizationGrant(
            code,
            grantType,
            clientId,
            subject,
            redirectUri,
            scopes,
            pkce,
            nonce,
            utcNow
        );
    }

    public void Consume() => Consumed = true;
    public bool IsExpired(DateTimeOffset utcNow) => utcNow >= ExpiresAt;
}