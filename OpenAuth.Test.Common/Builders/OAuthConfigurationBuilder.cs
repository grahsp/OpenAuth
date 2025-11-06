using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class OAuthConfigurationBuilder
{
    private IEnumerable<Audience>? _audiences;
    private IEnumerable<GrantType>? _grantTypes;
    private IEnumerable<RedirectUri>? _redirectUris;
    private bool? _requirePkce;

    public OAuthConfigurationBuilder WithAudiences(params Audience[] audiences)
    {
        _audiences = audiences;
        return this;
    }
    
    public OAuthConfigurationBuilder WithGrantTypes(params GrantType[] grantTypes)
    {
        _grantTypes = grantTypes;
        return this;
    }
    
    public OAuthConfigurationBuilder WithRedirectUris(params string[] redirectUris)
    {
        _redirectUris = redirectUris.Select(RedirectUri.Create);
        return this;
    }
    
    public OAuthConfigurationBuilder WithRedirectUris(params RedirectUri[] redirectUris)
    {
        _redirectUris = redirectUris;
        return this;
    }
    
    public OAuthConfigurationBuilder WithRequirePkce(bool requirePkce)
    {
        _requirePkce = requirePkce;
        return this;
    }

    public OAuthConfiguration Build()
    {
        var audiences = _audiences
                        ?? [new Audience(AudienceName.Create("api"),
                            new ScopeCollection([]))];
        var grantTypes = _grantTypes
            ?? [GrantType.AuthorizationCode];
        var redirectUris = _redirectUris
            ?? [RedirectUri.Create("https://example.com/callback")];
        var requirePkce = _requirePkce
            ?? true;
        
        return new OAuthConfiguration(audiences, grantTypes, redirectUris, requirePkce);
    }
}