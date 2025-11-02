using System.Collections.Immutable;
using OpenAuth.Domain.Clients.Audiences;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record OAuthConfiguration(
    IReadOnlyCollection<Audience> Audiences,
    IReadOnlyCollection<GrantType> GrantTypes,
    IReadOnlyCollection<RedirectUri> RedirectUris,
    bool RequirePkce)
{
    public OAuthConfiguration AddAudiences(params Audience[] audiences)
        => this with {
            Audiences = Audiences
                .Union(audiences)
                .ToImmutableArray()
        };

    public OAuthConfiguration AddGrantTypes(params GrantType[] grantTypes)
        => this with {
            GrantTypes = GrantTypes
                .Union(grantTypes)
                .ToImmutableArray()
        };

    public OAuthConfiguration AddRedirectUris(params RedirectUri[] redirectUris)
        => this with
        {
            RedirectUris = RedirectUris
                .Union(redirectUris)
                .ToImmutableArray()
        };

    public OAuthConfiguration SetRequirePkce(bool requirePkce)
        => this with { RequirePkce = requirePkce };
}