using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public abstract record TokenCommand
{
    public abstract GrantType GrantType { get; }
    
    public ClientId ClientId { get; }
    public AudienceName? RequestedAudience { get; }
    public ScopeCollection? RequestedScopes { get; }

    internal TokenCommand(ClientId clientId, AudienceName? audience, ScopeCollection? scope)
    {
        ClientId = clientId;
        RequestedAudience = audience;
        RequestedScopes = scope;
    }
}