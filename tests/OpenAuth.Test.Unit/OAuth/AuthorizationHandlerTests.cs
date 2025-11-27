using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationHandlerTests
{
    private readonly IAuthorizationRequestValidator _validator;
    private readonly IAuthorizationGrantStore _store;
    private readonly FakeTimeProvider _time;

    private readonly AuthorizationHandler _sut;

    private readonly AuthorizeCommand _validCommand;

    public AuthorizationHandlerTests()
    {
        _validator = Substitute.For<IAuthorizationRequestValidator>();
        _store = Substitute.For<IAuthorizationGrantStore>();
        _time = new FakeTimeProvider();

        _sut = new AuthorizationHandler(_validator, _store, _time);

        _validCommand = TestData.CreateValidAuthorizationCommand();

        _validator.ValidateAsync(Arg.Any<AuthorizeCommand>(), Arg.Any<CancellationToken>())
            .Returns(TestData.CreateValidAuthorizationValidationResult());

        _validCommand = TestData.CreateValidAuthorizationCommand();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidRequest_PropagatesException()
    {
        _validator.ValidateAsync(Arg.Any<AuthorizeCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidScopeException(""));

        await Assert.ThrowsAsync<InvalidScopeException>(() =>
            _sut.HandleAsync(_validCommand));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidRequest_DoesNotStoreAuthorizationGrant()
    {
        _validator.ValidateAsync(Arg.Any<AuthorizeCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidScopeException(""));

        await Assert.ThrowsAsync<InvalidScopeException>(() =>
            _sut.HandleAsync(_validCommand));

        await _store.DidNotReceive().AddAsync(Arg.Any<AuthorizationGrant>());
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_CreatesCorrectAuthorizationGrant()
    {
        var result = await _sut.HandleAsync(_validCommand);

        Assert.NotEmpty(result.Code);
        Assert.Equal(_validCommand.ClientId, result.ClientId);
        Assert.Equal(_validCommand.RedirectUri, result.RedirectUri);
        Assert.Equal(_validCommand.Scopes, result.GrantedScopes);
        Assert.Equal(_validCommand.Subject, result.Subject);
        Assert.Equal(_validCommand.Pkce, result.Pkce);
        Assert.Equal(_validCommand.Nonce, result.Nonce);
        Assert.Equal(_time.GetUtcNow(), result.CreatedAt);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_StoresAuthorizationGrant()
    {
        var result = await _sut.HandleAsync(_validCommand);
        await _store.Received(1).AddAsync(result);
    }
}