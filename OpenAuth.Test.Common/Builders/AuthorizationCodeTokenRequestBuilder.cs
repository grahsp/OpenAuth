using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizationCodeTokenRequestBuilder
{
    private AuthorizationCodeTokenRequest _request;
    
    public AuthorizationCodeTokenRequestBuilder(ClientId clientId, AuthorizationGrant authorizationGrant)
    {
        _request = new AuthorizationCodeTokenRequest
        {
            ClientId = clientId,
            Subject = authorizationGrant.Subject,
            RequestedAudience = authorizationGrant.Audience,
            RequestedScopes = authorizationGrant.Scopes,
            RedirectUri = authorizationGrant.RedirectUri,
            Code = authorizationGrant.Code,
            CodeVerifier = null,
            ClientSecret = null
        };       
    }
    
    public AuthorizationCodeTokenRequestBuilder WithCodeVerifier(string verifier)
    {
        _request = _request with { CodeVerifier = verifier };
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithClientSecret(string clientSecret)
    {
        _request = _request with { ClientSecret = clientSecret };
        return this;
    }
    
    public AuthorizationCodeTokenRequest Build() => _request;
}