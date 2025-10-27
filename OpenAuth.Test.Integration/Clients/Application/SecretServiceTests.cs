using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Application.Security.Hashing;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Infrastructure;

namespace OpenAuth.Test.Integration.Clients.Application;

[Collection("sqlserver")]
public class SecretServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly FakeTimeProvider _time;
    private readonly ISecretGenerator _generator;
    private readonly IHasher _hasher;
    private readonly IClientRepository _repository;
    private readonly SecretService _sut;
    
    public SecretServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
        _generator = new SecretGenerator();
        _hasher = new Pbkdf2Hasher();
        _repository = new ClientRepository(_fx.CreateContext());
        _sut = new SecretService(_repository, _generator, _hasher, _time);
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    public class AddSecretAsync : SecretServiceTests
    {
        public AddSecretAsync(SqlServerFixture fx) : base(fx) { }

        [Fact]
        public async Task AddsSecret_ToExistingClient()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.AddSecretAsync(client.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PlainTextSecret);
            Assert.NotEqual(Guid.Empty, result.SecretId.Value);
        }

        [Fact]
        public async Task AddedSecret_IsPersisted()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.AddSecretAsync(client.Id);

            // Assert - Verify secret is in database
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);

                Assert.Single(loaded.Secrets);
                Assert.Equal(result.SecretId, loaded.Secrets.First().Id);
            }
        }

        [Fact]
        public async Task AddedSecret_HasCorrectTimestamps()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.AddSecretAsync(client.Id);

            // Assert
            Assert.Equal(utcNow, result.CreatedAt);
            Assert.Equal(utcNow.AddDays(7), result.ExpiresAt);
        }

        [Fact]
        public async Task PlainSecret_IsHashed_BeforeStorage()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.AddSecretAsync(client.Id);

            // Assert - Verify stored hash is different from plain secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);

                var storedHash = loaded.Secrets.First().Hash;
                Assert.NotEqual(result.PlainTextSecret, storedHash.Value);
                
                // Verify the hash can validate the plain secret
                Assert.True(_hasher.Verify(result.PlainTextSecret, storedHash.Value));
            }
        }

        [Fact]
        public async Task MultipleSecrets_CanBeAdded_ToSameClient()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result1 = await _sut.AddSecretAsync(client.Id);
            var result2 = await _sut.AddSecretAsync(client.Id);
            var result3 = await _sut.AddSecretAsync(client.Id);

            // Assert
            Assert.NotEqual(result1.SecretId, result2.SecretId);
            Assert.NotEqual(result1.SecretId, result3.SecretId);
            Assert.NotEqual(result2.SecretId, result3.SecretId);

            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);

                Assert.Equal(3, loaded.Secrets.Count);
            }
        }

        [Fact]
        public async Task GeneratedSecrets_AreUnique()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result1 = await _sut.AddSecretAsync(client.Id);
            var result2 = await _sut.AddSecretAsync(client.Id);

            // Assert
            Assert.NotEqual(result1.PlainTextSecret, result2.PlainTextSecret);
        }

        [Fact]
        public async Task NonExistentClient_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AddSecretAsync(nonExistentClientId)
            );
            
            Assert.Equal("Client not found.", ex.Message);
        }

        [Fact]
        public async Task Result_ContainsAllRequiredFields()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.AddSecretAsync(client.Id);

            // Assert
            Assert.NotNull(result.PlainTextSecret);
            Assert.NotEmpty(result.PlainTextSecret);
            Assert.NotEqual(default, result.SecretId);
            Assert.Equal(utcNow, result.CreatedAt);
            Assert.Equal(utcNow.AddDays(7), result.ExpiresAt);
        }
    }


    public class RevokeSecretAsync(SqlServerFixture fx) : SecretServiceTests(fx)
    {
        [Fact]
        public async Task RevokesExistingSecret()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret")
                .WithSecret("secret")
                .Build();

            var secretId = client.Secrets.First().Id;

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await _sut.RevokeSecretAsync(client.Id, secretId);

            // Assert
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);

                var revokedSecret = loaded.Secrets.Single(s => s.Id == secretId);
                Assert.NotNull(revokedSecret.RevokedAt);
            }
        }

        [Fact]
        public async Task RevokedSecret_HasCorrectTimestamp()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret")
                .WithSecret("secret")
                .Build();

            var secretId = client.Secrets.First().Id;

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await _sut.RevokeSecretAsync(client.Id, secretId);

            // Assert
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);

                var revokedSecret = loaded.Secrets.Single(s => s.Id == secretId);;
                Assert.Equal(utcNow, revokedSecret.RevokedAt);
            }
        }

        [Fact]
        public async Task RevokingOneSecret_DoesNotAffectOthers()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret1")
                .WithSecret("secret2")
                .WithSecret("secret3")
                .Build();

            var secretToRevoke = client.Secrets.First().Id;
            var otherSecretIds = client.Secrets
                .Where(s => s.Id != secretToRevoke)
                .Select(s => s.Id)
                .ToList();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await _sut.RevokeSecretAsync(client.Id, secretToRevoke);

            // Assert
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);

                var revokedSecret = loaded.Secrets.Single(s => s.Id == secretToRevoke);
                Assert.NotNull(revokedSecret.RevokedAt);

                var activeSecrets = loaded.Secrets.Where(s => otherSecretIds.Contains(s.Id));
                Assert.All(activeSecrets, s => Assert.Null(s.RevokedAt));
            }
        }

        [Fact]
        public async Task NonExistentClient_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());
            var secretId = new SecretId(Guid.NewGuid());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.RevokeSecretAsync(nonExistentClientId, secretId)
            );
            
            Assert.Equal("Client not found.", ex.Message);
        }

        [Fact]
        public async Task NonExistentSecret_ThrowsException()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret1")
                .Build();

            var nonExistentSecretId = new SecretId(Guid.NewGuid());

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                () => _sut.RevokeSecretAsync(client.Id, nonExistentSecretId)
            );
        }
    }
}