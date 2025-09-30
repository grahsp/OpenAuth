using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Persistence;

[Collection("sqlserver")]
public class ClientMappingTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    public ClientMappingTests(SqlServerFixture fx) => _fx = fx;

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    
    // Helpers
    private static Scope Read => new("read");
    private static Scope Write => new("write");

    
    [Fact]
    public async Task Client_RoundTrips_AllFields()
    {
        var clientName = new ClientName("client");
        var client = new Client(clientName, _time.GetUtcNow());
        var api = new Audience("api");

        client.TryAddAudience(api, _time.GetUtcNow());
        client.GrantScopes(api, [Read, Write], _time.GetUtcNow());
        
        await using (var ctx1 = _fx.CreateContext())
        {
            ctx1.Add(client);
            await ctx1.SaveChangesAsync();
        }

        await using (var ctx2 = _fx.CreateContext())
        {
            var loaded = await ctx2.Clients
                .Include(x => x.Audiences)
                .ThenInclude(x => x.Scopes)
                .SingleAsync(x => x.Id == client.Id);

            Assert.Equal(client.Id, loaded.Id);
            Assert.Equal(clientName, loaded.Name);
            Assert.Equal(client.TokenLifetime, loaded.TokenLifetime);
            Assert.Equal(client.Enabled, loaded.Enabled);
            Assert.Equal(client.CreatedAt, loaded.CreatedAt);
            Assert.Equal(client.UpdatedAt, loaded.UpdatedAt);
            Assert.Equal(client.Audiences.Count, loaded.Audiences.Count);
            Assert.Equal([Read, Write], loaded.GetAllowedScopes(api).ToArray());
        }
    }

    [Fact]
    public async Task Client_Name_Must_Be_Unique()
    {
        await using var ctx = _fx.CreateContext();

        var clientName = new ClientName("client");

        ctx.Add(new Client(clientName, _time.GetUtcNow()));
        await ctx.SaveChangesAsync();
        
        ctx.Add(new Client(clientName, _time.GetUtcNow()));
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }

    [Fact]
    public async Task Deleting_Client_Cascades_To_ClientSecrets()
    {
        await using var ctx = _fx.CreateContext();
        
        var client = new Client(new ClientName("client"), _time.GetUtcNow());
        var secret = new ClientSecret(new SecretHash("secret"));

        client.AddSecret(secret, _time.GetUtcNow());
        
        ctx.Add(client);
        await ctx.SaveChangesAsync();
        Assert.True(await ctx.ClientSecrets.AnyAsync());

        ctx.Remove(client);
        await ctx.SaveChangesAsync();
        Assert.False(await ctx.ClientSecrets.AnyAsync());
    }
}