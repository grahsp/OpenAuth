using NSubstitute;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Queries.GetClientApiScopes;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationRequestValidatorTests
{
	private sealed class TestContext
	{
		public ApiResourceBuilder Api { get; } = new ApiResourceBuilder();
		public AuthorizeCommandBuilder Command { get; } = new AuthorizeCommandBuilder();
		
		public ClientAuthorizationData AuthorizationData { get; set; } = TestData.CreateValidAuthorizationData();
	
		public IQueryHandler<GetClientApiScopesQuery, ScopeCollection> ScopeHandler { get; }
		public IClientQueryService ClientQueryService { get; }
		public AuthorizationRequestValidator Sut { get; }

		public TestContext()
		{
			ScopeHandler = Substitute.For<IQueryHandler<GetClientApiScopesQuery, ScopeCollection>>();
			ClientQueryService = Substitute.For<IClientQueryService>();

			WithClient();
			WithApiResource();
			
			Command.WithPkce(TestData.CreateValidPkce());
        
			Sut = new AuthorizationRequestValidator(ScopeHandler, ClientQueryService);
		}

		public Task<AuthorizationValidationResult> ValidateAsync() =>
			Sut.ValidateAsync(Command.Build());

		public void WithClient()
		{
			ClientQueryService
				.GetAuthorizationDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
				.Returns(_ => AuthorizationData);
		}

		public void WithApiResource()
		{
			ScopeHandler
				.HandleAsync(Arg.Any<GetClientApiScopesQuery>(), Arg.Any<CancellationToken>())
				.Returns<Task<ScopeCollection>>(_ =>
				{
					var api = Api.Build();
					var scopes = new ScopeCollection(api.Permissions.Select(x => x.Scope));
					
					return Task.FromResult(scopes);
				});
		}
	}

	[Fact]
	public async Task ValidateAsync_WithUnsupportedResponseType_ThrowsUnsupportedResponseTypeException()
	{
		var context = new TestContext();
		context.Command.WithResponseType("unsupported");
        
		await Assert.ThrowsAsync<UnsupportedResponseTypeException>(() => context.ValidateAsync());
	}

	[Fact]
	public async Task ValidateAsync_WithInvalidRedirectUri_ThrowsInvalidRedirectUriException()
	{
		var context = new TestContext();
		context.Command.WithRedirectUri("https://invalid.com/");
		
		await Assert.ThrowsAsync<InvalidRedirectUriException>(() => context.ValidateAsync());
	}

	[Fact]
	public async Task ValidateAsync_WithRedirectUri_WhenClientHasMultiple_IncludesRequestedRedirectUriInResult()
	{
		var context = new TestContext();
		
		const string expected = "https://expected.com/";
		context.Command.WithRedirectUri(expected);
        
		context.AuthorizationData = context.AuthorizationData with
			{ RedirectUris = [RedirectUri.Parse(expected), RedirectUri.Parse("https://unexpected.com/")] };
        
		var result = await context.ValidateAsync();
		Assert.Equal(expected, result.RedirectUri.Value);       
	}

	[Fact]
	public async Task ValidateAsync_WithNoRedirectUri_WhileClientHasMultiple_ThrowsNoRedirectUriException()
	{
		var context = new TestContext();
		context.Command.WithRedirectUri("https://invalid.com/");

		context.AuthorizationData = context.AuthorizationData with
		{
			RedirectUris = [
				RedirectUri.Parse("https://example.com"),
				RedirectUri.Parse("https://example.org")]
		};
        
		await Assert.ThrowsAsync<InvalidRedirectUriException>(() => context.ValidateAsync());
	}

	[Fact]
	public async Task ValidateAsync_WithNoScopes_ThrowsInvalidScopeException()
	{
		var context = new TestContext();
		context.Command.WithScope("");

		await Assert.ThrowsAsync<InvalidScopeException>(() => context.ValidateAsync());
	}

	[Fact]
	public async Task ValidateAsync_WithNoPkce_WhenClientIsConfidential_ReturnsResult()
	{
		var context = new TestContext();
		context.Command.WithPkce(null);
		
		context.AuthorizationData = context.AuthorizationData with
			{ IsClientPublic = false };
        
		var result = await context.ValidateAsync();
		Assert.NotNull(result);
	}

	[Fact]
	public async Task ValidateAsync_WithNoPkce_WhenClientIsPublic_ThrowsInvalidRequestException()
	{
		var context = new TestContext();
		context.Command.WithPkce(null);
        
		await Assert.ThrowsAsync<InvalidRequestException>(() => context.ValidateAsync());
	}

	[Fact]
	public async Task ValidateAsync_WithPkce_WhenClientIsConfidential_ReturnsResult()
	{
		var context = new TestContext();
		context.Command.WithPkce(TestData.CreateValidPkce());
		
		context.AuthorizationData = context.AuthorizationData with
			{ IsClientPublic = false };
        
		var result = await context.ValidateAsync();
		Assert.NotNull(result);       
	}

	[Fact]
	public async Task ValidateAsync_WithPkce_WhenClientIsPublic_ReturnsResult()
	{
		var context = new TestContext();
		context.Command.WithPkce(TestData.CreateValidPkce());
		
		context.AuthorizationData = context.AuthorizationData with
			{ IsClientPublic = true};
        
		var result = await context.ValidateAsync();
		Assert.NotNull(result);
	}

	[Fact]
	public async Task ValidateAsync_WithOidcScopesAndMissingOpenId_ThrowsInvalidRequestException()
	{
		var context = new TestContext();
		context.Command.WithScope("profile email");
        
		await Assert.ThrowsAsync<InvalidRequestException>(() => context.ValidateAsync());       
	}

	[Fact]
	public async Task ValidateAsync_WithOidcScopesAndNonce_ReturnsResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithScope("openid profile")
			.WithNonce("nonce")
			.Build();
		
		var result = await context.ValidateAsync();
        
		Assert.Equal(command.Scopes, result.Scopes);
		Assert.Equal(command.Nonce, result.Nonce);
	}

	[Fact]
	public async Task ValidateAsync_WithMixedScopes_ReturnsResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithScope("openid profile read write")
			.WithNonce("nonce")
			.Build();
        
		var result = await context.ValidateAsync();
        
		Assert.Equal(command.Scopes, result.Scopes);
		Assert.Equal(command.Nonce, result.Nonce);       
	}
}