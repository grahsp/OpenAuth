using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public sealed class AuthorizeUriBuilder
{
    private string _responseType = DefaultValues.ResponseType;
    private string _clientId = DefaultValues.ClientId;
    private string _redirectUri = DefaultValues.RedirectUri;
    private string _audience = DefaultValues.ApiAudience;
    private string _scopes = DefaultValues.Scopes;
    private string? _state;
    private string? _nonce;
    private Pkce? _pkce = TestData.CreateValidPkce();

    public AuthorizeUriBuilder WithResponseType(string responseType)
    {
        _responseType = responseType;
        return this;
    }
    
    public AuthorizeUriBuilder WithClient(string clientId)
    {
        _clientId = clientId;
        return this;
    }

    public AuthorizeUriBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = redirectUri;
        return this;
    }
    
    public AuthorizeUriBuilder WithAudience(string audience)
    {
        _audience = audience;
        return this;
    }

    public AuthorizeUriBuilder WithScope(string scopes)
    {
        _scopes = scopes;
        return this;
    }

    public AuthorizeUriBuilder WithState(string? state)
    {
        _state = state;
        return this;
    }
    
    public AuthorizeUriBuilder WithNonce(string? nonce)
    {
        _nonce = nonce;
        return this;
    }

    public AuthorizeUriBuilder WithPkce(Pkce? pkce)
    {
        _pkce = pkce;
        return this;
    }

    public string Build()
    {
        var parameters = new Dictionary<string, string?>
        {
            ["response_type"] = _responseType,
            ["client_id"] = _clientId,
            ["redirect_uri"] = _redirectUri,
            ["audience"] = _audience,
            ["scope"] = _scopes,
            ["state"] = _state,
            ["nonce"] = _nonce,
            ["code_challenge"] = _pkce?.CodeChallenge,
            ["code_challenge_method"] = _pkce?.CodeChallengeMethod.ToString()
        };

        return QueryHelpers.AddQueryString("/connect/authorize", parameters);
    }
}