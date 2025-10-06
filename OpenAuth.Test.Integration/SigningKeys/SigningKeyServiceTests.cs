using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.SigningKeys;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterial;
using OpenAuth.Infrastructure.SigningKeys.Persistence;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.SigningKeys;

[Collection("sqlserver")]
public class SigningKeyServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly FakeTimeProvider _time;
    
    public SigningKeyServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private SigningKeyService CreateSut()
    {
        var context = _fx.CreateContext();
        return new SigningKeyService(
            new SigningKeyRepository(context), 
            new SigningKeyFactory([
                new RsaKeyMaterialGenerator(),
                new HmacKeyMaterialGenerator()
            ]), 
            _time);
    }

    public class CreateAsync : SigningKeyServiceTests
    {
        public CreateAsync(SqlServerFixture fx) : base(fx) { }

        [Fact]
        public async Task CreatesAndPersistsKey()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Assert
            await using var context = _fx.CreateContext();
            var persisted = await context.SigningKeys
                .FirstOrDefaultAsync(k => k.Id == key.Id);
            
            Assert.NotNull(persisted);
            Assert.Equal(key.Id, persisted.Id);
        }

        [Fact]
        public async Task CreatesKeyWithCorrectAlgorithm()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Assert
            Assert.Equal(SigningAlgorithm.RS256, key.Algorithm);
            Assert.Equal(KeyType.RSA, key.KeyType);
        }

        [Fact]
        public async Task CreatesKeyWithDefaultLifetime_WhenNotSpecified()
        {
            // Arrange
            var sut = CreateSut();
            var now = _time.GetUtcNow();

            // Act
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Assert
            Assert.Equal(now, key.CreatedAt);
            Assert.True(key.ExpiresAt > now);
        }

        [Fact]
        public async Task CreatesKeyWithCustomLifetime_WhenSpecified()
        {
            // Arrange
            var sut = CreateSut();
            var now = _time.GetUtcNow();
            var customLifetime = TimeSpan.FromDays(30);

            // Act
            var key = await sut.CreateAsync(SigningAlgorithm.RS256, customLifetime);

            // Assert
            var expectedExpiry = now.Add(customLifetime);
            Assert.Equal(expectedExpiry, key.ExpiresAt);
        }

        [Fact]
        public async Task CreatesUniqueKeyIds()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var key1 = await sut.CreateAsync(SigningAlgorithm.RS256);
            var key2 = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Assert
            Assert.NotEqual(key1.Id, key2.Id);
        }

        [Fact]
        public async Task SupportsMultipleAlgorithms()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var rsaKey = await sut.CreateAsync(SigningAlgorithm.RS256);
            var hmacKey = await sut.CreateAsync(SigningAlgorithm.HS256);

            // Assert
            Assert.Equal(KeyType.RSA, rsaKey.KeyType);
            Assert.Equal(KeyType.HMAC, hmacKey.KeyType);
        }
    }

    public class RevokeAsync : SigningKeyServiceTests
    {
        public RevokeAsync(SqlServerFixture fx) : base(fx) { }

        [Fact]
        public async Task RevokesAndPersistsKey()
        {
            // Arrange
            var sut = CreateSut();
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Act
            await sut.RevokeAsync(key.Id);

            // Assert
            await using var context = _fx.CreateContext();
            var persisted = await context.SigningKeys
                .FirstOrDefaultAsync(k => k.Id == key.Id);
            
            Assert.NotNull(persisted);
            Assert.NotNull(persisted.RevokedAt);
        }

        [Fact]
        public async Task SetsRevokedAtToCurrentTime()
        {
            // Arrange
            var sut = CreateSut();
            var now = _time.GetUtcNow();
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Act
            await sut.RevokeAsync(key.Id);

            // Assert
            await using var context = _fx.CreateContext();
            var persisted = await context.SigningKeys
                .FirstOrDefaultAsync(k => k.Id == key.Id);
            
            Assert.NotNull(persisted?.RevokedAt);
            Assert.Equal(now, persisted.RevokedAt);
        }

        [Fact]
        public async Task MakesKeyInactive()
        {
            // Arrange
            var sut = CreateSut();
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);

            // Act
            await sut.RevokeAsync(key.Id);

            // Assert
            await using var context = _fx.CreateContext();
            var persisted = await context.SigningKeys
                .FirstOrDefaultAsync(k => k.Id == key.Id);
            
            Assert.NotNull(persisted);
            Assert.False(persisted.IsActive(_time.GetUtcNow()));
        }

        [Fact]
        public async Task Throws_WhenKeyNotFound()
        {
            // Arrange
            var sut = CreateSut();
            var nonExistentId = SigningKeyId.New();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.RevokeAsync(nonExistentId));
            
            Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CanRevokeAlreadyRevokedKey()
        {
            // Arrange
            var sut = CreateSut();
            var key = await sut.CreateAsync(SigningAlgorithm.RS256);
            await sut.RevokeAsync(key.Id);

            // Act & Assert - Should not throw
            await sut.RevokeAsync(key.Id);
        }
    }
}