using System.Collections.Immutable;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.Enums;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Clients.Services;

public class ClientConfigurationFactory : IClientConfigurationFactory
{
    public ClientConfiguration Create(RegisterClientRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var name = ClientName.Create(request.Name);
        var security = new SecuritySettings();

        var audiences = CreateAudiences(request.Permissions);
        var redirectUris = CreateRedirectUris(request.RedirectUris);
        
        var oauth = request.Type switch
        {
            ClientApplicationType.SinglePageApplication => OAuthConfiguration.CreateSpa(audiences, redirectUris),
            ClientApplicationType.MachineToMachine => OAuthConfiguration.CreateM2M(audiences),
            _ => throw new NotSupportedException($"Unsupported application type: {request.Type}.")
        };
        
        var configuration = new ClientConfiguration(name, security, oauth);
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