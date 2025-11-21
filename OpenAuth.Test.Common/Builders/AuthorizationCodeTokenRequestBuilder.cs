using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizationCodeTokenRequestBuilder
{
    private string _clientId = DefaultValues.ClientId;
    private string _subject = DefaultValues.Subject;
    private string _audience = DefaultValues.Audience;
    private string _scopes = DefaultValues.Scopes;
    private string _redirectUri = DefaultValues.RedirectUri;
    private string _code = DefaultValues.Code;
    private string? _codeVerifier;
    private string? _clientSecret;
    
    public AuthorizationCodeTokenRequestBuilder() { }

    public AuthorizationCodeTokenRequestBuilder FromAuthorizationGrant(AuthorizationGrant grant)
    {
        _clientId = grant.ClientId.ToString();
        _subject = grant.Subject;
        _scopes = grant.Scopes.ToString();
        _redirectUri = grant.RedirectUri.ToString();
        _code = grant.Code;

        return this;
    }

    public AuthorizationCodeTokenRequestBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithAudience(string audience)
    {
        _audience = audience;
        return this;
    }

    public AuthorizationCodeTokenRequestBuilder WithScopes(string scopes)
    {
        _scopes = scopes;
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = redirectUri;
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithCodeVerifier(string? codeVerifier)
    {
        _codeVerifier = codeVerifier;
        return this;
    }
    
    public AuthorizationCodeTokenRequestBuilder WithClientSecret(string? clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }

    public TokenRequest Build()
    {
        var clientId = ClientId.Create(_clientId);
        var audience = AudienceName.Create(_audience);
        var scopes = ScopeCollection.Parse(_scopes);
        var redirectUri = RedirectUri.Create(_redirectUri);

        var request = new AuthorizationCodeTokenRequest
        {
            ClientId = clientId,
            Subject = _subject,
            RequestedAudience = audience,
            RequestedScopes = scopes,
            RedirectUri = redirectUri,
            Code = _code,
            CodeVerifier = _codeVerifier,
            ClientSecret = _clientSecret
        };

        return request;
    }
}