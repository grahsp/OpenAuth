using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Test.Unit.OAuth;

public class TokenServiceTests
{
    private readonly IClientQueryService _clientQueryService;
    private readonly ISigningKeyQueryService _signingKeyQueryService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ITokenIssuer _clientCredentialsIssuer;
    private readonly ITokenIssuer _authorizationCodeIssuer;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _clientQueryService = Substitute.For<IClientQueryService>();
        _signingKeyQueryService = Substitute.For<ISigningKeyQueryService>();
        _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _clientCredentialsIssuer = Substitute.For<ITokenIssuer>();
        _authorizationCodeIssuer = Substitute.For<ITokenIssuer>();

        _clientCredentialsIssuer.GrantType.Returns(GrantType.ClientCredentials);
        _authorizationCodeIssuer.GrantType.Returns(GrantType.AuthorizationCode);

        _tokenService = new TokenService(
            [_clientCredentialsIssuer],
            _clientQueryService,
            _signingKeyQueryService,
            _tokenGenerator);
    }


    [Fact]
    public void Constructor_WithDuplicateGrantTypes_ThrowsArgumentException()
    {
        var issuer1 = Substitute.For<ITokenIssuer>();
        var issuer2 = Substitute.For<ITokenIssuer>();
        issuer1.GrantType.Returns(GrantType.ClientCredentials);
        issuer2.GrantType.Returns(GrantType.ClientCredentials);

        Assert.Throws<ArgumentException>(() =>
            new TokenService(
                [issuer1, issuer2],
                _clientQueryService,
                _signingKeyQueryService,
                _tokenGenerator
            ));
    }

    [Fact]
    public void Constructor_WithNoIssuers_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new TokenService(
                [],
                _clientQueryService,
                _signingKeyQueryService,
                _tokenGenerator
            ));
    }

    [Fact]
    public async Task IssueToken_WithUnsupportedGrantType_ThrowsInvalidOperationException()
    {
        var tokenService = new TokenService(
            [_authorizationCodeIssuer],
            _clientQueryService,
            _signingKeyQueryService,
            _tokenGenerator);

        var request = CreateValidRequest();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tokenService.IssueToken(request));

        Assert.Equal("Invalid grant type.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithMismatchedGrantType_ThrowsInvalidOperationException()
    {
        var otherIssuer = Substitute.For<ITokenIssuer>();
        otherIssuer.GrantType.Returns(GrantType.AuthorizationCode);

        var tokenService = new TokenService(
            [otherIssuer],
            _clientQueryService,
            _signingKeyQueryService,
            _tokenGenerator);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tokenService.IssueToken(CreateValidRequest()));

        Assert.Equal("Invalid grant type.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithClientNotFound_ThrowsInvalidOperationException()
    {
        var request = CreateValidRequest();

        _clientQueryService
            .GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<AudienceName>(), Arg.Any<CancellationToken>())
            .Returns((ClientTokenData?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _tokenService.IssueToken(request));

        Assert.Equal("Client not found or does not have access to the requested audience.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithInvalidScopes_ThrowsInvalidOperationException()
    {
        var request = CreateValidRequest() with { RequestedScopes = ScopeCollection.Parse("invalid") };
        var tokenData = CreateValidTokenData();

        _clientQueryService
            .GetTokenDataAsync(request.ClientId, request.RequestedAudience, Arg.Any<CancellationToken>())
            .Returns(tokenData);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _tokenService.IssueToken(request));

        Assert.Contains($"Invalid scopes: '{ request.RequestedScopes }'", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WithNoActiveSigningKey_ThrowsInvalidOperationException()
    {
        var request = CreateValidRequest();
        var tokenData = CreateValidTokenData();
        var tokenContext = CreateValidTokenContext();

        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<AudienceName>(), Arg.Any<CancellationToken>())
            .Returns(tokenData);
        _clientCredentialsIssuer.IssueToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(tokenContext);
        _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns((SigningKeyData?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _tokenService.IssueToken(request));

        Assert.Equal("No active signing key found.", ex.Message);
    }

    [Fact]
    public async Task IssueToken_WhenTokenGeneratorThrows_PropagatesException()
    {
        var request = CreateValidRequest();
        var tokenData = CreateValidTokenData();
        var tokenContext = CreateValidTokenContext();
        var keyData = CreateValidKeyData();

        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<AudienceName>(), Arg.Any<CancellationToken>())
            .Returns(tokenData);
        _clientCredentialsIssuer.IssueToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(tokenContext);
        _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(keyData);
        _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData)
            .Throws(new InvalidOperationException("Signing failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _tokenService.IssueToken(request));
    }

    [Fact]
    public async Task IssueToken_WithValidRequest_ReturnsTokenResponse()
    {
        var request = CreateValidRequest();
        var tokenData = CreateValidTokenData();
        var tokenContext = CreateValidTokenContext();
        var keyData = CreateValidKeyData();
        const string generatedToken = "token-value";

        _clientQueryService.GetTokenDataAsync(request.ClientId, request.RequestedAudience, Arg.Any<CancellationToken>())
            .Returns(tokenData);
        _clientCredentialsIssuer.IssueToken(request, Arg.Any<CancellationToken>())
            .Returns(tokenContext);
        _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(keyData);
        _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData)
            .Returns(generatedToken);

        var result = await _tokenService.IssueToken(request);

        Assert.NotNull(result);
        Assert.Equal(generatedToken, result.Token);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn);
    }

    [Fact]
    public async Task IssueToken_PassesCancellationTokenThroughChain()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var request = CreateValidRequest();
        var tokenData = CreateValidTokenData();
        var tokenContext = CreateValidTokenContext();
        var keyData = CreateValidKeyData();

        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<AudienceName>(), token)
            .Returns(tokenData);
        _clientCredentialsIssuer.IssueToken(Arg.Any<TokenRequest>(), token)
            .Returns(tokenContext);
        _signingKeyQueryService.GetCurrentKeyDataAsync(token)
            .Returns(keyData);
        _tokenGenerator.GenerateToken(Arg.Any<TokenContext>(), Arg.Any<ClientTokenData>(), Arg.Any<SigningKeyData>())
            .Returns("token");

        await _tokenService.IssueToken(request, token);

        await _clientQueryService.Received(1).GetTokenDataAsync(request.ClientId, request.RequestedAudience, token);
        await _clientCredentialsIssuer.Received(1).IssueToken(request, token);
        await _signingKeyQueryService.Received(1).GetCurrentKeyDataAsync(token);
    }

    [Fact]
    public async Task IssueToken_WhenIssuerThrows_PropagatesException()
    {
        var request = CreateValidRequest();
        var tokenData = CreateValidTokenData();

        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<AudienceName>(), Arg.Any<CancellationToken>())
            .Returns(tokenData);
        _clientCredentialsIssuer.IssueToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
            .Throws(new UnauthorizedAccessException("Invalid credentials"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _tokenService.IssueToken(request));
    }

    [Fact]
    public async Task IssueToken_CallsTokenGeneratorWithCorrectParameters()
    {
        var request = CreateValidRequest();
        var tokenData = CreateValidTokenData();
        var tokenContext = CreateValidTokenContext();
        var keyData = CreateValidKeyData();

        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<AudienceName>(), Arg.Any<CancellationToken>())
            .Returns(tokenData);
        _clientCredentialsIssuer.IssueToken(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(tokenContext);
        _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(keyData);
        _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData)
            .Returns("token");

        await _tokenService.IssueToken(request);

        _tokenGenerator.Received(1).GenerateToken(tokenContext, tokenData, keyData);
    }

    
    private static ClientCredentialsTokenRequest CreateValidRequest() => new()
    {
        ClientId = ClientId.New(),
        ClientSecret = "secret",
        RequestedAudience = new AudienceName("api.example.com"),
        RequestedScopes = ScopeCollection.Parse("read write")
    };

    private static ClientTokenData CreateValidTokenData() => new(
        Scopes: ScopeCollection.Parse("read write admin"),
        [GrantType.ClientCredentials, GrantType.AuthorizationCode],
        TokenLifetime: TimeSpan.FromHours(1));

    private static TokenContext CreateValidTokenContext()
    {
        var clientId = ClientId.New();
        return new TokenContext(
            clientId,
            clientId.ToString(),
            new AudienceName("api.example.com"),
            ScopeCollection.Parse("read write admin")
        );
    }
    
    private static SigningKeyData CreateValidKeyData()
    {
        return new SigningKeyData(
            SigningKeyId.New(),
            KeyType.RSA,
            SigningAlgorithm.RS256,
            new Key("key")
        );
    }
}