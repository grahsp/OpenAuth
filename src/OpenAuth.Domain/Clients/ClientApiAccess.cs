using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients;

public sealed class ClientApiAccess
{
    public ApiId ApiId { get; private init; }
    public ScopeCollection AllowedScopes { get; private init; }
    
    private ClientApiAccess() { }

    private ClientApiAccess(ApiId apiId, ScopeCollection allowedScope)
    {
        ApiId = apiId;
        AllowedScopes = allowedScope;
    }

    internal static ClientApiAccess Create(ApiId apiId, ScopeCollection allowedScope)
        => new(apiId, allowedScope);
}