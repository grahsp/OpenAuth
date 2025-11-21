using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class ClientCredentialsTokenRequestBuilder
{
    private string _clientId = DefaultValues.ClientId;
    private string _clientSecret = DefaultValues.ClientSecret;
    private string _audience = DefaultValues.Audience;
    private string _scopes = DefaultValues.Scopes;

    public ClientCredentialsTokenRequestBuilder()
    { }

    public ClientCredentialsTokenRequestBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    public ClientCredentialsTokenRequestBuilder WithClientSecret(string clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }
    
    public ClientCredentialsTokenRequestBuilder WithAudience(string audience)
    {
        _audience = audience;
        return this;
    }
    
    public ClientCredentialsTokenRequestBuilder WithScopes(string scopes)
    {
        _scopes = scopes;
        return this;
    }

    public TokenRequest Build()
        => new ClientCredentialsTokenRequest
        {
            ClientId = ClientId.Create(_clientId),
            ClientSecret = _clientSecret,
            RequestedAudience = AudienceName.Create(_audience),
            RequestedScopes = ScopeCollection.Parse(_scopes)
        };
}