using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public abstract record TokenCommand
{
    public abstract GrantType GrantType { get; }
    
    public ClientId ClientId { get; init; }
    public ScopeCollection? RequestedScopes { get; init; }

    internal TokenCommand(ClientId clientId, ScopeCollection? scope)
    {
        ClientId = clientId;
        RequestedScopes = scope;
    }
}