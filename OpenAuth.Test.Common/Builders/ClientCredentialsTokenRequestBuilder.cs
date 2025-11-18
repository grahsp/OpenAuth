using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class ClientCredentialsTokenRequestBuilder
{
    private ClientCredentialsTokenRequest _request;

    public ClientCredentialsTokenRequestBuilder(ClientId clientId, string clientSecret)
    {
        _request = new ClientCredentialsTokenRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            RequestedAudience = null,
            RequestedScopes = null
        };
    }
    
    public ClientCredentialsTokenRequestBuilder WithAudience(string audience)
    {
        _request = _request with { RequestedAudience = AudienceName.Create(audience) };
        return this;
    }
    
    public ClientCredentialsTokenRequestBuilder WithScopes(string scopes)
    {
        _request = _request with { RequestedScopes = ScopeCollection.Parse(scopes)};
        return this;
    }
    
    public ClientCredentialsTokenRequest Build() => _request;
}