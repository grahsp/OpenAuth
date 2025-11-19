using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizeCommandBuilder
{
    private AuthorizeCommand _cmd;

    public AuthorizeCommandBuilder(ClientId clientId, string redirectUri)
    {
        _cmd = new AuthorizeCommand
        (
            "code",
            clientId.ToString(),
            "test-subject",
            redirectUri,
            null,
            null,
            null,
            null,
            null
        );
    }
    
    public AuthorizeCommandBuilder WithResponseType(string responseType)
    {
        _cmd = _cmd with { ResponseType = responseType };
        return this;
    }

    public AuthorizeCommandBuilder WithPkce(Pkce pkce)
    {
        _cmd = _cmd with
        {
            CodeChallenge = pkce.CodeChallenge,
            CodeChallengeMethod = pkce.CodeChallengeMethod.ToString()
        };
        return this;
    }

    public AuthorizeCommandBuilder WithAudience(string audience)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
        
        _cmd = _cmd with { Audience = audience };
        return this;
    }
    
    public AuthorizeCommandBuilder WithScope(string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        
        _cmd = _cmd with { Scope = scope };
        return this;
    }

    public AuthorizeCommandBuilder WithSubject(string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        _cmd = _cmd with { Subject = subject };
        return this;
    }
    
    public AuthorizeCommand Build() => _cmd;
}