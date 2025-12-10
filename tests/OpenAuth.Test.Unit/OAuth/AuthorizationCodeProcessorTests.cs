using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationCodeProcessorTests
{
    private readonly IAuthorizationGrantStore _grantStore;
    private readonly IAuthorizationCodeValidator _validator;
    private readonly AuthorizationCodeProcessor _sut;

    private readonly AuthorizationCodeTokenCommand _validCommand;
    private readonly ClientTokenData _validTokenData;
    
    public AuthorizationCodeProcessorTests()
    {
        _grantStore = Substitute.For<IAuthorizationGrantStore>();
        _validator = Substitute.For<IAuthorizationCodeValidator>();
        _sut = new AuthorizationCodeProcessor(_grantStore, _validator);

        _grantStore.GetAsync(DefaultValues.Code)
            .Returns(TestData.CreateValidAuthorizationGrant());

        _validator.ValidateAsync(Arg.Any<AuthorizationCodeValidatorContext>(), Arg.Any<CancellationToken>())
            .Returns(TestData.CreateValidAuthorizationCodeValidationResult());

        _validCommand = TestData.CreateValidAuthorizationCodeTokenCommand();
        _validTokenData = TestData.CreateValidTokenData();
    }

    [Fact]
    public async Task ProcessAsync_WithValidData_ReturnTokenContext()
    {
        var result = await _sut.ProcessAsync(_validCommand, _validTokenData);

        Assert.Equal(DefaultValues.Audience, result.Audience);
        Assert.Equal(DefaultValues.Subject, result.Subject);
        Assert.Equal(DefaultValues.Scopes, result.Scope.ToString());
        Assert.Null(result.OidcContext);
    }

    [Fact]
    public async Task ProcessAsync_WithOidcScopes_IncludesOidcContextInResult()
    {
        const string scopes = "openid profile";
        var validationResult = TestData.CreateValidAuthorizationCodeValidationResult() with
        {
            OidcScope = ScopeCollection.Parse(scopes)
        };
        
        _validator.ValidateAsync(Arg.Any<AuthorizationCodeValidatorContext>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);
        
        var result = await _sut.ProcessAsync(_validCommand, _validTokenData);

        var ctx = result.OidcContext;
        Assert.NotNull(ctx);       
        Assert.Equal(DefaultValues.Nonce, ctx.Nonce);
        Assert.Equal(scopes, ctx.Scopes.ToString());
    }
    
    [Fact]
    public async Task ProcessAsync_PassesCorrectContextToValidator()
    {
        AuthorizationCodeValidatorContext? passedContext = null;

        _validator.ValidateAsync(
                Arg.Do<AuthorizationCodeValidatorContext>(ctx => passedContext = ctx),
                Arg.Any<CancellationToken>())
            .Returns(TestData.CreateValidAuthorizationCodeValidationResult());

        var grant = TestData.CreateValidAuthorizationGrant();
        _grantStore.GetAsync(DefaultValues.Code).Returns(grant);

        await _sut.ProcessAsync(_validCommand, _validTokenData);

        Assert.NotNull(passedContext);
        Assert.Equal(_validCommand, passedContext.Command);
        Assert.Equal(_validTokenData, passedContext.TokenData);
        Assert.Equal(grant, passedContext.AuthorizationGrant);
    }
    
    [Fact]
    public async Task ProcessAsync_WithValidRequest_ConsumesAuthorizationGrantAndRemovesFromStore()
    {
        var authorizationGrant = TestData.CreateValidAuthorizationGrant();
        
        _grantStore.GetAsync(DefaultValues.Code)
            .Returns(authorizationGrant);
        
        await _sut.ProcessAsync(_validCommand, _validTokenData);
        
        Assert.True(authorizationGrant.Consumed);
        await _grantStore.Received(1).RemoveAsync(DefaultValues.Code);
    }

    [Fact]
    public async Task ProcessAsync_WhenValidatorThrows_DoesNotConsumeOrRemoveAuthorizationGrantFromStore()
    {
        var authorizationGrant = TestData.CreateValidAuthorizationGrant();

        _validator.ValidateAsync(Arg.Any<AuthorizationCodeValidatorContext>(), Arg.Any<CancellationToken>())
            .Throws(new InvalidScopeException("invalid scope"));
        
        _grantStore.GetAsync(DefaultValues.Code)
            .Returns(authorizationGrant);
        
        await Assert.ThrowsAsync<InvalidScopeException>(()
            => _sut.ProcessAsync(_validCommand, _validTokenData));
        
        Assert.False(authorizationGrant.Consumed);
        await _grantStore.DidNotReceive().RemoveAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidCode_ThrowsInvalidGrantException()
    {
        var command = _validCommand with
        {
            Code = "invalid-code"
        };
        
        await Assert.ThrowsAsync<InvalidGrantException>(()
            => _sut.ProcessAsync(command, _validTokenData));
    }
}