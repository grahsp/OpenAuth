using Microsoft.AspNetCore.WebUtilities;

namespace OpenAuth.Test.Integration.Infrastructure.Builders;

public sealed class AuthorizeUriBuilder
{
    private string _responseType = "code";
    private string? _clientId;
    private string? _redirectUri;
    private string? _scope;
    private string? _state;
    private string? _codeChallenge;
    private string? _codeChallengeMethod;

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

    public AuthorizeUriBuilder WithResponseType(string responseType)
    {
        _responseType = responseType;
        return this;
    }

    public AuthorizeUriBuilder WithScope(string? scope)
    {
        _scope = scope;
        return this;
    }

    public AuthorizeUriBuilder WithState(string? state)
    {
        _state = state;
        return this;
    }

    public AuthorizeUriBuilder WithPkce(string? challenge, string? method = "S256")
    {
        _codeChallenge = challenge;
        _codeChallengeMethod = method;
        return this;
    }

    public string Build()
    {
        var parameters = new Dictionary<string, string?>
        {
            ["response_type"] = _responseType,
            ["client_id"] = _clientId,
            ["redirect_uri"] = _redirectUri,
            ["scope"] = _scope,
            ["state"] = _state,
            ["code_challenge"] = _codeChallenge,
            ["code_challenge_method"] = _codeChallengeMethod
        };

        return QueryHelpers.AddQueryString("/connect/authorize", parameters);
    }
}