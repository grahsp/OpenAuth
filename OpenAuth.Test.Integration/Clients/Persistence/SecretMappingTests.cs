using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients.Persistence;

[Collection("sqlserver")]
public class SecretMappingTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    public SecretMappingTests(SqlServerFixture fx) => _fx = fx;

    public Task InitializeAsync() => _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    
    
    [Fact]
    public async Task ClientSecret_RoundTrips_AllFields()
    {
        await using var ctx = _fx.CreateContext();
        
        const string hash = "secret";
        var client = new ClientBuilder()
            .WithSecret(hash)
            .Build();
        
        ctx.Add(client);
        await ctx.SaveChangesAsync();

        var secret = client.Secrets.First();
        var loaded = await ctx.Set<Secret>().SingleAsync(x => x.Id == secret.Id);

        Assert.Equal(secret.Id, loaded.Id);
        Assert.Equal(client.Id, loaded.ClientId);
        Assert.Equal(hash, loaded.Hash.Value);
        Assert.Equal(secret.CreatedAt, loaded.CreatedAt);
        Assert.Equal(secret.ExpiresAt, loaded.ExpiresAt);
        Assert.Equal(secret.RevokedAt, loaded.RevokedAt);
    }

    [Fact]
    public async Task ClientSecret_Requires_Client()
    {
        await using var ctx = _fx.CreateContext();
        var secret = new SecretBuilder().Build();
        
        ctx.Add(secret);
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}