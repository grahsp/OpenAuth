using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.SigningKeys.Persistence;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.SigningKeys.Infrastructure;

[Collection("sqlserver")]
public class SigningKeyQueryServiceTests : IAsyncLifetime
{
    private SqlServerFixture _fx;
    private FakeTimeProvider _time;
    private SigningKeyQueryService _sut;

    public SigningKeyQueryServiceTests(SqlServerFixture fx)
    {
        _fx = fx;

        var context = _fx.CreateContext();
        _time = new FakeTimeProvider();
        _sut = new SigningKeyQueryService(context, _time);
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    
    public Task DisposeAsync() => Task.CompletedTask;

    
    public class GetCurrentAsync(SqlServerFixture fx) : SigningKeyQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsActiveKey_WhenExists()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var activeKey = new SigningKeyBuilder()
                .WithKey("active-key-material")
                .CreatedAt(now.AddMonths(-1))
                .ExpiresAt(now.AddHours(1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddAsync(activeKey);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetCurrentKeyDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(activeKey.Id, result.Kid);
            Assert.Equal(KeyType.RSA, result.Kty);
            Assert.Equal(SigningAlgorithm.RS256, result.Alg);
            Assert.Equal(activeKey.KeyMaterial.Key, result.Key);
        }

        [Fact]
        public async Task ReturnsNull_WhenNoActiveKeyExists()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var expiredKey = new SigningKeyBuilder()
                .WithKey("expired-key")
                .CreatedAt(now)
                .ExpiresAt(now)
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddAsync(expiredKey);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetCurrentKeyDataAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ReturnsNull_WhenOnlyRevokedKeyExists()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var key = new SigningKeyBuilder()
                .WithKey("revoked-key")
                .CreatedAt(now.AddMonths(-1))
                .ExpiresAt(now.AddMonths(1))
                .Build();
            
            key.Revoke(now.UtcDateTime);

            var context = _fx.CreateContext();
            await context.SigningKeys.AddAsync(key);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetCurrentKeyDataAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ReturnsMostRecentKey_WhenMultipleActive()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var olderKey = new SigningKeyBuilder()
                .WithKey("older-key")
                .CreatedAt(now.AddMonths(-6))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var newerKey = new SigningKeyBuilder()
                .WithKey("newer-key")
                .CreatedAt(now.AddMonths(-1))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddRangeAsync(olderKey, newerKey);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetCurrentKeyDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newerKey.Id, result.Kid);
            Assert.Equal("newer-key", result.Key.Value);
        }
    }

    public class GetByIdAsync(SqlServerFixture fx) : SigningKeyQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsKey_WhenExists()
        {
            // Arrange
            var now = _time.GetUtcNow();
            
            var key = new SigningKeyBuilder()
                .WithKey("ec-key-material")
                .WithKty(KeyType.RSA)
                .WithAlg(SigningAlgorithm.RS256)
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddAsync(key);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(key.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key.Id, result.Id);
            Assert.Equal(KeyType.RSA, result.KeyType);
            Assert.Equal(SigningAlgorithm.RS256, result.Algorithm);
            Assert.Equal(key.CreatedAt, result.CreatedAt);
            Assert.Equal(key.ExpiresAt, result.ExpiresAt);
            Assert.Null(result.RevokedAt);
        }

        [Fact]
        public async Task ReturnsNull_WhenDoesNotExist()
        {
            // Arrange
            var nonExistentId = new SigningKeyId(Guid.NewGuid());

            // Act
            var result = await _sut.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IncludesRevokedAt_WhenRevoked()
        {
            // Arrange
            var now = _time.GetUtcNow();
            
            var key = new SigningKeyBuilder()
                .WithKey("key-material")
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();
            
            key.Revoke(now.UtcDateTime);

            var context = _fx.CreateContext();
            await context.SigningKeys.AddAsync(key);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetByIdAsync(key.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.RevokedAt);
            Assert.Equal(now.UtcDateTime, result.RevokedAt.Value);
        }
    }

    public class GetAllAsync(SqlServerFixture fx) : SigningKeyQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsAllKeys_OrderedByCreatedAtDescending()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var key1 = new SigningKeyBuilder()
                .WithKey("key1")
                .CreatedAt(now.AddMonths(-3))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var key2 = new SigningKeyBuilder()
                .WithKey("key2")
                .CreatedAt(now.AddMonths(-2))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var key3 = new SigningKeyBuilder()
                .WithKey("key3")
                .CreatedAt(now.AddMonths(-1))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddRangeAsync(key1, key2, key3);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(key3.Id, result.ElementAt(0).Id);  // Most recent
            Assert.Equal(key2.Id, result.ElementAt(1).Id);
            Assert.Equal(key1.Id, result.ElementAt(2).Id);  // Oldest
        }

        [Fact]
        public async Task ReturnsEmpty_WhenNoKeysExist()
        {
            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task IncludesRevokedAndExpiredKeys()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var activeKey = new SigningKeyBuilder()
                .WithKey("active")
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var revokedKey = new SigningKeyBuilder()
                .WithKey("revoked")
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();
            revokedKey.Revoke(now);

            var expiredKey = new SigningKeyBuilder()
                .WithKey("expired")
                .CreatedAt(now.AddMonths(-12))
                .ExpiresAt(now.AddMonths(-5))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddRangeAsync(activeKey, revokedKey, expiredKey);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, k => k.Id == activeKey.Id);
            Assert.Contains(result, k => k.Id == revokedKey.Id);
            Assert.Contains(result, k => k.Id == expiredKey.Id);
        }
    }

    public class GetActiveAsync(SqlServerFixture fx) : SigningKeyQueryServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsOnlyActiveKeys()
        {
            // Arrange
            var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            _time.SetUtcNow(now);

            var activeKey1 = new SigningKeyBuilder()
                .WithKey("active1")
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var activeKey2 = new SigningKeyBuilder()
                .WithKey("active2")
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var revokedKey = new SigningKeyBuilder()
                .WithKey("revoked")
                .CreatedAt(now)
                .ExpiresAt(now.AddMonths(1))
                .Build();
            revokedKey.Revoke(now.UtcDateTime);

            var expiredKey = new SigningKeyBuilder()
                .WithKey("expired")
                .CreatedAt(now.AddMonths(-12))
                .ExpiresAt(now.AddMonths(-1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddRangeAsync(activeKey1, activeKey2, revokedKey, expiredKey);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetActiveAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, k => k.Id == activeKey1.Id);
            Assert.Contains(result, k => k.Id == activeKey2.Id);
            Assert.DoesNotContain(result, k => k.Id == revokedKey.Id);
            Assert.DoesNotContain(result, k => k.Id == expiredKey.Id);
        }

        [Fact]
        public async Task ReturnsEmpty_WhenNoActiveKeys()
        {
            // Arrange
            var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            _time.SetUtcNow(now);

            var expiredKey = new SigningKeyBuilder()
                .WithKey("expired")
                .CreatedAt(now.AddMonths(-12))
                .ExpiresAt(now.AddMonths(-1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddAsync(expiredKey);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetActiveAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task OrdersByCreatedAtDescending()
        {
            // Arrange
            var now = _time.GetUtcNow();

            var olderActive = new SigningKeyBuilder()
                .WithKey("older")
                .CreatedAt(now.AddMonths(-3))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var newerActive = new SigningKeyBuilder()
                .WithKey("newer")
                .CreatedAt(now.AddMonths(-1))
                .ExpiresAt(now.AddMonths(1))
                .Build();

            var context = _fx.CreateContext();
            await context.SigningKeys.AddRangeAsync(olderActive, newerActive);
            await context.SaveChangesAsync();

            // Act
            var result = await _sut.GetActiveAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(newerActive.Id, result.ElementAt(0).Id);  // Most recent first
            Assert.Equal(olderActive.Id, result.ElementAt(1).Id);
        }
    }
}



public class SigningKeyBuilder
{
    private Key? _key;
    private KeyType _kty = KeyType.RSA;
    private SigningAlgorithm _alg = SigningAlgorithm.RS256;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _expiresAt;
    
    public SigningKeyBuilder WithKey(string key)
    {
        _key = new Key(key);
        return this;
    }

    public SigningKeyBuilder WithKty(KeyType kty)
    {
        _kty = kty;
        return this;
    }
    
    public SigningKeyBuilder WithAlg(SigningAlgorithm alg)
    {
        _alg = alg;
        return this;
    }

    public SigningKeyBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public SigningKeyBuilder ExpiresAt(DateTimeOffset expiresAt)
    {
        _expiresAt = expiresAt;
        return this;
    }

    public SigningKeyBuilder AsRsa()
    {
        _kty = KeyType.RSA;
        _alg = SigningAlgorithm.RS256;
        return this;
    }

    public SigningKeyBuilder AsHmac()
    {
        _kty = KeyType.HMAC;
        _alg = SigningAlgorithm.HS256;
        return this;
    }

    public SigningKey Build()
    {
        var key = _key ?? new Key("default-private-key");
        var keyMaterial = new KeyMaterial(key, _alg, _kty);
        var expiresAt = _expiresAt ?? _createdAt.AddHours(24);
        
        return new SigningKey(keyMaterial, _createdAt, expiresAt);
    }
}