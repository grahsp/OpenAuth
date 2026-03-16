using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Clients.Commands.RevokeApiAccess;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Domain.ApiResources;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Application;

public class RevokeApiAccessHandlerTests
{
	private sealed class TestContext
	{
		public IClientRepository Clients { get; } = Substitute.For<IClientRepository>();
		public TimeProvider Time { get; } = new FakeTimeProvider();
		
		public RevokeApiAccessHandler Sut { get; }

		private readonly ClientBuilder _clientBuilder = new ClientBuilder();
		
		public ApiResource Api { get; }
		public Client Client { get; private set; }

		public TestContext()
		{
			Sut = new RevokeApiAccessHandler(Clients, Time);
			
			Api = new ApiResourceBuilder().Build();
			Client = _clientBuilder
				.WithApi(Api)
				.Build();

			WithClient();
		}

		public Task HandleAsync(ApiResourceId? apiResourceId = null)
		{
			var command = new RevokeApiAccessCommand(Client.Id, apiResourceId ?? Api.Id);
			return Sut.HandleAsync(command, CancellationToken.None);
		}
		
		public void WithClient(Action<ClientBuilder>? configure = null)
		{
			configure?.Invoke(_clientBuilder);
			Client = _clientBuilder.Build();
			
			Clients.GetByIdAsync(Client.Id, Arg.Any<CancellationToken>())
				.Returns(Client);
		}
	}

	[Fact]
	public async Task HandleAsync_WhenClientHasAccessToApi_RevokeAccess()
	{
		// Client has by default access to one API
		var context = new TestContext();
		
		Assert.Single(context.Client.Apis);
		await context.HandleAsync();
		
		Assert.Empty(context.Client.Apis);
	}
	
	[Fact]
	public async Task HandleAsync_WhenAccessAlreadyRevoked_DoesNothing()
	{
		var context = new TestContext();

		await context.HandleAsync(); // revoke first time
		Assert.Empty(context.Client.Apis);

		await context.HandleAsync(); // revoke again
		Assert.Empty(context.Client.Apis);
	}    
	
	[Fact]
	public async Task HandleAsync_WhenClientHasNoAccessToApi_DoesNothing()
	{
		var context = new TestContext();
		
		var before = Assert.Single(context.Client.Apis);
		await context.HandleAsync(ApiResourceId.New());
		
		var after = Assert.Single(context.Client.Apis);
		Assert.Equal(before, after);
	}
    
	[Fact]
	public async Task HandleAsync_WhenClientNotFound_Throws()
	{
		var context = new TestContext();
		var command = new RevokeApiAccessCommand(ClientId.New(), ApiResourceId.New());
		
		await Assert.ThrowsAsync<ClientNotFoundException>(() =>
			context.Sut.HandleAsync(command, CancellationToken.None));
	}
}