using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizeCommandTests
{
    private static string ValidClientId => Guid.NewGuid().ToString();
    private const string ValidSubject = "test-subject";
    private const string ValidRedirectUri = "https://client.example.com/callback";
    private const string ValidScope = "read write";
    private const string ValidPkceChallenge = "abcdef";
    private const string ValidPkceMethod = "S256"; 
    
    
    [Fact]
    public void Create_WhenValidData_ReturnsExpectedCommand()
    {
        var validClientId = ValidClientId;
        
        var command = AuthorizeCommand.Create(
            "code",
            validClientId,
            ValidSubject,
            ValidRedirectUri,
            ValidScope,
            ValidPkceChallenge,
            ValidPkceMethod
        );
        
        Assert.Equal("code", command.ResponseType);
        Assert.Equal(validClientId, command.ClientId.ToString());
        Assert.Equal(ValidSubject, command.Subject);
        Assert.Equal(ValidRedirectUri, command.RedirectUri.ToString());
        Assert.Equal(ValidScope, command.Scopes.ToString());
        Assert.Equal(ValidPkceChallenge, command.Pkce!.CodeChallenge);
        Assert.Equal(ValidPkceMethod, command.Pkce!.CodeChallengeMethod.ToString());
    }
    
    [Fact]
    public void Create_WhenValidDataWithoutPkce_ReturnsExpectedCommand()
    {
        var validClientId = ValidClientId;
        
        var command = AuthorizeCommand.Create(
            "code",
            validClientId,
            ValidSubject,
            ValidRedirectUri,
            ValidScope,
            null,
            null
        );
        
        Assert.Equal("code", command.ResponseType);
        Assert.Equal(validClientId, command.ClientId.ToString());
        Assert.Equal(ValidSubject, command.Subject);
        Assert.Equal(ValidRedirectUri, command.RedirectUri.ToString());
        Assert.Equal(ValidScope, command.Scopes.ToString());
        Assert.Null(command.Pkce);
    }
    
    [Fact]
    public void Create_WhenMalformedClientId_ThrowsMalformedClientIdException()
    {
        const string invalidClientId = "invalid-client-id";

        Assert.Throws<MalformedClientException>(()
            => AuthorizeCommand.Create(
                "code",
                invalidClientId,
                ValidSubject,
                ValidRedirectUri,
                ValidScope,
                ValidPkceChallenge,
                ValidPkceMethod
            ));
    }
    
    [Fact]
    public void Create_WhenMalformedRedirectUri_ThrowsMalformedRedirectUriException()
    {
        const string invalidRedirectUri = "invalid-redirect-uri";

        Assert.Throws<MalformedRedirectUriException>(()
            => AuthorizeCommand.Create(
                "code",
                ValidClientId,
                ValidSubject,
                invalidRedirectUri,
                ValidScope,
                ValidPkceChallenge,
                ValidPkceMethod
            ));
    }
    
    [Fact]
    public void Create_WhenMalformedScope_ThrowsMalformedScopeException()
    {
        const string invalidScope = "";

        Assert.Throws<MalformedScopeException>(()
            => AuthorizeCommand.Create(
                "code",
                ValidClientId,
                ValidSubject,
                ValidRedirectUri,
                invalidScope,
                ValidPkceChallenge,
                ValidPkceMethod
            ));
    }
    
    [Fact]
    public void Create_WhenMissingCodeChallenge_ThrowsMalformedPkceException()
    {
        Assert.Throws<MalformedPkceException>(()
            => AuthorizeCommand.Create(
                "code",
                ValidClientId,
                ValidSubject,
                ValidRedirectUri,
                ValidScope,
                null,
                ValidPkceMethod
            ));
    }
    
    [Fact]
    public void Create_WhenMissingCodeChallengeMethod_ThrowsMalformedPkceException()
    {
        Assert.Throws<MalformedPkceException>(()
            => AuthorizeCommand.Create(
                "code",
                ValidClientId,
                ValidSubject,
                ValidRedirectUri,
                ValidScope,
                ValidPkceChallenge,
                null
            ));
    }
}