using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientCredentialsTokenRequestBuilder
{
    private string _clientId = ClientId.New().ToString();
    private string _clientSecret = "this-is-a-secret";
    private string _audience = "api";
    private string _scopes = "read write";

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