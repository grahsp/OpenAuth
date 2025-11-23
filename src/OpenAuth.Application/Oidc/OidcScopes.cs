using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public static class OidcScopes
{
    public static readonly Scope OpenId = new("openid");
    public static readonly Scope Profile = new("profile");
    public static readonly Scope Email = new("email");
    public static readonly Scope Phone = new("phone");
    public static readonly Scope Address = new("address");

    public static readonly ScopeCollection All =
        new([OpenId, Profile, Email, Phone, Address]);

    
    public static bool ContainsOpenIdScope(this ScopeCollection collection) =>
        collection.Contains(OpenId);
    
    public static ScopeCollection GetOidcScopes(this ScopeCollection collection)
    {
        var scopes = collection
            .Where(s => All.Contains(s));

        return new ScopeCollection(scopes);
    }

    public static ScopeCollection GetApiScopes(this ScopeCollection collection)
    {
        var scopes = collection
            .Where(s => !All.Contains(s));

        return new ScopeCollection(scopes);       
    }
}