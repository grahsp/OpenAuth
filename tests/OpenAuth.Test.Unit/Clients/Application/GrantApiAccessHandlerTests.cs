using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.ApiResources;
using OpenAuth.Application.Audiences.Interfaces;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Clients.Commands.GrantApiAccess;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.Clients.Application;

public class GrantApiAccessHandlerTests
{
	private sealed class TestContext
	{
		public IApiResourceRepository Apis { get; } = Substitute.For<IApiResourceRepository>();
		public IClientRepository Clients { get; } = Substitute.For<IClientRepository>();
		public TimeProvider Time { get; } = new FakeTimeProvider();
		
		public GrantApiAccessHandler Sut { get; }

		private ApiResourceBuilder _apiBuilder = new ApiResourceBuilder();
		private ClientBuilder _clientBuilder = new ClientBuilder();

		public ApiResource Api { get; private set; }
		public Client Client { get; private set; }

		public TestContext()
		{
			Sut = new GrantApiAccessHandler(Apis, Clients, Time);
			
			Api = _apiBuilder.Build();
			Client = _clientBuilder.Build();

			WithClient();
			WithApi();
		}

		public Task HandleAsync(string? scopes = null)
		{
			var command = new GrantApiAccessCommand(Client.Id, Api.Id,ScopeCollection.Parse(scopes ?? DefaultValues.Scopes));
			return Sut.HandleAsync(command, CancellationToken.None);
		}
		
		public void WithClient(Action<ClientBuilder>? configure = null)
		{
			configure?.Invoke(_clientBuilder);
			Client = _clientBuilder.Build();
			
			Clients.GetByIdAsync(Client.Id, Arg.Any<CancellationToken>())
				.Returns(Client);
		}

		public void WithApi(Action<ApiResourceBuilder>? configure = null)
		{
			configure?.Invoke(_apiBuilder);
			Api = _apiBuilder.Build();
			
			Apis.GetByIdAsync(Api.Id, Arg.Any<CancellationToken>())
				.Returns(Api);
		}
	}
	
	[Fact]
	public async Task HandleAsync_Succeeds()
	{
		var context = new TestContext();
		
		await context.HandleAsync();
            
		var access = Assert.Single(context.Client.Apis);
		Assert.Equal(context.Api.Id, access.ApiResourceId);
	}
	
	[Fact]
	public async Task HandleAsync_WhenMultiplePermissions_GrantsOnlyRequested()
	{
		var context = new TestContext();
		context.WithApi(builder => builder
			.WithPermission("read")
			.WithPermission("write"));
		
		await context.HandleAsync("write");
            
		var access = Assert.Single(context.Client.Apis);
		var scope = Assert.Single(access.AllowedScopes);
		Assert.Equal("write", scope);
	}
	
	[Fact]
	public async Task HandleAsync_WhenRequestingMultipleValidScopes_GrantsAll()
	{
		var context = new TestContext();
		context.WithApi(builder => builder.WithScopes("read write"));

		await context.HandleAsync("read write");

		var access = Assert.Single(context.Client.Apis);
		Assert.Equal(2, access.AllowedScopes.Count);
	}
	
	[Fact]
	public async Task HandleAsync_WhenApiAlreadyGranted_Throws()
	{
		var context = new TestContext();
		
		await context.HandleAsync();
		await Assert.ThrowsAsync<InvalidOperationException>(() => context.HandleAsync());
	}
	
	[Fact]
	public async Task HandleAsync_WhenRequestingNonExistentPermission_Throws()
	{
		var context = new TestContext();
		context.WithApi(builder => builder.WithPermission("write"));
	
		await Assert.ThrowsAsync<InvalidScopeException>(() => context.HandleAsync("read"));
	}
	
	[Fact]
	public async Task HandleAsync_WhenClientNotFound_Throws()
	{
		var context = new TestContext();
		var command = new GrantApiAccessCommand(ClientId.New(), context.Api.Id, ScopeCollection.Parse(""));
		
		await Assert.ThrowsAsync<ClientNotFoundException>(() => context.Sut.HandleAsync(command, CancellationToken.None));
	}
	
	[Fact]
	public async Task HandleAsync_WhenApiNotFound_Throws()
	{
		var context = new TestContext();
		var command = new GrantApiAccessCommand(context.Client.Id, ApiResourceId.New(), ScopeCollection.Parse(""));
		
		await Assert.ThrowsAsync<ApiResourceNotFoundException>(() => context.Sut.HandleAsync(command, CancellationToken.None));
	}
}