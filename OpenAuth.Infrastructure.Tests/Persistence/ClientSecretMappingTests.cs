using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Tests.Fixtures;

namespace OpenAuth.Infrastructure.Tests.Persistence;

[Collection("sqlserver")]
public class ClientSecretMappingTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    public ClientSecretMappingTests(SqlServerFixture fx) => _fx = fx;

    public Task InitializeAsync() => _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    
    
    [Fact]
    public async Task ClientSecret_RoundTrips_AllFields()
    {
        await using var ctx = _fx.CreateContext();
        
        var client = new Client("client");
        var hash = new SecretHash("secret");
        var secret = new ClientSecret(hash, DateTime.UtcNow.AddDays(7));

        client.AddSecret(secret);
        ctx.Add(client);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Set<ClientSecret>().SingleAsync(x => x.Id == secret.Id);

        Assert.Equal(secret.Id, loaded.Id);
        Assert.Equal(client.Id, loaded.ClientId);
        Assert.Equal(hash, loaded.Hash);
        Assert.Equal(secret.CreatedAt, loaded.CreatedAt);
        Assert.Equal(secret.ExpiresAt, loaded.ExpiresAt);
        Assert.Equal(secret.RevokedAt, loaded.RevokedAt);
    }

    [Fact]
    public async Task ClientSecret_Requires_Client()
    {
        await using var ctx = _fx.CreateContext();

        var hash = new SecretHash("secret");
        var secret = new ClientSecret(hash);
        
        ctx.Add(secret);
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}