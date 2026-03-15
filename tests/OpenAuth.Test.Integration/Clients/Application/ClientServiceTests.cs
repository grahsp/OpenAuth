using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Fixtures;
using OpenAuth.Test.Common.Hosting;

namespace OpenAuth.Test.Integration.Clients.Application;

// TODO: add missing tests for api methods
public class ClientServiceTests(TestFixture fixture) : IClassFixture<TestFixture>, IAsyncLifetime
{
	private TestHost _host = null!;

	public async Task InitializeAsync()
	{
		_host = fixture.CreateDefaultHost();
		await fixture.ResetAsync();
	}
    
	public async Task DisposeAsync() => await _host.DisposeAsync();
    
	
	private async Task<Client> GetClient(ClientId id) =>
		await _host.WithScopeAsync<Client>(async sp =>
		{
			await using var context = sp.GetRequiredService<AppDbContext>();
			var client = await context.Clients
				.SingleOrDefaultAsync(c => c.Id == id);

			return client!;
		});
	
	private async Task<IEnumerable<Secret>> GetClientSecrets(ClientId id) =>
		await _host.WithScopeAsync<IEnumerable<Secret>>(async sp =>
		{
			await using var context = sp.GetRequiredService<AppDbContext>();
			return await context.ClientSecrets
				.Where(s => s.ClientId == id)
				.ToListAsync();
		});
    
	private static CreateClientRequest CreateM2MRequest()
		=> new CreateClientRequest(
			ClientApplicationTypes.M2M,
			ClientName.Create("test client"),
			[RedirectUri.Create("https://example.com/callback")]);

	private static CreateClientRequest CreateSpaRequest()
		=> new CreateClientRequest(
			ClientApplicationTypes.Spa,
			ClientName.Create("test client"),
			[RedirectUri.Create("https://example.com/callback")]);
    
	private static readonly RedirectUri UriA = RedirectUri.Create("https://a.com/callback");
	private static readonly RedirectUri UriB = RedirectUri.Create("https://b.com/callback");

    
	[Fact]
	public async Task RegisterAsync_PersistsConfidentialClientWithSecret()
	{
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			return await sut.RegisterAsync(CreateM2MRequest());
		});
        
		var client = await GetClient(result.Client.Id);
		Assert.NotNull(client);
		Assert.Equal(result.Client.Name, client.Name);
        
		var secrets = await GetClientSecrets(result.Client.Id);
		Assert.Single(secrets);
	}

	[Fact]
	public async Task RegisterAsync_PersistsPublicClientWithoutSecrets()
	{
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			return await sut.RegisterAsync(CreateSpaRequest());
		});
        
		var client = await GetClient(result.Client.Id);
		Assert.NotNull(client);
		Assert.Equal(result.Client.Name, client.Name);

		var secrets = await GetClientSecrets(result.Client.Id);
		Assert.Empty(secrets);
	}
    
	[Fact]
	public async Task RenameAsync_PersistsUpdatedName()
	{
		var expected = new ClientName("new client");
        
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateSpaRequest());
            
			await sut.RenameAsync(result.Client.Id, expected);

			return result;
		});
         

		var client = await GetClient(result.Client.Id);
		Assert.Equal(expected, client.Name);
	}
    
	[Fact]
	public async Task SetGrantTypesAsync_PersistsUpdate()
	{
		var grantTypes = new[] { GrantType.ClientCredentials, GrantType.RefreshToken };
        
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateM2MRequest());
            
			await sut.SetGrantTypesAsync(result.Client.Id, grantTypes);

			return result;
		});

		var client = await GetClient(result.Client.Id);
		Assert.Equal(grantTypes.Length, client.AllowedGrantTypes.Count);
		Assert.All(grantTypes, r => Assert.Contains(r, client.AllowedGrantTypes));
	}
    
	[Fact]
	public async Task AddAndRemoveGrantTypeAsync_PersistsUpdate()
	{
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateM2MRequest());
            
			// M2M clients already contain ClientCredentials by default
			await sut.AddGrantTypeAsync(result.Client.Id, GrantType.RefreshToken);
			await sut.RemoveGrantTypeAsync(result.Client.Id, GrantType.ClientCredentials);

			return result;
		});
             
		var client = await GetClient(result.Client.Id);
		Assert.Null(client.AllowedGrantTypes.SingleOrDefault(r => r == GrantType.ClientCredentials));
		Assert.NotNull(client.AllowedGrantTypes.SingleOrDefault(r => r == GrantType.RefreshToken));
	}
    
	[Fact]
	public async Task SetRedirectUrisAsync_PersistsUpdate()
	{
		var redirectUris = new[] { UriA, UriB };
        
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateSpaRequest());

			await sut.SetRedirectUrisAsync(result.Client.Id, redirectUris);

			return result;
		});

		var client = await GetClient(result.Client.Id);
		Assert.Equal(redirectUris.Length, client.RedirectUris.Count);
		Assert.All(redirectUris, r => Assert.Contains(r, client.RedirectUris));
	}
    
	[Fact]
	public async Task AddAndRemoveRedirectUriAsync_PersistsUpdate()
	{
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateSpaRequest());

			await sut.AddRedirectUriAsync(result.Client.Id, UriA);
			await sut.AddRedirectUriAsync(result.Client.Id, UriB);
			await sut.RemoveRedirectUriAsync(result.Client.Id, UriA);
            
			return result;
		});

		var client = await GetClient(result.Client.Id);
		Assert.Null(client.RedirectUris.SingleOrDefault(r => r == UriA));
		Assert.NotNull(client.RedirectUris.SingleOrDefault(r => r == UriB));
	}
    
	[Fact]
	public async Task DeleteAsync_RemovesClient()
	{
		var result = await _host.WithScopeAsync<RegisteredClientResponse>(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateSpaRequest());
            
			await sut.DeleteAsync(result.Client.Id);
            
			return result;
		});

		var client = await GetClient(result.Client.Id);
		Assert.Null(client);
	}
     
	[Fact]
	public async Task EnableAndDisableAsync_TogglesAndPersistsFlag()
	{
		await _host.WithScopeAsync(async sp =>
		{
			var sut = sp.GetRequiredService<IClientService>();
			var result = await sut.RegisterAsync(CreateSpaRequest());

			await sut.DisableAsync(result.Client.Id);
			var disabledClient = await GetClient(result.Client.Id);
			Assert.False(disabledClient.Enabled);
         
			await sut.EnableAsync(result.Client.Id);
			var enabledClient = await GetClient(result.Client.Id);
			Assert.True(enabledClient.Enabled);
		});
	}
}