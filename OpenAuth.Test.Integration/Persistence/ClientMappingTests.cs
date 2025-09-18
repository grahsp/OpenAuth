using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Persistence;

[Collection("sqlserver")]
public class ClientMappingTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    public ClientMappingTests(SqlServerFixture fx) => _fx = fx;

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    
    // Helpers
    private static Scope Read => new("read");
    private static Scope Write => new("write");

    
    [Fact]
    public async Task Client_RoundTrips_AllFields()
    {
        const string name = "client";
        var client = new Client(name);
        var api = new Audience("api");

        client.TryAddAudience(api);
        client.GrantScopes(api, Read, Write);
        
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
            Assert.Equal(name, loaded.Name);
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

        const string name = "client";

        ctx.Add(new Client(name));
        await ctx.SaveChangesAsync();
        
        ctx.Add(new Client(name));
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }

    [Fact]
    public async Task Deleting_Client_Cascades_To_ClientSecrets()
    {
        await using var ctx = _fx.CreateContext();
        
        var client = new Client("client");
        var secret = new ClientSecret(new SecretHash("secret"));

        client.AddSecret(secret);
        
        ctx.Add(client);
        await ctx.SaveChangesAsync();
        Assert.True(await ctx.ClientSecrets.AnyAsync());

        ctx.Remove(client);
        await ctx.SaveChangesAsync();
        Assert.False(await ctx.ClientSecrets.AnyAsync());
    }
    
    [Fact]
    public async Task Deleting_Client_Cascades_To_SigningKeys()
    {
        await using var ctx = _fx.CreateContext();

        var client = new Client("client");
        client.SigningKeys.Add(SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key"));

        ctx.Add(client);
        await ctx.SaveChangesAsync();
        Assert.True(await ctx.SigningKeys.AnyAsync());

        ctx.Remove(client);
        await ctx.SaveChangesAsync();
        Assert.False(await ctx.SigningKeys.AnyAsync());
    }
}