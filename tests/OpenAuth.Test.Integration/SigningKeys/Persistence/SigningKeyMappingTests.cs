using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;
using Key = OpenAuth.Domain.SigningKeys.ValueObjects.Key;

namespace OpenAuth.Test.Integration.SigningKeys.Persistence;

[Collection("sqlserver")]
public class SigningKeyMappingTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly TimeProvider _time = new FakeTimeProvider();
    public SigningKeyMappingTests(SqlServerFixture fx) => _fx = fx;

    public Task InitializeAsync() => _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    
    [Fact]
    public async Task SigningKey_RoundTrips_AllFields()
    {
        await using var ctx = _fx.CreateContext();

        var key = new Key("private-key");
        var signingKey = TestSigningKey.CreateRsaSigningKey(_time, key);
        
        ctx.Add(signingKey);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.SigningKeys.SingleAsync(c => c.Id == signingKey.Id);

        Assert.Equal(signingKey.Id, loaded.Id);
        Assert.Equal(signingKey.KeyMaterial.Alg, loaded.KeyMaterial.Alg);
        Assert.Equal(key, loaded.KeyMaterial.Key);
        Assert.Equal(signingKey.CreatedAt, loaded.CreatedAt);
        Assert.Equal(signingKey.ExpiresAt, loaded.ExpiresAt);
    }
}