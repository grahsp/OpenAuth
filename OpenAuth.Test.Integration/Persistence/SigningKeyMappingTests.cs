using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Test.Integration.Fixtures;
using Key = OpenAuth.Domain.ValueObjects.Key;

namespace OpenAuth.Test.Integration.Persistence;

[Collection("sqlserver")]
public class SigningKeyMappingTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    public SigningKeyMappingTests(SqlServerFixture fx) => _fx = fx;

    public Task InitializeAsync() => _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    
    [Fact]
    public async Task SigningKey_RoundTrips_AllFields()
    {
        await using var ctx = _fx.CreateContext();

        var key = new Key("private-key");
        var signingKey = new SigningKey(SigningAlgorithm.Hmac, key, DateTime.UtcNow.AddHours(1));
        
        ctx.Add(signingKey);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.SigningKeys.SingleAsync(c => c.Id == signingKey.Id);

        Assert.Equal(signingKey.Id, loaded.Id);
        Assert.Equal(SigningAlgorithm.Hmac, loaded.Algorithm);
        Assert.Equal(key, loaded.Key);
        Assert.Equal(signingKey.CreatedAt, loaded.CreatedAt);
        Assert.Equal(signingKey.ExpiresAt, loaded.ExpiresAt);
    }
}