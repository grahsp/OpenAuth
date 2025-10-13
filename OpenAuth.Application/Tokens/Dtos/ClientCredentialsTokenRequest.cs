using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record ClientCredentialsTokenRequest : TokenRequest
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    public required string ClientSecret { get; init; }
}