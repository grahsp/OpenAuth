using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Dtos;

public record ClientCredentialsTokenCommand : TokenCommand
{
    public override GrantType GrantType => GrantType.ClientCredentials;
    
    public string ClientSecret { get; }

    private ClientCredentialsTokenCommand(
        ClientId clientId,
        AudienceName? audience,
        ScopeCollection? scope,
        string clientSecret)
        : base(clientId, audience, scope)
    {
        ClientSecret = clientSecret;
    }

    public static ClientCredentialsTokenCommand Create(
        ClientId clientId,
        AudienceName? audience,
        ScopeCollection? scope,
        string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("ClientSecret is required.");

        return new ClientCredentialsTokenCommand(clientId, audience, scope, clientSecret);
    }
}