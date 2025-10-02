using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Repository;

[Collection("sqlserver")]
public class ClientRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    public ClientRepositoryTests(SqlServerFixture fx) => _fx = fx;

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;
    
    private static ClientRepository CreateRepository(AppDbContext context)
        => new(context);

    [Fact]
    public async Task GetByIdAsync_ReturnsClient_WhenExists()
    {
        await using var context = _fx.CreateContext();
        
        var repo = CreateRepository(context);
        var client = new ClientBuilder().Build();
        
        repo.Add(client);
        await context.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(client.Id);
        Assert.NotNull(fetched);
        Assert.Same(client, fetched);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = _fx.CreateContext();
        var repo = CreateRepository(context);

        var fetched = await repo.GetByIdAsync(new ClientId(Guid.NewGuid()));
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsClient_WhenExists()
    {
        await using var context = _fx.CreateContext();
        var repo = CreateRepository(context);

        var clientName = new ClientName("client");
        var client = new ClientBuilder()
            .WithName(clientName)
            .Build();
        
        repo.Add(client);
        await context.SaveChangesAsync();

        var fetched = await repo.GetByNameAsync(clientName);
        Assert.NotNull(fetched);
        Assert.Same(client, fetched);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNull_WhenNotFound()
    {
        await using var context = _fx.CreateContext();
        var repo = CreateRepository(context);

        var fetched = await repo.GetByNameAsync(new ClientName("missing-client"));
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetByIdAsync_IncludesSecrets()
    {
        await using var context = _fx.CreateContext();
        var repo = CreateRepository(context);

        var client = new ClientBuilder().Build();
        var secret = new ClientSecretBuilder().Build();
        client.AddSecret(secret, _time.GetUtcNow());
        
        repo.Add(client);
        await context.SaveChangesAsync();

        var fetched = await repo.GetByIdAsync(client.Id);
        Assert.NotNull(fetched);
        Assert.Single(fetched.Secrets);
    }
}