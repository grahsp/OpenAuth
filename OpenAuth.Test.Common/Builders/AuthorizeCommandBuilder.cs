using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizeCommandBuilder
{
    private string _responseType = DefaultValues.ResponseType;
    private string _clientId = DefaultValues.ClientId;
    private string _subject = DefaultValues.Subject;
    private string _redirectUri = DefaultValues.RedirectUri;
    private string _scopes = DefaultValues.Scopes;
    private string? _codeChallenge;
    private string? _codeMethod;

    public AuthorizeCommandBuilder()
    {
    }
    
    public AuthorizeCommandBuilder WithResponseType(string responseType)
    {
        _responseType = responseType;
        return this;
    }

    public AuthorizeCommandBuilder WithClientId(string clientId)
    {
        _clientId = clientId;
        return this;
    }
    
    public AuthorizeCommandBuilder WithSubject(string subject)
    {
        _subject = subject;
        return this;
    }
    
    public AuthorizeCommandBuilder WithRedirectUri(string redirectUri)
    {
        _redirectUri = redirectUri;
        return this;
    }
    
    public AuthorizeCommandBuilder WithScopes(string scopes)
    {
        _scopes = scopes;
        return this;
    }

    public AuthorizeCommandBuilder WithPkce(Pkce pkce)
    {
        _codeChallenge = pkce.CodeChallenge;
        _codeMethod = pkce.CodeChallengeMethod.ToString();
        
        return this;
    }
    
    public AuthorizeCommand Build()
        => AuthorizeCommand.Create(
            _responseType,
            _clientId,
            _subject,
            _redirectUri,
            _scopes,
            _codeChallenge,
            _codeMethod
        );
}