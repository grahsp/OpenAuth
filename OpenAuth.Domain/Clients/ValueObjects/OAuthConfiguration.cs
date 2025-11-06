using System.Collections.Immutable;
using OpenAuth.Domain.Clients.Audiences;
using OpenAuth.Domain.Shared;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record OAuthConfiguration
{
    public IReadOnlyCollection<NewAudience> Audiences { get; private init; }
    public IReadOnlyCollection<GrantType> GrantTypes { get; private init; }
    public IReadOnlyCollection<RedirectUri> RedirectUris { get; private init; }
    public bool RequirePkce { get; private init; }
    
    public OAuthConfiguration(
        IEnumerable<NewAudience> audiences,
        IEnumerable<GrantType> grantTypes,
        IEnumerable<RedirectUri> redirectUris,
        bool requirePkce)
    {
        Audiences = audiences.CreateDistinctCollection(a => a.Name);
        GrantTypes = grantTypes.CreateDistinctCollection();
        RedirectUris = redirectUris.CreateDistinctCollection();
        RequirePkce = requirePkce;
        
        DomainRules.EnsureCollectionNotEmpty(Audiences, DomainErrors.OAuth.AudienceRequired);
        DomainRules.EnsureCollectionNotEmpty(GrantTypes, DomainErrors.OAuth.GrantTypeRequired);
        
        EnsureGrantTypeConsistency(GrantTypes);
        EnsureRedirectUrisForGrantTypes(GrantTypes, RedirectUris);
        EnsurePkceCompliance(RequirePkce);
    }
    
    
    public bool IsPublicClient() => IsPublicClient(GrantTypes, RequirePkce);
    public bool RequiresRedirectUri() => RequiresRedirectUri(GrantTypes);
    public bool CanDisablePkce() => !IsPublicClient();
    
    private static bool IsPublicClient(IEnumerable<GrantType> grantTypes, bool requirePkce)
        => grantTypes.All(gt => gt.IsPublic(requirePkce));
    
    private static bool RequiresRedirectUri(IEnumerable<GrantType> grantTypes)
        => grantTypes.Any(gt => gt.RequiresRedirectUri);
    
    
    public OAuthConfiguration SetAudiences(IEnumerable<NewAudience> audiences)
    {
        var items = audiences.CreateDistinctCollection(a => a.Name);
        DomainRules.EnsureCollectionNotEmpty(items, DomainErrors.OAuth.AudienceRequired);

        if (Audiences.SequenceEqual(items))
            return this;
        
        return this with { Audiences = items.ToImmutableList() };
    }

    public OAuthConfiguration SetGrantTypes(IEnumerable<GrantType> grantTypes)
    {
        var grants = grantTypes.CreateDistinctCollection();
        DomainRules.EnsureCollectionNotEmpty(grants, DomainErrors.OAuth.GrantTypeRequired);
        
        EnsureGrantTypeConsistency(grants);
        EnsureRedirectUrisForGrantTypes(grants, RedirectUris);
        
        if (GrantTypes.SequenceEqual(grants))
            return this;
        
        return this with { GrantTypes = grants.ToImmutableList() };
    }

    private void EnsureGrantTypeConsistency(IReadOnlyCollection<GrantType> grants)
    {
        var allPublic = IsPublicClient(grants, RequirePkce);
        var allConfidential= grants.All(gt => gt.IsConfidential());
        
        if (!allPublic && !allConfidential)
            throw new InvalidOperationException("Cannot mix public and confidential grant types.");
    }

    public OAuthConfiguration SetRedirectUris(IEnumerable<RedirectUri> redirectUris)
    {
        var uris = redirectUris.CreateDistinctCollection();
        
        EnsureRedirectUrisForGrantTypes(GrantTypes, uris);

        if (RedirectUris.SequenceEqual(uris))
            return this;
        
        return this with { RedirectUris = uris.ToImmutableList() };
    }

    private static void EnsureRedirectUrisForGrantTypes(IEnumerable<GrantType> grants, IEnumerable<RedirectUri> uris)
    {
        if (!uris.Any() && RequiresRedirectUri(grants))
            throw new InvalidOperationException("Client must have at least one redirect URI with the current grant types.");
    }

    public OAuthConfiguration SetRequirePkce(bool requirePkce)
    {
        EnsurePkceCompliance(requirePkce);
        
        if (RequirePkce == requirePkce)
            return this;
        
        return this with { RequirePkce = requirePkce };
    }

    private void EnsurePkceCompliance(bool requirePkce)
    {
        if (!requirePkce && !CanDisablePkce())
            throw new InvalidOperationException("Cannot disable PKCE for public clients.");
    }
}