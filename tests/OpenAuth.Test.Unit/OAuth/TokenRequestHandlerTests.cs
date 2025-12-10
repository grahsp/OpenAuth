using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Oidc;
using OpenAuth.Application.Tokens;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class TokenRequestHandlerTests
{
    private readonly ITokenRequestProcessor _processor;
    private readonly IClientQueryService _clientQueryService;
    private readonly ITokenHandler<AccessTokenContext> _accessTokenHandler;
    private readonly ITokenHandler<IdTokenContext> _idTokenHandler;
    private readonly TokenRequestHandler _sut;

    private readonly TokenCommand _validCommand;
    
    public TokenRequestHandlerTests()
    {
        _processor = Substitute.For<ITokenRequestProcessor>();
        _clientQueryService = Substitute.For<IClientQueryService>();
        _accessTokenHandler = Substitute.For<ITokenHandler<AccessTokenContext>>();
        _idTokenHandler = Substitute.For<ITokenHandler<IdTokenContext>>();
        
        _processor.GrantType.Returns(GrantType.AuthorizationCode);
        _processor.ProcessAsync(Arg.Any<AuthorizationCodeTokenCommand>(),
                Arg.Any<ClientTokenData>(), Arg.Any<CancellationToken>())
            .Returns(TestData.CreateValidTokenContext());

        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
            .Returns(TestData.CreateValidTokenData());

        _accessTokenHandler.CreateAsync(Arg.Any<AccessTokenContext>())
            .Returns("access_token");

        _idTokenHandler.CreateAsync(Arg.Any<IdTokenContext>())
            .Returns("id_token");
        
        _sut = new TokenRequestHandler([_processor], _clientQueryService, _accessTokenHandler, _idTokenHandler);
        
        _validCommand = TestData.CreateValidAuthorizationCodeTokenCommand();
    }

    [Fact]
    public void Constructor_WithNoRegisteredProcessors_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new TokenRequestHandler([], _clientQueryService, _accessTokenHandler, _idTokenHandler));
    }

    [Fact]
    public async Task HandleAsync_WithUnsupportedIncomingGrantType_ThrowsInvalidRequestException()
    {
        _processor.GrantType.Returns(GrantType.ClientCredentials);
        
        var sut = new TokenRequestHandler([_processor], _clientQueryService, _accessTokenHandler, _idTokenHandler);
        
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            sut.HandleAsync(_validCommand));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidClientId_ThrowsInvalidClientException()
    {
        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
            .Returns((ClientTokenData)null!);
        
        await Assert.ThrowsAsync<InvalidClientException>(() =>
            _sut.HandleAsync(_validCommand));
    }

    [Fact]
    public async Task HandleAsync_WithGrantTypeUnsupportedByClient_ThrowsUnauthorizedClientException()
    {
        var tokenData = TestData.CreateValidTokenData() with
        {
            AllowedGrantTypes = [GrantType.ClientCredentials]
        };
        
        _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
            .Returns(tokenData);
        
        await Assert.ThrowsAsync<UnauthorizedClientException>(() =>
            _sut.HandleAsync(_validCommand));
    }

    [Fact]
    public async Task HandleAsync_ForwardsCorrectDataToAccessTokenHandler()
    {
        AccessTokenContext? forwardedContext = null;
        
        _accessTokenHandler
            .CreateAsync(Arg.Do<AccessTokenContext>(ctx => forwardedContext = ctx))
            .Returns("access_token");

        await _sut.HandleAsync(_validCommand);
        
        Assert.NotNull(forwardedContext);
        Assert.Equal(DefaultValues.ClientId, forwardedContext.ClientId);
        Assert.Equal(DefaultValues.Audience, forwardedContext.Audience);
        Assert.Equal(DefaultValues.Subject, forwardedContext.Subject);
    }

    [Fact]
    public async Task HandleAsync_ForwardsCorrectDataToIdTokenHandler()
    {
        IdTokenContext? forwardedContext = null;
        
        _idTokenHandler
            .CreateAsync(Arg.Do<IdTokenContext>(ctx => forwardedContext = ctx))
            .Returns("id_token");

        await _sut.HandleAsync(_validCommand);
        
        Assert.NotNull(forwardedContext);
        Assert.Equal(DefaultValues.ClientId, forwardedContext.ClientId);
        Assert.Equal(DefaultValues.Subject, forwardedContext.Subject);
        Assert.Equal("openid profile", forwardedContext.Scopes.ToString());
        Assert.NotEqual(0, forwardedContext.AuthTimeInSeconds);
        Assert.NotEqual(0, forwardedContext.LifetimeInSeconds);
    }

    [Fact]
    public async Task HandleAsync_ReturnsValidTokenResult()
    {
        var result = await _sut.HandleAsync(_validCommand);

        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
        Assert.Equal("id_token",  result.IdToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.NotEqual(0, result.ExpiresIn);
    }
}