using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class AuthorizeCommandBuilder
{
    private AuthorizeCommand _cmd;

    public AuthorizeCommandBuilder(ClientId clientId, string redirectUri)
    {
        _cmd = AuthorizeCommand.Create
        (
            "code",
            clientId.ToString(),
            "test-subject",
            redirectUri,
            "",
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
        _cmd = _cmd with { Pkce = pkce };
        return this;
    }
    
    public AuthorizeCommandBuilder WithScopes(string scopes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopes);
        
        _cmd = _cmd with { Scopes = ScopeCollection.Parse(scopes) };
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