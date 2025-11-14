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
    
    private static RegisterClientRequest CreateM2MRequest()
        => new(
            ApplicationType: "m2m", 
            Name: "test client", 
            Permissions: new Dictionary<string, IEnumerable<string>>
                { { "api", ["read", "write"] } }
        );
    
    private static RegisterClientRequest CreateSpaRequest()
        => new(
            ApplicationType: "spa", 
            Name: "test client", 
            RedirectUris: ["https://example.com/callback"]);

    
    [Fact]
    public async Task RegisterAsync_PersistsConfidentialClientWithSecret()
    {
        var result = await _sut.RegisterAsync(CreateM2MRequest());
        
        await using var ctx = _fx.CreateContext();
        var client = await ctx.Clients.
            SingleAsync(c => c.Id == ClientId.Create(result.Client.Id));

        Assert.NotNull(client);
        Assert.Equal(result.Client.Name, client.Name.ToString());
        
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
            SingleAsync(c => c.Id == ClientId.Create(result.Client.Id));

        Assert.NotNull(client);
        Assert.Equal(result.Client.Name, client.Name.ToString());
        
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
         
         var clientId = ClientId.Create(result.Client.Id);
         await _sut.RenameAsync(clientId, expected);

         await using var ctx = _fx.CreateContext();
         var fetched = await ctx.Clients.SingleAsync(c => c.Id == clientId);
         
         Assert.Equal(expected, fetched.Name);
     }

     [Fact]
     public async Task SetAudiences_PersistsNewAudiences()
     {
         var result = await _sut.RegisterAsync(CreateSpaRequest());
         var audiences = new Audience[]
         {
             new(AudienceName.Create("api"), ScopeCollection.Parse("read write")),
             new(AudienceName.Create("web"), ScopeCollection.Parse("read"))
         };

         var clientId = ClientId.Create(result.Client.Id);
         await _sut.SetAudiencesAsync(clientId, audiences);
         
         await using var ctx = _fx.CreateContext();
         var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

         Assert.Equal(audiences.Length, client.AllowedAudiences.Count);
         Assert.All(audiences, a =>
             Assert.Contains(a, client.AllowedAudiences));
     }

     [Fact]
     public async Task AddAndRemoveAudienceAsync_PersistsChanges()
     {
         var result = await _sut.RegisterAsync(CreateSpaRequest());
         
         var api = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
         var web = new Audience(AudienceName.Create("web"), ScopeCollection.Parse("read"));

         var clientId = ClientId.Create(result.Client.Id);
         await _sut.AddAudienceAsync(clientId, api);
         await _sut.AddAudienceAsync(clientId, web);
         await _sut.RemoveAudienceAsync(clientId, web.Name);
         
         await using var ctx = _fx.CreateContext();
         var client = await ctx.Clients.SingleAsync(c => c.Id == clientId);

         var actual = Assert.Single(client.AllowedAudiences);
         Assert.Equal(api.Name, actual.Name);
     }

     [Fact]
     public async Task DeleteAsync_RemovesClient()
     {
         var result = await _sut.RegisterAsync(CreateSpaRequest());
         
         var clientId = ClientId.Create(result.Client.Id);
         await _sut.DeleteAsync(clientId);

         await using var ctx = _fx.CreateContext();
         var client = await ctx.Clients.SingleOrDefaultAsync(c => c.Id == clientId);
         
         Assert.Null(client);
     }
     
     [Fact]
     public async Task EnableAndDisableAsync_TogglesAndPersistsFlag()
     {
         var result = await _sut.RegisterAsync(CreateSpaRequest());
         
         var clientId = ClientId.Create(result.Client.Id);
         
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