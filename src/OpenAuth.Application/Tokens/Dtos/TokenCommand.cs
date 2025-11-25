using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public abstract record TokenCommand
{
    public abstract GrantType GrantType { get; }
    
    public ClientId ClientId { get; init; }
    public AudienceName? RequestedAudience { get; init; }
    public ScopeCollection? RequestedScopes { get; init; }

    internal TokenCommand(ClientId clientId, AudienceName? audience, ScopeCollection? scope)
    {
        ClientId = clientId;
        RequestedAudience = audience;
        RequestedScopes = scope;
    }
}