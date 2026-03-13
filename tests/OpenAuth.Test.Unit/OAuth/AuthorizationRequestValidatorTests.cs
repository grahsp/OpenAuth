using NSubstitute;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

public class AuthorizationRequestValidatorTests
{
	private sealed class TestContext
	{
		public AuthorizeCommandBuilder Command { get; } = new AuthorizeCommandBuilder();
	
		public IApiResourceRepository ApiResourceRepository { get; }
		public IClientQueryService ClientQueryService { get; }
		public AuthorizationRequestValidator Sut { get; }

		public TestContext()
		{
			ApiResourceRepository = Substitute.For<IApiResourceRepository>();
			ClientQueryService = Substitute.For<IClientQueryService>();

			WithClient();
			WithApiResource();
        
			Sut = new AuthorizationRequestValidator(ApiResourceRepository, ClientQueryService);       
		}

		public void WithClient()
		{
			ClientQueryService
				.GetAuthorizationDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
				.Returns(TestData.CreateValidAuthorizationData());
		}

		public void WithApiResource()
		{
			ApiResourceRepository
				.GetByAudienceAsync(Arg.Any<AudienceIdentifier>(), Arg.Any<CancellationToken>())
				.Returns(new ApiBuilder().Build());
		}
	}
	
	[Fact]
	public async Task ValidateAsync_WithIncorrectClientId_ThrowsInvalidClientException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithClientId(ClientId.New())
			.Build();
        
		await Assert.ThrowsAsync<InvalidClientException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));
	}

	[Fact]
	public async Task ValidateAsync_WithUnsupportedResponseType_ThrowsUnsupportedResponseTypeException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithResponseType("unsupported")
			.Build();
        
		await Assert.ThrowsAsync<UnsupportedResponseTypeException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));
	}

	[Fact]
	public async Task ValidateAsync_WithInvalidRedirectUri_ThrowsInvalidRedirectUriException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithRedirectUri("invalid-redirect-uri")
			.Build();
        
		await Assert.ThrowsAsync<InvalidRedirectUriException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));
	}

	[Fact]
	public async Task ValidateAsync_WithRedirectUri_WhenClientHasMultiple_IncludesRequestedRedirectUriInResult()
	{
		const string expected = "https://expected.com";
		var context = new TestContext();

		var command = context.Command
			.WithRedirectUri(expected)
			.Build();
		
		var clientAuthorizationData = TestData.CreateValidAuthorizationData() with
		{
			RedirectUris = [
				command.RedirectUri,
				RedirectUri.Create("https://unexpected.com")]
		};
        
		context.ClientQueryService.GetAuthorizationDataAsync(command.ClientId, Arg.Any<CancellationToken>())
			.Returns(clientAuthorizationData);
        
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
		Assert.Equal(expected, result.RedirectUri.Value);       
	}

	[Fact]
	public async Task ValidateAsync_WithNoRedirectUri_WhileClientHasMultiple_ThrowsNoRedirectUriException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithRedirectUri(null)
			.Build();

		var clientAuthorizationData = TestData.CreateValidAuthorizationData() with
		{
			RedirectUris = [
				RedirectUri.Create("https://example.com"),
				RedirectUri.Create("https://example.se")]
		};
        
		context.ClientQueryService.GetAuthorizationDataAsync(command.ClientId, Arg.Any<CancellationToken>())
			.Returns(clientAuthorizationData);
        
		await Assert.ThrowsAsync<InvalidRedirectUriException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));
	}

	[Fact]
	public async Task ValidateAsync_WithNoRedirectUri_WhileClientHasSingle_IncludesRedirectUriInResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithRedirectUri(null)
			.Build();
		
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
		Assert.Equal(command.RedirectUri, result.RedirectUri);
	}

	[Fact]
	public async Task ValidateAsync_WithNoScopes_ThrowsInvalidScopeException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithScope("")
			.Build();

		await Assert.ThrowsAsync<InvalidScopeException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));
	}

	[Fact]
	public async Task ValidateAsync_WithNoPkce_WhenClientIsConfidential_ReturnsResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithPkce(null)
			.Build();
		
		var authorizationClientData = TestData.CreateValidAuthorizationData() with
			{ IsClientPublic = false };
        
		context.ClientQueryService.GetAuthorizationDataAsync(command.ClientId, Arg.Any<CancellationToken>())
			.Returns(authorizationClientData);
        
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
		Assert.NotNull(result);
	}

	[Fact]
	public async Task ValidateAsync_WithNoPkce_WhenClientIsPublic_ThrowsInvalidRequestException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithPkce(null)
			.Build();
        
		await Assert.ThrowsAsync<InvalidRequestException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));
	}

	[Fact]
	public async Task ValidateAsync_WithPkce_WhenClientIsConfidential_ReturnsResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithPkce(TestData.CreateValidPkce())
			.Build();
		
		var clientAuthorizationData = TestData.CreateValidAuthorizationData() with
			{ IsClientPublic = false };
        
		context.ClientQueryService.GetAuthorizationDataAsync(command.ClientId, Arg.Any<CancellationToken>())
			.Returns(clientAuthorizationData);
        
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
		Assert.NotNull(result);       
	}

	[Fact]
	public async Task ValidateAsync_WithPkce_WhenClientIsPublic_ReturnsResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithPkce(TestData.CreateValidPkce())
			.Build();
		
		var clientAuthorizationData = TestData.CreateValidAuthorizationData() with
			{ IsClientPublic = true };
		context.ClientQueryService.GetAuthorizationDataAsync(command.ClientId, Arg.Any<CancellationToken>())
			.Returns(clientAuthorizationData);
        
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
		Assert.NotNull(result);
	}

	[Fact]
	public async Task ValidateAsync_WithOidcScopesAndMissingOpenId_ThrowsInvalidRequestException()
	{
		var context = new TestContext();
		var command = context.Command
			.WithScope("profile email")
			.Build();
        
		await Assert.ThrowsAsync<InvalidRequestException>(() =>
			context.Sut.ValidateAsync(command, CancellationToken.None));       
	}

	[Fact]
	public async Task ValidateAsync_WithOidcScopesAndNonce_ReturnsResult()
	{
		var context = new TestContext();
		var command = context.Command
			.WithScope("openid profile")
			.WithNonce("nonce")
			.Build();
		
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
        
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
        
		var result = await context.Sut.ValidateAsync(command, CancellationToken.None);
        
		Assert.Equal(command.Scopes, result.Scopes);
		Assert.Equal(command.Nonce, result.Nonce);       
	}
}