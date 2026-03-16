using OpenAuth.Application.Oidc;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Extensions;

public static class ScopeCollectionExtensions
{
    public static bool ContainsOpenIdScope(this ScopeCollection collection) =>
        collection.Contains(OidcScopes.OpenId);

    public static bool ContainsOidcScopes(this ScopeCollection collection) =>
        collection.Any(s => OidcScopes.All.Contains(s));
    
    public static ScopeCollection GetOidcScopes(this ScopeCollection collection)
    {
        var scopes = collection
            .Where(s => OidcScopes.All.Contains(s));

        return new ScopeCollection(scopes);
    }

    public static ScopeCollection GetApiScopes(this ScopeCollection collection)
    {
        var scopes = collection
            .Where(s => !OidcScopes.All.Contains(s));

        return new ScopeCollection(scopes);       
    }
    
    public static ScopeCollection GetFilteredApiScopes(this ScopeCollection collection, ScopeCollection audienceScopes)
    {
        var scopes = collection
            .Where(s => !OidcScopes.All.Contains(s))
            .Where(audienceScopes.Contains);

        return new ScopeCollection(scopes);
    }
}