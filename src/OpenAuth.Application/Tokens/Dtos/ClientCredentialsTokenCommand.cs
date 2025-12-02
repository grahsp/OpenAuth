using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record ClientCredentialsTokenCommand : TokenCommand
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    public string ClientSecret { get; }

    private ClientCredentialsTokenCommand(
        ClientId clientId,
        ScopeCollection scope,
        string clientSecret)
        : base(clientId, scope)
    {
        ClientSecret = clientSecret;
    }

    public static ClientCredentialsTokenCommand Create(
        ClientId clientId,
        ScopeCollection scope,
        string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("ClientSecret is required.");

        return new ClientCredentialsTokenCommand(clientId, scope, clientSecret);
    }
}