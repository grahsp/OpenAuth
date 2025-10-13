using NSubstitute;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.AuthorizationGrant;
using OpenAuth.Domain.AuthorizationGrant.Enums;
using OpenAuth.Domain.AuthorizationGrant.ValueObjects;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationCodeTokenIssuerTests
{
    private readonly IAuthorizationGrantStore _grantStore;
    private readonly AuthorizationCodeTokenIssuer _sut;

    private static readonly ClientId DefaultClientId = ClientId.New();


    public AuthorizationCodeTokenIssuerTests()
    {
        _grantStore = Substitute.For<IAuthorizationGrantStore>();
        _sut = new AuthorizationCodeTokenIssuer(_grantStore);
    }
    
    private static AuthorizationCodeTokenRequest DefaultRequest()
        => new() {
            ClientId = DefaultClientId,
            Code = "auth-code",
            RedirectUri = RedirectUri.Create("https://client.app/callback"),
            RequestedAudience = new AudienceName("api.example.com"),
            RequestedScopes = [new Scope("read"), new Scope("write")]
        };

    private static AuthorizationGrant DefaultGrant(string? codeChallenge = null)
    {
        var pkce = codeChallenge is not null
            ? Pkce.Create(codeChallenge, CodeChallengeMethod.S256)
            : null;
        
        return AuthorizationGrant.Create(
            "auth-code",
            GrantType.AuthorizationCode,
            DefaultClientId,
            RedirectUri.Create("https://client.app/callback"),
            new AudienceName("api.example.com"),
            [new Scope("read"), new Scope("write")],
            DateTimeOffset.UtcNow,
            pkce
        );
    }

    private void SetupGrantStore(AuthorizationGrant grant)
        => _grantStore.GetAsync(grant.Code).Returns(grant);

    [Fact]
    public async Task IssueToken_WithValidGrant_WithoutPkce_ReturnsExpectedTokenContext()
    {
        // Arrange
        var request = DefaultRequest();
        var grant = DefaultGrant();
        SetupGrantStore(grant);

        // Act
        var result = await _sut.IssueToken(request);

        // Assert
        Assert.Equal(grant.ClientId, result.ClientId);
        Assert.Equal(grant.ClientId.ToString(), result.Subject);
        Assert.Equal(grant.Audience, result.RequestedAudience);
        Assert.Equal(grant.Scopes, result.RequestedScopes);
    }
    
    [Fact]
    public async Task IssueToken_WithValidGrant_WithPkce_ReturnsExpectedTokenContext()
    {
        // Arrange
        var request = DefaultRequest() with { CodeVerifier = "code-challenge" };
        var grant = DefaultGrant("code-challenge");
        SetupGrantStore(grant);

        // Act
        var result = await _sut.IssueToken(request);

        // Assert
        Assert.Equal(grant.ClientId, result.ClientId);
        Assert.Equal(grant.ClientId.ToString(), result.Subject);
        Assert.Equal(grant.Audience, result.RequestedAudience);
        Assert.Equal(grant.Scopes, result.RequestedScopes);
    }

    [Fact]
    public async Task IssueToken_WithUnknownCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest() with { Code = "invalid-code" };
        _grantStore.GetAsync("invalid-code")
            .Returns((AuthorizationGrant?)null);
    
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Invalid authorization code.", ex.Message);
    }
    
    [Fact]
    public async Task IssueToken_WhenGrantIsConsumed_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest();
        var grant = DefaultGrant();
        
        grant.Consume();
        SetupGrantStore(grant);
    
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Authorization code has already been used.", ex.Message);
    }
    
    [Fact]
    public async Task IssueToken_WithClientIdMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest() with { ClientId = ClientId.New() };
        var grant = DefaultGrant();
        SetupGrantStore(grant);
    
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Client ID mismatch.", ex.Message);
    }
    
    [Fact]
    public async Task IssueToken_WithRedirectUriMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest() with { RedirectUri = RedirectUri.Create("https://invalid-uri.com/callback") };
        var grant = DefaultGrant();
        SetupGrantStore(grant);
    
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Redirect URI mismatch.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithMissingCodeChallenge_WhenPkceRequired_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest() with { CodeVerifier = null };
        var grant = DefaultGrant("code-challenge");
        SetupGrantStore(grant);
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Invalid PKCE code verifier.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithInvalidCodeChallenge_WhenPkceRequired_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest() with { CodeVerifier = "invalid-code-challenge" };
        var grant = DefaultGrant("code-challenge");
        SetupGrantStore(grant);
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Invalid PKCE code verifier.", ex.Message);
    }
}