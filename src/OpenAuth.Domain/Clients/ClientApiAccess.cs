using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients;

public sealed class ClientApiAccess
{
    public ApiResourceId ApiResourceId { get; private init; }
    public ScopeCollection AllowedScopes { get; private init; }
    
    private ClientApiAccess() { }

    private ClientApiAccess(ApiResourceId apiResourceId, ScopeCollection allowedScope)
    {
        ApiResourceId = apiResourceId;
        AllowedScopes = allowedScope;
    }

    internal static ClientApiAccess Create(ApiResourceId apiResourceId, ScopeCollection allowedScope) =>
        new ClientApiAccess(apiResourceId, allowedScope);
}