using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Domain.ApiResources;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationCodeProcessorTests
{
	private sealed class TestContext
	{
		public ApiResourceBuilder Api { get; } = new ApiResourceBuilder();
		public AuthorizationCodeTokenCommandBuilder Command { get; } = new AuthorizationCodeTokenCommandBuilder();
		public AuthorizationGrantBuilder Grant { get; } = new AuthorizationGrantBuilder();
		public ClientTokenData TokenData { get; } = TestData.CreateValidTokenData();
        
		public IApiResourceRepository ApiRepository { get; }
		public IAuthorizationGrantStore GrantStore { get; }
		public IAuthorizationCodeValidator Validator { get; }
		public AuthorizationCodeProcessor Sut { get; }

		public TestContext()
		{
			ApiRepository = Substitute.For<IApiResourceRepository>();
			GrantStore = Substitute.For<IAuthorizationGrantStore>();
			Validator = Substitute.For<IAuthorizationCodeValidator>();
			
			WithApiResource();
			WithGrant();
			WithValidatorResult(TestData.CreateValidAuthorizationCodeValidationResult());
            
			Sut = new AuthorizationCodeProcessor(GrantStore, ApiRepository, Validator);
		}

		public Task<TokenContext> ProcessAsync() => Sut.ProcessAsync(Command.Build(), TokenData);

		private void WithApiResource()
		{
			ApiRepository.GetByAudienceAsync(Arg.Any<AudienceIdentifier>(), Arg.Any<CancellationToken>())
				.Returns(_ => Api.Build());
		}

		private void WithGrant()
		{
			GrantStore.GetAsync(Arg.Any<string>())
				.Returns(_ => Grant.Build());
		}

		public void WithValidatorResult(AuthorizationCodeValidationResult result)
		{
			Validator.ValidateAsync(
					Arg.Any<AuthorizationCodeTokenCommand>(),
					Arg.Any<ClientTokenData>(),
					Arg.Any<AuthorizationGrant>(),
					Arg.Any<ApiResource>(),
					Arg.Any<CancellationToken>())
				.Returns(result);
		}
		
		public void WithValidatorException(Exception ex)
		{
			Validator.ValidateAsync(
					Arg.Any<AuthorizationCodeTokenCommand>(),
					Arg.Any<ClientTokenData>(),
					Arg.Any<AuthorizationGrant>(),
					Arg.Any<ApiResource>(),
					Arg.Any<CancellationToken>())
				.Throws(ex);
		}
	}

	[Fact]
	public async Task ProcessAsync_WithValidData_ReturnTokenContext()
	{
		var context = new TestContext();
		var grant = context.Grant.Build();
		
		var result = await context.ProcessAsync();

		Assert.Equal(grant.Audience, result.Audience);
		Assert.Equal(grant.GrantedScopes, result.Scope);
		Assert.Null(result.OidcContext);
	}

	[Fact]
	public async Task ProcessAsync_WithOidcScopes_IncludesOidcContextInResult()
	{
		var context = new TestContext();
		var grant = context.Grant
			.WithScopes("openid profile")
			.Build();
        
		var result = await context.ProcessAsync();

		var oidc = result.OidcContext;
		Assert.NotNull(oidc);       
		Assert.Equal(DefaultValues.Nonce, oidc.Nonce);
		Assert.Equal(grant.GrantedScopes, oidc.Scopes);
	}
    
	[Fact]
	public async Task ProcessAsync_WithValidRequest_ConsumesAuthorizationGrantAndRemovesFromStore()
	{
		var context = new TestContext();
		var authorizationGrant = context.Grant.Build();
        
		context.GrantStore.GetAsync(authorizationGrant.Code)
			.Returns(authorizationGrant);
        
		await context.ProcessAsync();
        
		Assert.True(authorizationGrant.Consumed);
		await context.GrantStore.Received(1).RemoveAsync(authorizationGrant.Code);
	}

	[Fact]
	public async Task ProcessAsync_WhenValidatorThrows_DoesNotConsumeOrRemoveAuthorizationGrantFromStore()
	{
		var context = new TestContext();
		var authorizationGrant = context.Grant.Build();

		context.WithValidatorException(new InvalidScopeException("invalid scope"));
        
		context.GrantStore.GetAsync(authorizationGrant.Code)
			.Returns(authorizationGrant);
        
		await Assert.ThrowsAsync<InvalidScopeException>(()
			=> context.ProcessAsync());
        
		Assert.False(authorizationGrant.Consumed);
		await context.GrantStore.DidNotReceive().RemoveAsync(Arg.Any<string>());
	}

	[Fact]
	public async Task ProcessAsync_WithInvalidCode_ThrowsInvalidGrantException()
	{
		var context = new TestContext();

		context.GrantStore.GetAsync(Arg.Any<string>())
			.Returns((AuthorizationGrant?)null);
        
		await Assert.ThrowsAsync<InvalidGrantException>(()
			=> context.ProcessAsync());
	}
}