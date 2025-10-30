using OpenAuth.Application.OAuth.Authorization.Factories;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationRequestFactoryTests
{
    private readonly IAuthorizationRequestFactory _sut = new AuthorizationRequestFactory();
    
    
    [Fact]
    public void CreateFrom_WhenValidInput_ReturnsSuccess()
    {
        var query = TestAuthorizeQuery.CreateDefault();

        var result = _sut.CreateFrom(query);
        var request = result.Value;
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(request);
        
        Assert.Equal(query.ClientId, request.ClientId.ToString());
        Assert.Equal(query.RedirectUri, request.RedirectUri.Value);
        Assert.Equal(query.Audience, request.Audience.Value);
        Assert.Equal(query.Scope?.Split(' '), request.Scopes.Select(x => x.Value).ToArray());
    }

    [Fact]
    public void CreateFrom_WhenInvalidResponseType_ReturnsFailure()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { ResponseType = "invalid" };
        
        var result = _sut.CreateFrom(query);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("response type", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void CreateFrom_WhenInvalidClientId_ReturnsFailure()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { ClientId = "invalid" };
        
        var result = _sut.CreateFrom(query);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("client id", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void CreateFrom_WhenInvalidRedirectUri_ReturnsFailure()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { RedirectUri = "invalid" };
        
        var result = _sut.CreateFrom(query);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("redirect uri", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateFrom_WhenInvalidAudience_ReturnsFailure()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { Audience = null! };
        
        var result = _sut.CreateFrom(query);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("audience", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void CreateFrom_WhenEmptyScope_ReturnsSuccess()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { Scope = "" };
        
        var result = _sut.CreateFrom(query);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public void CreateFrom_WhenValidPkce_ReturnsSuccess()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { CodeChallenge = "123456", CodeChallengeMethod = "S256" };

        var result = _sut.CreateFrom(query);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var pkce = result.Value.Pkce;
        Assert.NotNull(pkce);
        Assert.Equal(query.CodeChallenge, pkce.CodeChallenge);
        Assert.Equal(query.CodeChallengeMethod, pkce.CodeChallengeMethod.ToString());
    }

    [Fact]
    public void CreateFrom_WhenMissingCodeChallengeIgnoreMethod_ReturnsSuccess()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { CodeChallenge = null, CodeChallengeMethod = "S256" };

        var result = _sut.CreateFrom(query);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.Pkce);
    }
    
    [Fact]
    public void CreateFrom_WhenMissingCodeChallengeMethod_ReturnsFailure()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { CodeChallenge = "123456", CodeChallengeMethod = null };
        
        var result = _sut.CreateFrom(query);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("code challenge method", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void CreateFrom_WhenInvalidCodeChallengeMethod_ReturnsFailure()
    {
        var query = TestAuthorizeQuery.CreateDefault() with { CodeChallenge = "123456", CodeChallengeMethod = "invalid" };
        
        var result = _sut.CreateFrom(query);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("code challenge method", result.Error.Message, StringComparison.OrdinalIgnoreCase);       
    }
}