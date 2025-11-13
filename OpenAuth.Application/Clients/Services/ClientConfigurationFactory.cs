using System.Collections.Immutable;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public class ClientConfigurationFactory : IClientConfigurationFactory
{
    public ClientConfiguration Create(RegisterClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var name = ClientName.Create(request.Name);

        var audiences = CreateAudiences(request.Permissions);
        var redirectUris = CreateRedirectUris(request.RedirectUris);

        var applicationType = ClientApplicationTypes.Parse(request.ApplicationType);
        
        var configuration = new ClientConfiguration(name,
            applicationType,
            audiences,
            applicationType.DefaultGrantTypes,
            redirectUris
        );
        return configuration;
    }

    private static ImmutableArray<Audience> CreateAudiences(Dictionary<string, IEnumerable<string>>? permissions)
        => permissions?.Select(x =>
        {
            var scopes = x.Value.Select(s => new Scope(s));
            var scopeCollection = new ScopeCollection(scopes);

            var audienceName = AudienceName.Create(x.Key);
            var audience = new Audience(audienceName, scopeCollection);

            return audience;
        }).ToImmutableArray() ?? [];

    private static ImmutableArray<RedirectUri> CreateRedirectUris(IEnumerable<string>? redirectUris)
        => redirectUris?.Select(RedirectUri.Create).ToImmutableArray() ?? [];
}