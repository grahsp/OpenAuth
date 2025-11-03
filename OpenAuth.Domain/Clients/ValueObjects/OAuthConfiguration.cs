using System.Collections.Immutable;
using OpenAuth.Domain.Clients.Audiences;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record OAuthConfiguration
{
    public IReadOnlyCollection<Audience> Audiences { get; private init; }
    public IReadOnlyCollection<GrantType> GrantTypes { get; private init; }
    public IReadOnlyCollection<RedirectUri> RedirectUris { get; private init; }
    public bool RequirePkce { get; private init; }
    
    public OAuthConfiguration(
        IEnumerable<Audience> audiences,
        IEnumerable<GrantType> grantTypes,
        IEnumerable<RedirectUri> redirectUris,
        bool requirePkce)
    {
        Audiences = audiences.ToImmutableList();
        GrantTypes = grantTypes.ToImmutableList();
        RedirectUris = redirectUris.ToImmutableList();
        RequirePkce = requirePkce;
    }
    
    public OAuthConfiguration SetAudiences(IEnumerable<Audience> audiences)
    {
        ArgumentNullException.ThrowIfNull(audiences);

        var items = audiences.DistinctBy(a => a.Name).ToArray();
        
        if (items.Length == 0)
            throw new InvalidOperationException("Client must have at least one audience.");

        if (Audiences.SequenceEqual(items))
            return this;
        
        return this with { Audiences = items.ToImmutableList() };
    }

    public OAuthConfiguration SetGrantTypes(IEnumerable<GrantType> grantTypes)
    {
        ArgumentNullException.ThrowIfNull(grantTypes);
        
        var grants = grantTypes.Distinct().ToArray();
        
        if (grants.Length == 0)
            throw new InvalidOperationException("Client must have at least one grant type.");

        var allPublic= grants.All(gt => gt.SupportsPublicClient);
        var allConfidential= grants.All(gt => !gt.SupportsPublicClient);
        var includesAuthCode = grants.Contains(GrantType.AuthorizationCode);
        
        var isMixed = !allPublic && !allConfidential;
        var isAuthCodePublic = includesAuthCode && RequirePkce;
        
        if (isMixed && !isAuthCodePublic)
            throw new InvalidOperationException("Cannot mix public and confidential grant types.");
        
        var requiresRedirectUri = grants.Any(gt => gt.RequiresRedirectUri);
        
        if (requiresRedirectUri && RedirectUris.Count == 0)
            throw new InvalidOperationException("Client must have at least one redirect URI.");
        
        return this with { GrantTypes = grants.ToImmutableList() };
    }

    public OAuthConfiguration SetRedirectUris(IEnumerable<RedirectUri> redirectUris)
    {
        ArgumentNullException.ThrowIfNull(redirectUris);
        
        var uris = redirectUris.Distinct().ToArray();
        
        if (uris.Length == 0 && GrantTypes.Any(gt => gt.RequiresRedirectUri))
            throw new InvalidOperationException("Client must have at least one redirect URI.");
        
        return this with { RedirectUris = uris.ToImmutableList() };
    }

    public OAuthConfiguration SetRequirePkce(bool requirePkce)
    {
        var allPublic = GrantTypes.All(gt => gt.SupportsPublicClient);
        var includesAuthCode = GrantTypes.Contains(GrantType.AuthorizationCode);
        
        if (!requirePkce && allPublic && includesAuthCode)
            throw new InvalidOperationException("Cannot disable PKCE for public clients.");
        
        if (RequirePkce == requirePkce)
            return this;
        
        return this with { RequirePkce = requirePkce };
    }
}