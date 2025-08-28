using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Infrastructure.Tests.Fixtures;

namespace OpenAuth.Infrastructure.Tests.Persistence;

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

        var client = new Client("client");
        const string publicKey = "public-key";
        const string privateKey = "private-key";
        var signingKey = SigningKey.CreateAsymmetric(
            SigningAlgorithm.Hmac,
            publicKey, 
            privateKey,
            DateTime.UtcNow.AddHours(1));
        
        client.AddSigningKey(signingKey);
        ctx.Add(client);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.SigningKeys.SingleAsync(c => c.KeyId == signingKey.KeyId);

        Assert.Equal(signingKey.KeyId, loaded.KeyId);
        Assert.Equal(client.Id, loaded.ClientId);
        Assert.Equal(SigningAlgorithm.Hmac, loaded.Algorithm);
        Assert.Equal(publicKey, loaded.PublicKey);
        Assert.Equal(privateKey, loaded.PrivateKey);
        Assert.Equal(signingKey.CreatedAt, loaded.CreatedAt);
        Assert.Equal(signingKey.ExpiresAt, loaded.ExpiresAt);
    }

    [Fact]
    public async Task ClientSecret_Requires_Client()
    {
        await using var ctx = _fx.CreateContext();

        var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
        
        ctx.Add(signingKey);
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}