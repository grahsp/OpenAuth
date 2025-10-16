using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients.Infrastructure;

[Collection("sqlserver")]
public class ClientRepositoryTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly FakeTimeProvider _time;
    
    public ClientRepositoryTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private static ClientRepository CreateRepository(AppDbContext context)
        => new(context);

    
    public class GetByIdAsync(SqlServerFixture fx) : ClientRepositoryTests(fx)
    {
        [Fact]
        public async Task ReturnsClient_WhenExists()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            
            var client = new ClientBuilder().Build();
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Act
            var fetched = await repo.GetByIdAsync(client.Id);

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal(client.Id, fetched.Id);
            Assert.Equal(client.Name, fetched.Name);
        }

        [Fact]
        public async Task ReturnsNull_WhenNotFound()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);

            // Act
            var fetched = await repo.GetByIdAsync(ClientId.New());

            // Assert
            Assert.Null(fetched);
        }

        [Fact]
        public async Task IncludesSecrets()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);

            var client = new ClientBuilder()
                .WithSecret("hash1")
                .WithSecret("hash2")
                .Build();
            
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Act
            var fetched = await repo.GetByIdAsync(client.Id);

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal(2, fetched.Secrets.Count);
        }

        [Fact]
        public async Task IncludesAudiences()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);

            var client = new ClientBuilder().Build();
            var audience1 = client.AddAudience(new AudienceName("api"), _time.GetUtcNow());
            var audience2 = client.AddAudience(new AudienceName("web"), _time.GetUtcNow());
            
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Act
            var fetched = await repo.GetByIdAsync(client.Id);

            // Assert
            Assert.NotNull(fetched);
            Assert.Equal(2, fetched.AllowedAudiences.Count);
            Assert.Contains(fetched.AllowedAudiences, a => a.Name == audience1.Name);
            Assert.Contains(fetched.AllowedAudiences, a => a.Name == audience2.Name);
        }

        [Fact]
        public async Task IncludesAudiencesWithScopes()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);

            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            var scopes = new[] { new Scope("read"), new Scope("write") };
            client.GrantScopes(audienceName, scopes, _time.GetUtcNow());
            
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Act
            var fetched = await repo.GetByIdAsync(client.Id);

            // Assert
            Assert.NotNull(fetched);
            var fetchedAudience = fetched.AllowedAudiences.Single();
            Assert.Equal(2, fetchedAudience.AllowedScopes.Count);
        }
    }

    public class Add(SqlServerFixture fx) : ClientRepositoryTests(fx)
    {
        [Fact]
        public async Task AddsClientToContext()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            var client = new ClientBuilder().Build();

            // Act
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Assert
            var persisted = await context.Clients.FindAsync(client.Id);
            Assert.NotNull(persisted);
        }

        [Fact]
        public async Task PersistsClientProperties()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            
            var name = new ClientName("test-client");
            var client = new ClientBuilder()
                .WithName(name)
                .Build();

            // Act
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Assert
            var persisted = await context.Clients.FindAsync(client.Id);
            Assert.NotNull(persisted);
            Assert.Equal(name, persisted.Name);
        }
    }

    public class Remove(SqlServerFixture fx) : ClientRepositoryTests(fx)
    {
        [Fact]
        public async Task RemovesClientFromDatabase()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            
            var client = new ClientBuilder().Build();
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Act
            repo.Remove(client);
            await repo.SaveChangesAsync();

            // Assert
            var persisted = await context.Clients.FindAsync(client.Id);
            Assert.Null(persisted);
        }

        [Fact]
        public async Task CascadesDeleteToSecrets()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            
            var client = new ClientBuilder()
                .WithSecret("hash1")
                .WithSecret("hash2")
                .Build();
            
            repo.Add(client);
            await repo.SaveChangesAsync();

            var secretIds = client.Secrets.Select(s => s.Id).ToArray();

            // Act
            repo.Remove(client);
            await repo.SaveChangesAsync();

            // Assert
            foreach (var secretId in secretIds)
            {
                var secret = await context.ClientSecrets.FindAsync(secretId);
                Assert.Null(secret);
            }
        }
    }

    public class SaveChangesAsync(SqlServerFixture fx) : ClientRepositoryTests(fx)
    {
        [Fact]
        public async Task PersistsChangesToDatabase()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            
            var client = new ClientBuilder().Build();
            repo.Add(client);

            // Act
            await repo.SaveChangesAsync();

            // Assert
            await using var verifyContext = _fx.CreateContext();
            var persisted = await verifyContext.Clients.FindAsync(client.Id);
            Assert.NotNull(persisted);
        }

        [Fact]
        public async Task PersistsModifications()
        {
            // Arrange
            await using var context = _fx.CreateContext();
            var repo = CreateRepository(context);
            
            var client = new ClientBuilder().Build();
            repo.Add(client);
            await repo.SaveChangesAsync();

            // Modify client
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            await repo.SaveChangesAsync();

            // Assert
            await using var verifyContext = _fx.CreateContext();
            var persisted = await verifyContext.Clients
                .Include(c => c.AllowedAudiences)
                .FirstOrDefaultAsync(c => c.Id == client.Id);
            
            Assert.NotNull(persisted);
            Assert.Single(persisted.AllowedAudiences);
        }
    }
}