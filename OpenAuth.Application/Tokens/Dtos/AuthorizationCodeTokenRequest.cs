using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record AuthorizationCodeTokenRequest : TokenRequest
{
    public override GrantType GrantType => GrantType.AuthorizationCode;
    
    public required string Code { get; init; }
    public required string Subject { get; init; }
    public required RedirectUri RedirectUri { get; init; }
    
    public string? CodeVerifier { get; init; }
    public string? ClientSecret { get; init; }
}