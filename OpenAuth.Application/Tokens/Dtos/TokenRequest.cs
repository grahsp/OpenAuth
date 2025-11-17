using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public abstract record TokenRequest
{
    public abstract GrantType GrantType { get; }
    
    public required ClientId ClientId { get; init; }
    public required AudienceName? RequestedAudience { get; init; }
    public required ScopeCollection? RequestedScopes { get; init; }
}