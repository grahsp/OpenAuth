using NSubstitute;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationRequestValidatorTests
{
    private readonly IClientQueryService _clientQueryService;
    private readonly IAuthorizationRequestValidator _sut;

    private readonly AuthorizeCommand _validCommand;

    public AuthorizationRequestValidatorTests()
    {
        _clientQueryService = Substitute.For<IClientQueryService>();
        _sut = new AuthorizationRequestValidator(_clientQueryService);

        _validCommand = TestData.CreateValidAuthorizationCommand();

        _clientQueryService.GetAuthorizationDataAsync(_validCommand.ClientId, Arg.Any<CancellationToken>())
            .Returns(TestData.CreateValidAuthorizationData());
    }

    [Fact]
    public async Task ValidateAsync_WithIncorrectClientId_ThrowsInvalidClientException()
    {
        var command = _validCommand with { ClientId = ClientId.New() };
        
        await Assert.ThrowsAsync<InvalidClientException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAsync_WithUnsupportedResponseType_ThrowsUnsupportedResponseTypeException()
    {
        var command = _validCommand with { ResponseType = "invalid-response-type" };
        
        await Assert.ThrowsAsync<UnsupportedResponseTypeException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidRedirectUri_ThrowsInvalidRedirectUriException()
    {
        var command = _validCommand with { RedirectUri = RedirectUri.Create("https://invalid-uri.com") };
        
        await Assert.ThrowsAsync<InvalidRedirectUriException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAsync_WithRedirectUri_WhenClientHasMultiple_IncludesRequestedRedirectUriInResult()
    {
        var expected = RedirectUri.Create("https://expected.com");
        
        var command = _validCommand with { RedirectUri = expected };
        var authData = TestData.CreateValidAuthorizationData() with
        {
            RedirectUris = [expected, RedirectUri.Create("https://unexpected.se")]
        };
        
        _clientQueryService.GetAuthorizationDataAsync(_validCommand.ClientId, Arg.Any<CancellationToken>())
            .Returns(authData);
        
        var result = await _sut.ValidateAsync(command, CancellationToken.None);
        Assert.Equal(expected, result.RedirectUri);       
    }

    [Fact]
    public async Task ValidateAsync_WithNoRedirectUri_WhileClientHasMultiple_ThrowsNoRedirectUriException()
    {
        var command = _validCommand with { RedirectUri = null! };

        var authData = TestData.CreateValidAuthorizationData() with
        {
            RedirectUris = [RedirectUri.Create("https://example.com"), RedirectUri.Create("https://example.se")]
        };
        
        _clientQueryService.GetAuthorizationDataAsync(_validCommand.ClientId, Arg.Any<CancellationToken>())
            .Returns(authData);
        
        await Assert.ThrowsAsync<InvalidRedirectUriException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAsync_WithNoRedirectUri_WhileClientHasSingle_IncludesRedirectUriInResult()
    {
        var command = _validCommand with { RedirectUri = null! };
        var result = await _sut.ValidateAsync(command, CancellationToken.None);
        
        Assert.Equal(_validCommand.RedirectUri, result.RedirectUri);
    }

    [Fact]
    public async Task ValidateAsync_WithNoScopes_ThrowsInvalidScopeException()
    {
        var command = _validCommand with { Scopes = ScopeCollection.Parse("") };

        await Assert.ThrowsAsync<InvalidScopeException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAsync_WithNoPkce_WhenClientIsConfidential_ReturnsResult()
    {
        var command = _validCommand with { Pkce = null };
        var authData = TestData.CreateValidAuthorizationData() with { IsClientPublic = false };
        
        _clientQueryService.GetAuthorizationDataAsync(_validCommand.ClientId, Arg.Any<CancellationToken>())
            .Returns(authData);
        
        var result = await _sut.ValidateAsync(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ValidateAsync_WithNoPkce_WhenClientIsPublic_ThrowsInvalidRequestException()
    {
        var command = _validCommand with { Pkce = null };
        
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ValidateAsync_WithPkce_WhenClientIsConfidential_ReturnsResult()
    {
        var command = _validCommand with { Pkce = TestData.CreateValidPkce() };
        var authData = TestData.CreateValidAuthorizationData() with { IsClientPublic = false };
        
        _clientQueryService.GetAuthorizationDataAsync(_validCommand.ClientId, Arg.Any<CancellationToken>())
            .Returns(authData);
        
        var result = await _sut.ValidateAsync(command, CancellationToken.None);

        Assert.NotNull(result);       
    }

    [Fact]
    public async Task ValidateAsync_WithPkce_WhenClientIsPublic_ReturnsResult()
    {
        var command = _validCommand with { Pkce = TestData.CreateValidPkce() };
        var authData = TestData.CreateValidAuthorizationData() with { IsClientPublic = true };
        
        _clientQueryService.GetAuthorizationDataAsync(_validCommand.ClientId, Arg.Any<CancellationToken>())
            .Returns(authData);
        
        var result = await _sut.ValidateAsync(command, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ValidateAsync_WithOidcScopesAndMissingOpenId_ThrowsInvalidRequestException()
    {
        var command = _validCommand with { Scopes = ScopeCollection.Parse("profile email")};
        
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));       
    }

    [Fact]
    public async Task ValidateAsync_WithOidsScopesAndMissingNonce_ThrowsInvalidRequestException()
    {
        var command = _validCommand with
        {
            Scopes = ScopeCollection.Parse("openid profile"),
            Nonce = null
        };
        
        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            _sut.ValidateAsync(command, CancellationToken.None));              
    }

    [Fact]
    public async Task ValidateAsync_WithOidcScopesAndNonce_ReturnsResult()
    {
        var command = _validCommand with
        {
            Scopes = ScopeCollection.Parse("openid profile"),
            Nonce = DefaultValues.Nonce
        };
        
        var result = await _sut.ValidateAsync(command, CancellationToken.None);
        
        Assert.Equal(command.Scopes, result.Scopes);
        Assert.Equal(command.Nonce, result.Nonce);
    }

    [Fact]
    public async Task ValidateAsync_WithMixedScopes_ReturnsResult()
    {
        var command = _validCommand with
        {
            Scopes = ScopeCollection.Parse("openid profile read write"),
            Nonce = DefaultValues.Nonce
        };
        
        var result = await _sut.ValidateAsync(command, CancellationToken.None);
        
        Assert.Equal(command.Scopes, result.Scopes);
        Assert.Equal(command.Nonce, result.Nonce);       
    }
}