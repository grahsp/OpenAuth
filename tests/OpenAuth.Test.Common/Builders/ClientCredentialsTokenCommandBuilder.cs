using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class ClientCredentialsTokenCommandBuilder
{
    private string _clientId = DefaultValues.ClientId;
    private string _clientSecret = DefaultValues.ClientSecret;
    private string _audience = DefaultValues.Audience;
    private string _scopes = DefaultValues.Scopes;

    public ClientCredentialsTokenCommandBuilder()
    { }

    public ClientCredentialsTokenCommandBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    public ClientCredentialsTokenCommandBuilder WithClientSecret(string clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }
    
    public ClientCredentialsTokenCommandBuilder WithAudience(string audience)
    {
        _audience = audience;
        return this;
    }
    
    public ClientCredentialsTokenCommandBuilder WithScopes(string scopes)
    {
        _scopes = scopes;
        return this;
    }

    public TokenCommand Build()
    {
        var clientId = ClientId.Create(_clientId);
        var audience = AudienceName.Create(_audience);
        var scopes = ScopeCollection.Parse(_scopes);
        
        return ClientCredentialsTokenCommand.Create(
            clientId,
            audience,
            scopes,
            _clientSecret
        );
    }
}