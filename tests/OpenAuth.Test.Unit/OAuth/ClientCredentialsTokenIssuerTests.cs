using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.OAuth;

public class ClientCredentialsTokenIssuerTests
{
    private readonly ISecretQueryService _secretQueryService;
    private readonly ClientCredentialsTokenIssuer _issuer;

    public ClientCredentialsTokenIssuerTests()
    {
        _secretQueryService = Substitute.For<ISecretQueryService>();
        _issuer = new ClientCredentialsTokenIssuer(_secretQueryService);
    }

    
    private static ClientCredentialsTokenCommand CreateRequest(
        string? secret = "secret",
        string? audience = "api.example.com",
        IEnumerable<Scope>? scopes = null)
    {
        return ClientCredentialsTokenCommand.Create(
            ClientId.New(),
            new AudienceName(audience ?? "api.example.com"),
            ScopeCollection.Parse("read"),
            secret ?? "secret"
        );
    }

    [Fact]
    public void GrantType_ShouldReturnClientCredentials()
    {
        Assert.Equal(GrantType.ClientCredentials, _issuer.GrantType);
    }

    [Fact]
    public async Task IssueToken_WithValidCredentials_ReturnsTokenContext()
    {
        // Arrange
        var request = CreateRequest("valid-secret", scopes: [new Scope("read"), new Scope("write")]);

        _secretQueryService
            .ValidateSecretAsync(request.ClientId, request.ClientSecret, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _issuer.IssueToken(request);

        // Assert
        await _secretQueryService.Received(1)
            .ValidateSecretAsync(request.ClientId, request.ClientSecret, Arg.Any<CancellationToken>());

        Assert.NotNull(result);
        Assert.Equal(request.ClientId, result.ClientId);
        Assert.Equal(request.ClientId.ToString(), result.Subject);
        Assert.Equal(request.RequestedAudience, result.RequestedAudience);
        Assert.Equal(request.RequestedScopes, result.RequestedScopes);
    }

    [Fact]
    public async Task IssueToken_WithInvalidCredentials_ThrowsInvalidClientException()
    {
        var request = CreateRequest("invalid-secret");

        _secretQueryService
            .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        await Assert.ThrowsAsync<InvalidClientException>(
            () => _issuer.IssueToken(request));
    }

    [Fact]
    public async Task IssueToken_PassesCancellationTokenToService()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var request = CreateRequest();

        _secretQueryService
            .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), token)
            .Returns(true);

        await _issuer.IssueToken(request, token);

        await _secretQueryService.Received(1).ValidateSecretAsync(
            request.ClientId,
            request.ClientSecret,
            token);
    }

    [Fact]
    public async Task IssueToken_WhenServiceThrows_PropagatesException()
    {
        var request = CreateRequest();

        _secretQueryService
            .ValidateSecretAsync(Arg.Any<ClientId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("Database connection failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _issuer.IssueToken(request));
    }
}