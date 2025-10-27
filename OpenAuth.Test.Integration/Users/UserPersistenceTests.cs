using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Users;
using OpenAuth.Domain.Users.ValueObjects;
using OpenAuth.Test.Integration.Infrastructure;

namespace OpenAuth.Test.Integration.Users;

[Collection("sqlserver")]
public class UserPersistenceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly FakeTimeProvider _time;

    public UserPersistenceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
    }
    
    public Task InitializeAsync()
        => _fx.ResetAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;
    
    
    [Fact]
    public async Task User_RoundTrip_PersistsAsExpected()
    {
        var now = _time.GetUtcNow();
        var user = User.Create("john", new Email("john@example.com"), new HashedPassword("hash"), now);
        
        await using var ctx1 = _fx.CreateContext();
        ctx1.Users.Add(user);
        await ctx1.SaveChangesAsync();

        
        // Act + Assert
        await using var ctx2 = _fx.CreateContext();
        var loaded = await ctx2.Users.FirstAsync();

        Assert.Equal(user.Id, loaded.Id);
        Assert.Equal(user.Username, loaded.Username);
        Assert.Equal(user.Email, loaded.Email);
        Assert.Equal(user.HashedPassword, loaded.HashedPassword);
        Assert.Equal(now, loaded.CreatedAt);
    }
    
    [Fact]
    public async Task User_Username_IsUnique()
    {
        var now = _time.GetUtcNow();
        
        var user1 = User.Create(
            "john",
            new Email("john@example.com"),
            new HashedPassword("hash1"),
            now);

        var user2 = User.Create(
            "john",
            new Email("doe@example.com"),
            new HashedPassword("hash2"),
            now);

        await using var ctx = _fx.CreateContext();
        ctx.Users.Add(user1);
        await ctx.SaveChangesAsync();

        // Act + Assert
        ctx.Users.Add(user2);
        await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
    
    [Fact]
    public async Task User_Email_IsUnique()
    {
        var now = _time.GetUtcNow();
        
        var user1 = User.Create(
            "john",
            new Email("john@example.com"),
            new HashedPassword("hash1"),
            now);

        var user2 = User.Create(
            "doe",
            new Email("john@example.com"),
            new HashedPassword("hash2"),
            now);

        await using var ctx = _fx.CreateContext();
        ctx.Users.Add(user1);
        await ctx.SaveChangesAsync();

        // Act + Assert
        ctx.Users.Add(user2);
        await Assert.ThrowsAsync<DbUpdateException>(() => ctx.SaveChangesAsync());
    }
}