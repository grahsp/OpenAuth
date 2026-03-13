using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record ClientCredentialsTokenCommand : TokenCommand
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    public AudienceIdentifier Audience { get; }
    public string ClientSecret { get; }

    private ClientCredentialsTokenCommand(
        ClientId clientId,
        AudienceIdentifier audience,
        ScopeCollection scope,
        string clientSecret) : base(clientId, scope)
    {
        Audience = audience;
        ClientSecret = clientSecret;
    }

    public static ClientCredentialsTokenCommand Create(
        ClientId clientId,
        AudienceIdentifier audience,
        ScopeCollection scope,
        string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("ClientSecret is required.");

        return new ClientCredentialsTokenCommand(clientId, audience, scope, clientSecret);
    }
}