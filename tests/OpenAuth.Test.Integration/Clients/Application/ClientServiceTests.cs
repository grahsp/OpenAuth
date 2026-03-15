using Microsoft.EntityFrameworkCore;
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
		await fixture.ResetAsync(_host);
	}
    
	public async Task DisposeAsync() => await _host.DisposeAsync();


	private async Task<Client> GetClient(TestScope scope, ClientId id)
	{
		var context = scope.Resolve<AppDbContext>();
		var client = await context.Clients
			.SingleOrDefaultAsync(c => c.Id == id);

		return client!;
	}

	private async Task<IEnumerable<Secret>> GetClientSecrets(TestScope scope, ClientId id)
	{
		await using var context = scope.Resolve<AppDbContext>();
		return await context.ClientSecrets
			.Where(s => s.ClientId == id)
			.ToListAsync();
	}
    
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
		await using var scope = _host.CreateScope();
		
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateM2MRequest());
        
		var client = await GetClient(scope, result.Client.Id);
		Assert.NotNull(client);
		Assert.Equal(result.Client.Name, client.Name);
        
		var secrets = await GetClientSecrets(scope, result.Client.Id);
		Assert.Single(secrets);
	}

	[Fact]
	public async Task RegisterAsync_PersistsPublicClientWithoutSecrets()
	{
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateSpaRequest());
        
		var client = await GetClient(scope, result.Client.Id);
		Assert.NotNull(client);
		Assert.Equal(result.Client.Name, client.Name);

		var secrets = await GetClientSecrets(scope, result.Client.Id);
		Assert.Empty(secrets);
	}
    
	[Fact]
	public async Task RenameAsync_PersistsUpdatedName()
	{
		var expected = new ClientName("new client");
		
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateSpaRequest());
            
		await sut.RenameAsync(result.Client.Id, expected);

		var client = await GetClient(scope, result.Client.Id);
		Assert.Equal(expected, client.Name);
	}
    
	[Fact]
	public async Task SetGrantTypesAsync_PersistsUpdate()
	{
		var grantTypes = new[] { GrantType.ClientCredentials, GrantType.RefreshToken };
        
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateM2MRequest());
            
		await sut.SetGrantTypesAsync(result.Client.Id, grantTypes);
			
		var client = await GetClient(scope, result.Client.Id);
		Assert.Equal(grantTypes.Length, client.AllowedGrantTypes.Count);
		Assert.All(grantTypes, r => Assert.Contains(r, client.AllowedGrantTypes));
	}
    
	[Fact]
	public async Task AddAndRemoveGrantTypeAsync_PersistsUpdate()
	{
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateM2MRequest());
            
		// M2M clients already contain ClientCredentials by default
		await sut.AddGrantTypeAsync(result.Client.Id, GrantType.RefreshToken);
		await sut.RemoveGrantTypeAsync(result.Client.Id, GrantType.ClientCredentials);

		var client = await GetClient(scope, result.Client.Id);
		Assert.Null(client.AllowedGrantTypes.SingleOrDefault(r => r == GrantType.ClientCredentials));
		Assert.NotNull(client.AllowedGrantTypes.SingleOrDefault(r => r == GrantType.RefreshToken));
	}
    
	[Fact]
	public async Task SetRedirectUrisAsync_PersistsUpdate()
	{
		var redirectUris = new[] { UriA, UriB };
        
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateSpaRequest());

		await sut.SetRedirectUrisAsync(result.Client.Id, redirectUris);

		var client = await GetClient(scope, result.Client.Id);
		Assert.Equal(redirectUris.Length, client.RedirectUris.Count);
		Assert.All(redirectUris, r => Assert.Contains(r, client.RedirectUris));
	}
    
	[Fact]
	public async Task AddAndRemoveRedirectUriAsync_PersistsUpdate()
	{
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateSpaRequest());

		await sut.AddRedirectUriAsync(result.Client.Id, UriA);
		await sut.AddRedirectUriAsync(result.Client.Id, UriB);
		await sut.RemoveRedirectUriAsync(result.Client.Id, UriA);
			
		var client = await GetClient(scope, result.Client.Id);
		Assert.Null(client.RedirectUris.SingleOrDefault(r => r == UriA));
		Assert.NotNull(client.RedirectUris.SingleOrDefault(r => r == UriB));
	}
    
	[Fact]
	public async Task DeleteAsync_RemovesClient()
	{
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateSpaRequest());
            
		await sut.DeleteAsync(result.Client.Id);

		var client = await GetClient(scope, result.Client.Id);
		Assert.Null(client);
	}
     
	[Fact]
	public async Task EnableAndDisableAsync_TogglesAndPersistsFlag()
	{
		await using var scope = _host.CreateScope();
		var sut = scope.Resolve<IClientService>();
		var result = await sut.RegisterAsync(CreateSpaRequest());

		await sut.DisableAsync(result.Client.Id);
		var disabledClient = await GetClient(scope, result.Client.Id);
		Assert.False(disabledClient.Enabled);
         
		await sut.EnableAsync(result.Client.Id);
		var enabledClient = await GetClient(scope, result.Client.Id);
		Assert.True(enabledClient.Enabled);
	}
}