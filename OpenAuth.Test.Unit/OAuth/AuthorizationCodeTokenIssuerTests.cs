using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationCodeTokenIssuerTests
{
    private readonly IAuthorizationGrantStore _grantStore;
    private readonly ISecretQueryService _secretQueryService;
    private readonly AuthorizationCodeTokenIssuer _sut;

    private static readonly ClientId DefaultClientId = ClientId.New();


    public AuthorizationCodeTokenIssuerTests()
    {
        _grantStore = Substitute.For<IAuthorizationGrantStore>();
        _secretQueryService = Substitute.For<ISecretQueryService>();
        _sut = new AuthorizationCodeTokenIssuer(_grantStore, _secretQueryService);
    }
    
    private static AuthorizationCodeTokenRequest DefaultRequest()
        => new() {
            ClientId = DefaultClientId,
            Code = "auth-code",
            Subject = "Test-Subject",
            RedirectUri = RedirectUri.Create("https://client.app/callback"),
            RequestedAudience = new AudienceName("api.example.com"),
            RequestedScopes = ScopeCollection.Parse("read write"),
            CodeVerifier = "secret-code",
            ClientSecret = "client-secret"
        };

    private static AuthorizationGrant DefaultGrant(string? codeVerifier = null)
    {
        var codeChallenge =
            Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier ?? "secret-code")));
        
        var pkce = codeVerifier is not null
            ? Pkce.Create(codeChallenge, CodeChallengeMethod.S256)
            : null;
        
        return AuthorizationGrant.Create(
            "auth-code",
            GrantType.AuthorizationCode,
            "Test-Subject",
            DefaultClientId,
            RedirectUri.Create("https://client.app/callback"),
            new AudienceName("api.example.com"),
            ScopeCollection.Parse("read write"),
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

        _secretQueryService.ValidateSecretAsync(grant.ClientId, request.ClientSecret!, Arg.Any<CancellationToken>())
            .Returns(true);

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
    public async Task IssueToken_CallsRemoveOfStore()
    {
        // Arrange
        var request = DefaultRequest() with { CodeVerifier = "secret-code" };
        var grant = DefaultGrant("secret-code");
        SetupGrantStore(grant);
        
        // Act
        await _sut.IssueToken(request);

        // Assert
        await _grantStore.Received(1).RemoveAsync(grant.Code);
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
    public async Task IssueToken_WithSubjectMismatch_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = DefaultRequest() with { Subject = "invalid-subject" };
        var grant = DefaultGrant();
        SetupGrantStore(grant);
    
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.IssueToken(request));
        Assert.Equal("Subject mismatch.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithMissingCodeVerifier_WhenPkceRequired_ThrowsInvalidOperationException()
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
    public async Task IssueToken_WithInvalidCodeVerifier_WhenPkceRequired_ThrowsInvalidOperationException()
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