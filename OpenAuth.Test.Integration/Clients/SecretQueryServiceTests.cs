using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Security.Hashing;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients.Secrets;
using OpenAuth.Infrastructure.Clients.Secrets.Persistence;
using OpenAuth.Infrastructure.Security.Hashing;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients;

[Collection("sqlserver")]
public class SecretQueryServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly FakeTimeProvider _time;
    private readonly ISecretHasher _hasher;
    private readonly SecretQueryService _sut;
    
    public SecretQueryServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
        _hasher = new Pbkdf2Hasher();
        _sut = new SecretQueryService(_fx.CreateContext(), _hasher, _time);
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    public class ValidateSecretAsync(SqlServerFixture fx) : SecretQueryServiceTests(fx)
    {
        [Fact]
        public async Task ValidSecret_ReturnsTrue()
        {
            // Arrange
            const string plain = "my-secret-key-12345";
            
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret(_hasher.Hash(plain))
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.ValidateSecretAsync(client.Id, plain);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InvalidSecret_ReturnsFalse()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("correct-secret")
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.ValidateSecretAsync(client.Id, "wrong-secret");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ClientWithNoSecrets_ReturnsFalse()
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
            var result = await _sut.ValidateSecretAsync(client.Id, "any-secret");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task NonExistentClient_ReturnsFalse()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());

            // Act
            var result = await _sut.ValidateSecretAsync(nonExistentClientId, "any-secret");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExpiredSecret_ReturnsFalse()
        {
            // Arrange
            const string plain = "my-secret-key";
            var utcNow = _time.GetUtcNow();
            
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret(_hasher.Hash(plain))
                .CreatedAt(utcNow.AddDays(-10))
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Advance time past expiration
            _time.Advance(TimeSpan.FromDays(8));
            var result = await _sut.ValidateSecretAsync(client.Id, plain);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RevokedSecret_ReturnsFalse()
        {
            // Arrange
            const string plain = "my-secret-key";
            
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret(_hasher.Hash(plain))
                .WithSecret("another-secret")
                .Build();

            var secretId = client.Secrets.First().Id;

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Revoke the secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);
                loaded.RevokeSecret(secretId, _time.GetUtcNow());
                await ctx.SaveChangesAsync();
            }

            // Act
            var result = await _sut.ValidateSecretAsync(client.Id, plain);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MultipleSecrets_ValidatesCorrectOne()
        {
            // Arrange
            const string plain1 = "first-secret";
            const string plain2 = "second-secret";
            const string plain3 = "third-secret";
            
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret(_hasher.Hash(plain1))
                .WithSecret(_hasher.Hash(plain2))
                .WithSecret(_hasher.Hash(plain3))
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act & Assert
            Assert.True(await _sut.ValidateSecretAsync(client.Id, plain1));
            Assert.True(await _sut.ValidateSecretAsync(client.Id, plain2));
            Assert.True(await _sut.ValidateSecretAsync(client.Id, plain3));
            Assert.False(await _sut.ValidateSecretAsync(client.Id, "wrong-secret"));
        }

        [Fact]
        public async Task MixOfActiveAndInactiveSecrets_OnlyValidatesActive()
        {
            // Arrange
            const string activePlain = "active-secret";
            const string revokedPlain = "revoked-secret";
            
            var utcNow = _time.GetUtcNow();
            
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret(_hasher.Hash(activePlain))
                .WithSecret(_hasher.Hash(revokedPlain))
                .CreatedAt(utcNow)
                .Build();

            var revokedSecretId = client.Secrets.Last().Id;

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Revoke one secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);
                loaded.RevokeSecret(revokedSecretId, utcNow.AddDays(1));
                await ctx.SaveChangesAsync();
            }

            // Act & Assert
            Assert.True(await _sut.ValidateSecretAsync(client.Id, activePlain));
            Assert.False(await _sut.ValidateSecretAsync(client.Id, revokedPlain));
        }
    }


    public class GetActiveSecretsAsync(SqlServerFixture fx) : SecretQueryServiceTests(fx)
    {
        [Fact]
        public async Task ClientWithActiveSecrets_ReturnsAllActiveSecrets()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret1")
                .WithSecret("secret2")
                .WithSecret("secret3")
                .CreatedAt(utcNow)
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var secrets = (await _sut.GetActiveSecretsAsync(client.Id)).ToArray();

            // Assert
            Assert.Equal(3, secrets.Length);
            Assert.All(secrets, s =>
            {
                Assert.Equal(utcNow, s.CreatedAt);
                Assert.Equal(utcNow.AddDays(7), s.ExpiresAt);
                Assert.Null(s.RevokedAt);
            });
        }

        [Fact]
        public async Task ClientWithNoSecrets_ReturnsEmptyList()
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
            var secrets = await _sut.GetActiveSecretsAsync(client.Id);

            // Assert
            Assert.Empty(secrets);
        }

        [Fact]
        public async Task NonExistentClient_ReturnsEmptyList()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());

            // Act
            var secrets = await _sut.GetActiveSecretsAsync(nonExistentClientId);

            // Assert
            Assert.Empty(secrets);
        }

        [Fact]
        public async Task ExpiredSecrets_AreNotIncluded()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret1")
                .CreatedAt(utcNow.AddDays(-10))
                .Build();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Advance time past expiration (7 days from creation)
            _time.Advance(TimeSpan.FromDays(8));
            var secrets = await _sut.GetActiveSecretsAsync(client.Id);

            // Assert
            Assert.Empty(secrets);
        }

        [Fact]
        public async Task RevokedSecrets_AreNotIncluded()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret1")
                .WithSecret("secret2")
                .CreatedAt(utcNow)
                .Build();

            var secretToRevoke = client.Secrets.First().Id;

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Revoke one secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);
                loaded.RevokeSecret(secretToRevoke, utcNow.AddDays(1));
                await ctx.SaveChangesAsync();
            }

            // Act
            var secrets = (await _sut.GetActiveSecretsAsync(client.Id)).ToArray();

            // Assert
            Assert.Single(secrets);
            Assert.NotEqual(secretToRevoke, secrets[0].Id);
        }

        [Fact]
        public async Task MixOfActiveAndInactiveSecrets_ReturnsOnlyActive()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("active-secret")
                .WithSecret("revoked-secret")
                .CreatedAt(utcNow.AddDays(-10))
                .Build();

            var revokedSecretId = client.Secrets.First().Id;

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Revoke one secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);
                loaded.RevokeSecret(revokedSecretId, utcNow);
                await ctx.SaveChangesAsync();
            }

            // Add a new active secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(c => c.Secrets)
                    .SingleAsync(c => c.Id == client.Id);
                loaded.AddSecret(new SecretHash("new-active-secret"), utcNow);
                await ctx.SaveChangesAsync();
            }

            // Act - Time hasn't advanced, so only the new secret should be active
            var secrets = (await _sut.GetActiveSecretsAsync(client.Id)).ToArray();

            // Assert
            Assert.Single(secrets);
            Assert.Equal(utcNow, secrets[0].CreatedAt);
        }

        [Fact]
        public async Task ReturnedDto_ContainsAllFields()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("secret1")
                .CreatedAt(utcNow)
                .Build();

            var originalSecret = client.Secrets.Single();

            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            var secrets = await _sut.GetActiveSecretsAsync(client.Id);

            // Assert
            var dto = Assert.Single(secrets);
            Assert.Equal(originalSecret.Id, dto.Id);
            Assert.Equal(utcNow, dto.CreatedAt);
            Assert.Equal(utcNow.AddDays(7), dto.ExpiresAt);
            Assert.Null(dto.RevokedAt);
        }
    }
}