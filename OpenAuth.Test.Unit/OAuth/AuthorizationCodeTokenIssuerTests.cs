using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.OAuth;

// TODO: temporary fix after refactor -- refactor whole test class!!
public class AuthorizationCodeTokenIssuerTests
{
    private readonly IAuthorizationGrantStore _grantStore;
    private readonly IClientQueryService _clientQueryService;
    private readonly ISecretQueryService _secretQueryService;
    private readonly AuthorizationCodeTokenIssuer _sut;

    private static readonly ClientId DefaultClientId = ClientId.New();
    private static readonly AudienceName DefaultAudience = new("api.example.com");
    private static readonly ScopeCollection DefaultScopes = ScopeCollection.Parse("read write");
    private static readonly RedirectUri DefaultRedirect = RedirectUri.Create("https://client.app/callback");

    public AuthorizationCodeTokenIssuerTests()
    {
        _grantStore = Substitute.For<IAuthorizationGrantStore>();
        _clientQueryService = Substitute.For<IClientQueryService>();
        _secretQueryService = Substitute.For<ISecretQueryService>();
        _sut = new AuthorizationCodeTokenIssuer(_grantStore, _clientQueryService, _secretQueryService);
    }

    private static AuthorizationCodeTokenRequest DefaultRequest()
        => new()
        {
            ClientId = DefaultClientId,
            Code = "auth-code",
            Subject = "Test-Subject",
            RedirectUri = DefaultRedirect,
            RequestedAudience = DefaultAudience,
            RequestedScopes = DefaultScopes,
            CodeVerifier = "secret-code",
            ClientSecret = "client-secret"
        };

    private static AuthorizationGrant DefaultGrant(string? codeVerifier = null)
    {
        var pkce = codeVerifier is null
            ? null
            : Pkce.Create(
                Base64UrlEncoder.Encode(
                    SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier))),
                CodeChallengeMethod.S256);

        return AuthorizationGrant.Create(
            "auth-code",
            GrantType.AuthorizationCode,
            "Test-Subject",
            DefaultClientId,
            DefaultRedirect,
            DefaultScopes,
            pkce,
            DateTimeOffset.UtcNow
        );
    }

    private void SetupGrantStore(AuthorizationGrant grant)
        => _grantStore.GetAsync(grant.Code).Returns(grant);

    private void SetupClientWithAudienceAndScopes()
    {
        var audience = new Audience(
            DefaultAudience,
            DefaultScopes);

        var clientData = new ClientTokenData(
            AllowedAudiences: [audience],
            AllowedGrantTypes: [],
            TimeSpan.Zero);

        _clientQueryService
            .GetTokenDataAsync(DefaultClientId, Arg.Any<CancellationToken>())
            .Returns(clientData);
    }

    // ---- TESTS ----

    [Fact]
    public async Task IssueToken_WithValidGrant_WithoutPkce_ReturnsExpectedTokenContext()
    {
        var request = DefaultRequest();
        var grant = DefaultGrant();
        SetupGrantStore(grant);
        SetupClientWithAudienceAndScopes();

        _secretQueryService
            .ValidateSecretAsync(DefaultClientId, request.ClientSecret!, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _sut.IssueToken(request);

        Assert.Equal(grant.ClientId, result.ClientId);
        Assert.Equal(grant.ClientId.ToString(), result.Subject);
        Assert.Equal(request.RequestedScopes, result.RequestedScopes);
    }

    [Fact]
    public async Task IssueToken_WithValidGrant_WithPkce_ReturnsExpectedTokenContext()
    {
        var request = DefaultRequest() with { CodeVerifier = "code-challenge" };
        var grant = DefaultGrant("code-challenge");

        SetupGrantStore(grant);
        SetupClientWithAudienceAndScopes();

        var result = await _sut.IssueToken(request);

        Assert.Equal(grant.ClientId, result.ClientId);
        Assert.Equal(grant.ClientId.ToString(), result.Subject);
        Assert.Equal(request.RequestedScopes, result.RequestedScopes);
    }

    [Fact]
    public async Task IssueToken_CallsRemoveOfStore()
    {
        var request = DefaultRequest();
        var grant = DefaultGrant();
        SetupGrantStore(grant);
        SetupClientWithAudienceAndScopes();

        _secretQueryService
            .ValidateSecretAsync(DefaultClientId, request.ClientSecret!, Arg.Any<CancellationToken>())
            .Returns(true);

        await _sut.IssueToken(request);

        await _grantStore.Received(1).RemoveAsync(grant.Code);
    }

    [Fact]
    public async Task IssueToken_WithUnknownCode_Throws()
    {
        var request = DefaultRequest() with { Code = "invalid-code" };
        _grantStore.GetAsync("invalid-code").Returns((AuthorizationGrant?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Invalid authorization code.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WhenGrantIsConsumed_Throws()
    {
        var request = DefaultRequest();
        var grant = DefaultGrant();
        grant.Consume();

        SetupGrantStore(grant);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Authorization code has already been used.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithClientIdMismatch_Throws()
    {
        var request = DefaultRequest() with { ClientId = ClientId.New() };
        SetupGrantStore(DefaultGrant());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Client ID mismatch.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithRedirectUriMismatch_Throws()
    {
        var request = DefaultRequest() with { RedirectUri = RedirectUri.Create("https://invalid.com") };
        SetupGrantStore(DefaultGrant());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Redirect URI mismatch.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithSubjectMismatch_Throws()
    {
        var request = DefaultRequest() with { Subject = "invalid" };
        SetupGrantStore(DefaultGrant());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Subject mismatch.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithMissingCodeVerifier_WhenPkceRequired_Throws()
    {
        var request = DefaultRequest() with { CodeVerifier = null };
        var grant = DefaultGrant("code-challenge");

        SetupGrantStore(grant);
        SetupClientWithAudienceAndScopes();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Invalid PKCE code verifier.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithInvalidCodeVerifier_WhenPkceRequired_Throws()
    {
        var request = DefaultRequest() with { CodeVerifier = "invalid" };
        var grant = DefaultGrant("code-challenge");

        SetupGrantStore(grant);
        SetupClientWithAudienceAndScopes();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.IssueToken(request));

        Assert.Equal("Invalid PKCE code verifier.", ex.Message);
    }
}