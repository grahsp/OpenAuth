using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationHandlerTests
{
    private readonly IClientQueryService _clientQueryService;
    private readonly IAuthorizationGrantStore _store;
    private readonly FakeTimeProvider _time;
    private readonly AuthorizationHandler _sut;

    public AuthorizationHandlerTests()
    {
        _clientQueryService = Substitute.For<IClientQueryService>();
        _store = Substitute.For<IAuthorizationGrantStore>();
        _time = new FakeTimeProvider();
        
        _sut = new AuthorizationHandler(_clientQueryService, _store, _time);
    }

    // ------------------------------------------------------------
    // Mothers / Builders for Unit Tests
    // ------------------------------------------------------------

    private static AuthorizeCommand ValidCommand(Action<AuthorizeCommandParameters>? overrides = null)
    {
        var p = new AuthorizeCommandParameters
        {
            ResponseType = "code",
            ClientId = ClientId.New().ToString(),
            Subject = "test-subject",
            RedirectUri = "https://example.com/callback",
            Scopes = "read write",
            CodeChallenge = "this-is-a-code-challenge",
            CodeChallengeMethod = "S256"
        };

        overrides?.Invoke(p);

        return AuthorizeCommand.Create(
            p.ResponseType,
            p.ClientId,
            p.Subject,
            p.RedirectUri,
            p.Scopes,
            p.CodeChallenge,
            p.CodeChallengeMethod
        );
    }

    private static ClientAuthorizationData ConfidentialClient(ClientId? id = null)
    {
        var clientId = id ?? ClientId.New();

        return new ClientAuthorizationData(
            clientId,
            IsClientPublic: false,
            [],
            RedirectUris: [RedirectUri.Create("https://example.com/callback")]
        );
    }

    private static ClientAuthorizationData PublicClient(ClientId? id = null)
    {
        var clientId = id ?? ClientId.New();

        return new ClientAuthorizationData(
            clientId,
            IsClientPublic: true,
            [],
            RedirectUris: [RedirectUri.Create("https://example.com/callback")]
        );
    }

    // ------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------

    [Fact]
    public async Task ClientNotFound_ThrowsInvalidClientException()
    {
        // Arrange
        var command = ValidCommand();

        // Act / Assert
        await Assert.ThrowsAsync<InvalidClientException>(() =>
            _sut.AuthorizeAsync(command));
    }

    [Fact]
    public async Task InvalidResponseType_ThrowsUnsupportedResponseTypeException()
    {
        // Arrange
        var command = ValidCommand(p => p.ResponseType = "invalid");
        var client = ConfidentialClient(command.ClientId);

        _clientQueryService
            .GetAuthorizationDataAsync(command.ClientId)
            .Returns(client);

        // Act / Assert
        await Assert.ThrowsAsync<UnsupportedResponseTypeException>(() =>
            _sut.AuthorizeAsync(command));
    }

    [Fact]
    public async Task InvalidRedirectUri_ThrowsInvalidRedirectUriException()
    {
        // Arrange
        var command = ValidCommand(p =>
            p.RedirectUri = "https://malicious.com/callback");

        var client = ConfidentialClient(command.ClientId);

        _clientQueryService
            .GetAuthorizationDataAsync(command.ClientId)
            .Returns(client);

        // Act / Assert
        await Assert.ThrowsAsync<InvalidRedirectUriException>(() =>
            _sut.AuthorizeAsync(command));
    }
    
    [Fact]
    public async Task ValidRequest_ReturnsAuthorizationGrant()
    {
        // Arrange
        var command = ValidCommand();
        var client = ConfidentialClient(command.ClientId);

        _clientQueryService
            .GetAuthorizationDataAsync(command.ClientId)
            .Returns(client);

        // Act
        var grant = await _sut.AuthorizeAsync(command);

        // Assert
        Assert.Equal(command.RedirectUri, grant.RedirectUri);
        Assert.NotNull(grant.Code);
        Assert.NotEmpty(grant.Code);
        Assert.Equal(command.ClientId, grant.ClientId);
    }

    [Fact]
    public async Task AuthorizeAsync_AddsGrantToStore()
    {
        // Arrange
        var command = ValidCommand();
        var client = ConfidentialClient(command.ClientId);

        _clientQueryService
            .GetAuthorizationDataAsync(command.ClientId)
            .Returns(client);

        // Act
        await _sut.AuthorizeAsync(command);

        // Assert
        await _store.Received(1)
            .AddAsync(Arg.Any<AuthorizationGrant>());
    }
}

/// <summary>
/// Parameter object for building AuthorizeCommand in tests.
/// </summary>
public class AuthorizeCommandParameters
{
    public string ResponseType { get; set; }
    public string ClientId { get; set; }
    public string Subject { get; set; }
    public string RedirectUri { get; set; }
    public string Scopes { get; set; }
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
}