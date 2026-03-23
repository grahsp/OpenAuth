using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients;

public sealed class ClientApiAccess
{
    public ApiResourceId ApiResourceId { get; private init; }
    public ScopeCollection AllowedScopes { get; private set; }
    
    private ClientApiAccess() { }

    private ClientApiAccess(ApiResourceId apiResourceId, ScopeCollection allowedScope)
    {
        ApiResourceId = apiResourceId;
        AllowedScopes = allowedScope;
    }

    internal static ClientApiAccess Create(ApiResourceId apiResourceId, ScopeCollection allowedScope) =>
        new ClientApiAccess(apiResourceId, allowedScope);

    public void SetScopes(ScopeCollection scopes)
    {
        if (scopes.IsEmpty)
            throw new InvalidOperationException("Scopes cannot be empty");
        
        AllowedScopes = scopes;
    }
}