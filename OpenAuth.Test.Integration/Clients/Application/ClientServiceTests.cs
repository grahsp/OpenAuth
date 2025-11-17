using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.Factories;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Test.Integration.Infrastructure;

namespace OpenAuth.Test.Integration.Clients.Application;


[Collection("sqlserver")]
public class ClientServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly ClientService _sut;

    public ClientServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        var time = TimeProvider.System;
        
        var context = _fx.CreateContext();
        var repo = new ClientRepository(context);
        
        var secretGenerator = new SecretGenerator();
        var hasher = new Pbkdf2Hasher();
        var hashProvider = new SecretHashProvider(secretGenerator, hasher);
        
        var clientFactory = new ClientFactory(hashProvider, time);
        
        var configFactory = new ClientConfigurationFactory();

        _sut = new ClientService(repo, clientFactory, configFactory, time);
    }
    
    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    
    private static RegisterClientCommand CreateM2MRequest()
        => new(
            ApplicationType: "m2m", 
            Name: "test client", 
            Permissions: new Dictionary<string, IEnumerable<string>>
                { { "api", ["read", "write"] } }
        );
    
    private static RegisterClientCommand CreateSpaRequest()
        => new(
            ApplicationType: "spa", 
            Name: "test client", 
            RedirectUris: ["https://example.com/callback"]);
    
    private static readonly Audience ApiAudience = new(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
    private static readonly Audience WebAudience = new(AudienceName.Create("web"), ScopeCollection.Parse("read"));
    
    private static readonly RedirectUri UriA = RedirectUri.Create("https://a.com/callback");
    private static readonly RedirectUri UriB = RedirectUri.Create("https://b.com/callback");

    
    [Fact]
    public async Task RegisterAsync_PersistsConfidentialClientWithSecret()
    {
        var result = await _sut.RegisterAsync(CreateM2MRequest());
        
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.
            SingleAsync(c => c.Id == result.Client.Id);

        Assert.NotNull(client);
        Assert.Equal(result.Client.Name, client.Name);
        
        var secrets = await ctx.ClientSecrets
            .Where(s => s.ClientId == client.Id)
            .ToArrayAsync();

        Assert.Single(secrets);
    }

    [Fact]
    public async Task RegisterAsync_PersistsPublicClientWithoutSecrets()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
        
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.
            SingleAsync(c => c.Id == result.Client.Id);

        Assert.NotNull(client);
        Assert.Equal(result.Client.Name, client.Name);
        
        var secrets = await ctx.ClientSecrets
            .Where(s => s.ClientId == client.Id)
            .ToArrayAsync();

        Assert.Empty(secrets);      
    }
    
    [Fact]
    public async Task RenameAsync_PersistsUpdatedName()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
         
        var expected = new ClientName("new client");
         
        await _sut.RenameAsync(result.Client.Id, expected);

        await using var ctx = _fx.CreateContext();
        var fetched = await ctx.Clients.SingleAsync(c => c.Id == result.Client.Id);
         
        Assert.Equal(expected, fetched.Name);
    }
    
    [Fact]
    public async Task SetGrantTypesAsync_PersistsUpdate()
    {
        var result = await _sut.RegisterAsync(CreateM2MRequest());
        var clientId = result.Client.Id;
             
        var grantTypes = new[] { GrantType.ClientCredentials, GrantType.RefreshToken };
        await _sut.SetGrantTypesAsync(clientId, grantTypes);
         
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

        Assert.Equal(grantTypes.Length, client.AllowedGrantTypes.Count);
        Assert.All(grantTypes, r =>
            Assert.Contains(r, client.AllowedGrantTypes));
    }

    [Fact]
    public async Task AddAndRemoveGrantTypeAsync_PersistsUpdate()
    {
        var result = await _sut.RegisterAsync(CreateM2MRequest());
        var clientId = result.Client.Id;
             
        // M2M clients already contain ClientCredentials by default
        await _sut.AddGrantTypeAsync(clientId, GrantType.RefreshToken);
        await _sut.RemoveGrantTypeAsync(clientId, GrantType.ClientCredentials);
         
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

        Assert.Null(client.AllowedGrantTypes.SingleOrDefault(r => r == GrantType.ClientCredentials));
        Assert.NotNull(client.AllowedGrantTypes.SingleOrDefault(r => r == GrantType.RefreshToken));
    }
    
    [Fact]
    public async Task SetRedirectUrisAsync_PersistsUpdate()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
        var clientId = result.Client.Id;
             
        var redirectUris = new[] { UriA, UriB };
        await _sut.SetRedirectUrisAsync(clientId, redirectUris);
         
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

        Assert.Equal(redirectUris.Length, client.RedirectUris.Count);
        Assert.All(redirectUris, r =>
            Assert.Contains(r, client.RedirectUris));
    }

    [Fact]
    public async Task AddAndRemoveRedirectUriAsync_PersistsUpdate()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
        var clientId = result.Client.Id;
             
        await _sut.AddRedirectUriAsync(clientId, UriA);
        await _sut.AddRedirectUriAsync(clientId, UriB);
        await _sut.RemoveRedirectUriAsync(clientId, UriA);
         
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

        Assert.Null(client.RedirectUris.SingleOrDefault(r => r == UriA));
        Assert.NotNull(client.RedirectUris.SingleOrDefault(r => r == UriB));
    }
    
    [Fact]
    public async Task SetAudiencesAsync_PersistsUpdate()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
        var audiences = new[] { ApiAudience, WebAudience };

        var clientId = result.Client.Id;
        await _sut.SetAudiencesAsync(clientId, audiences);
         
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

        Assert.Equal(audiences.Length, client.AllowedAudiences.Count);
        Assert.All(audiences, a =>
            Assert.Contains(a, client.AllowedAudiences));
    }
    
    [Fact]
    public async Task AddAndRemoveAudienceAsync_PersistsUpdate()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
        var clientId = result.Client.Id;
        
        await _sut.AddAudienceAsync(clientId, ApiAudience);
        await _sut.AddAudienceAsync(clientId, WebAudience);
        await _sut.RemoveAudienceAsync(clientId, WebAudience.Name);
         
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

        var actual = Assert.Single(client.AllowedAudiences);
        Assert.Equal(ApiAudience, actual);
    }

    [Fact]
    public async Task DeleteAsync_RemovesClient()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
         
        var clientId = result.Client.Id;
        await _sut.DeleteAsync(clientId);

        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.SingleOrDefaultAsync(c => c.Id == clientId);
         
        Assert.Null(client);
    }
     
    [Fact]
    public async Task EnableAndDisableAsync_TogglesAndPersistsFlag()
    {
        var result = await _sut.RegisterAsync(CreateSpaRequest());
         
        var clientId = result.Client.Id;
         
        await _sut.DisableAsync(clientId);
        await using (var ctx = _fx.CreateContext())
        {
            var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);
            Assert.False(client.Enabled);
        }
         
        await _sut.EnableAsync(clientId);
        await using (var ctx = _fx.CreateContext())
        {
            var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);
            Assert.True(client.Enabled);
        }
    }
}