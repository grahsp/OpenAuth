using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizationCodeTokenCommandBuilder
{
    private ClientId _clientId = ClientId.Create(DefaultValues.ClientId);
    private string _audience = DefaultValues.Audience;
    private string _scopes = DefaultValues.Scopes;
    private string _redirectUri = DefaultValues.RedirectUri;
    private string _code = DefaultValues.Code;
    private string? _codeVerifier;
    private string? _clientSecret;
    
    public AuthorizationCodeTokenCommandBuilder() { }

    public AuthorizationCodeTokenCommandBuilder FromAuthorizationGrant(AuthorizationGrant grant)
    {
        _clientId = grant.ClientId;
        _scopes = grant.GrantedScopes.ToString();
        _redirectUri = grant.RedirectUri.ToString();
        _code = grant.Code;

        return this;
    }

    public AuthorizationCodeTokenCommandBuilder WithClientId(ClientId clientId)
    {
        _clientId = clientId;
        return this;
    }
    
    public AuthorizationCodeTokenCommandBuilder WithAudience(string audience)
    {
        _audience = audience;
        return this;
    }

    public AuthorizationCodeTokenCommandBuilder WithScopes(string scopes)
    {
        _scopes = scopes;
        return this;
    }
    
    public AuthorizationCodeTokenCommandBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = redirectUri;
        return this;
    }
    
    public AuthorizationCodeTokenCommandBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }
    
    public AuthorizationCodeTokenCommandBuilder WithCodeVerifier(string? codeVerifier)
    {
        _codeVerifier = codeVerifier;
        return this;
    }
    
    public AuthorizationCodeTokenCommandBuilder WithClientSecret(string? clientSecret)
    {
        _clientSecret = clientSecret;
        return this;
    }

    public AuthorizationCodeTokenCommand Build()
    {
        var scopes = ScopeCollection.Parse(_scopes);
        var redirectUri = RedirectUri.Create(_redirectUri);

        var request = AuthorizationCodeTokenCommand.Create(
            _code,
            _clientId,
            redirectUri,
            scopes,
            _codeVerifier,
            _clientSecret
        );

        return request;
    }
}