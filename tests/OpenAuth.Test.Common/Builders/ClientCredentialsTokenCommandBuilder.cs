using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class ClientCredentialsTokenCommandBuilder
{
    private ClientId _clientId = ClientId.Parse(DefaultValues.ClientId);
    private string _clientSecret = DefaultValues.ClientSecret;
    private string _audience = DefaultValues.ApiAudience;
    private string _scopes = DefaultValues.Scopes;

    public ClientCredentialsTokenCommandBuilder()
    { }

    public ClientCredentialsTokenCommandBuilder WithClientId(ClientId clientId)
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
        var audience = new AudienceIdentifier(_audience);
        var scopes = ScopeCollection.Parse(_scopes);
        
        return ClientCredentialsTokenCommand.Create(
            _clientId,
            audience,
            scopes,
            _clientSecret
        );
    }
}