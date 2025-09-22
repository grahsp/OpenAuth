using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Test.Common.Fakes;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.SigningKeys;

[Collection("sqlserver")]
public class SigningKeyServiceTests : IAsyncLifetime
{
    public SigningKeyServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
    }
    
    private readonly SqlServerFixture _fx;
    

    public async Task InitializeAsync()
        => await _fx.ResetAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;

    private SigningKeyService CreateSut()
    {
        var context = _fx.CreateContext();
        return new SigningKeyService(new SigningKeyRepository(context), new FakeSigningKeyFactory());
    }
    
    public class GetByIdAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsKey_WhenKeyExists()
        {
            var service = CreateSut();

            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var fetched = await service.GetByIdAsync(key.KeyId);
            Assert.NotNull(fetched);
            Assert.Equal(key.KeyId, fetched.KeyId);
        }

        [Fact]
        public async Task ReturnsNull_WhenKeyDoesNotExist()
        {
            var service = CreateSut();
            
            var fetched = await service.GetByIdAsync(SigningKeyId.New());
            Assert.Null(fetched);
        }
    }

    public class GetCurrentAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsKey_WhenSingleKeyExists()
        {
            var service = CreateSut();

            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var fetched = await service.GetCurrentAsync();
            Assert.NotNull(fetched);
            Assert.Equal(key.KeyId, fetched.KeyId);
        }
        
        [Fact]
        public async Task ReturnsCurrentKey_WhenMultipleActiveKeysExists()
        {
            var service = CreateSut();

            await service.CreateAsync(SigningAlgorithm.Rsa);
            await Task.Delay(10);
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var fetched = await service.GetCurrentAsync();
            Assert.NotNull(fetched);
            Assert.Equal(key.KeyId, fetched.KeyId);
        }

        [Fact]
        public async Task ReturnsCurrentKey_WhenLatestKeyIsRevoked()
        {
            var service = CreateSut();
            
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var revoked = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.RevokeAsync(revoked.KeyId);
            
            var fetched = await service.GetCurrentAsync();
            Assert.NotNull(fetched);
            Assert.Equal(key.KeyId, fetched.KeyId);           
        }
        
        [Fact]
        public async Task ReturnsCurrentKey_WhenLatestKeyIsExpired()
        {
            var service = CreateSut();
            
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var expired = await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow);
            await service.RevokeAsync(expired.KeyId);
            
            var fetched = await service.GetCurrentAsync();
            Assert.NotNull(fetched);
            Assert.Equal(key.KeyId, fetched.KeyId);           
        }
        
        [Fact]
        public async Task ReturnsNull_WhenNoKeysExist()
        {
            var service = CreateSut();
            
            var fetched = await service.GetCurrentAsync();
            Assert.Null(fetched);       
        }
        
        [Fact]
        public async Task ReturnsNull_WhenNoActiveKeys()
        {
            var service = CreateSut();
            
            
            await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow.AddDays(-1));
            await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow.AddDays(-1));
            
            var fetched = await service.GetCurrentAsync();
            Assert.Null(fetched);
        }
    }

    public class GetAllAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsExistingKeys()
        {
            var service = CreateSut();

            await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.CreateAsync(SigningAlgorithm.Rsa);

            var fetched = await service.GetAllAsync();
            Assert.Equal(2, fetched.Count());
        }

        [Fact]
        public async Task ReturnsExistingExpiredKeys()
        {
            var service = CreateSut();
            
            await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow.AddDays(-1));
            
            var fetched = await service.GetAllAsync();
            Assert.False(fetched.First().IsActive());
        }
        
        [Fact]
        public async Task ReturnsExistingRevokedKeys()
        {
            var service = CreateSut();
            
            var revoked = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.RevokeAsync(revoked.KeyId);
            
            var fetched = await service.GetAllAsync();
            Assert.False(fetched.First().IsActive());
        }

        [Fact]
        public async Task ReturnsExistingKeys_OrderedByCreationDate()
        {
            var service = CreateSut();

            await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.CreateAsync(SigningAlgorithm.Rsa);

            var fetched = (await service.GetAllAsync()).ToArray();
            
            for (var i = 1; i < fetched.Length; i++)
                Assert.True(fetched[i].CreatedAt <= fetched[i - 1].CreatedAt);
        }

        [Fact]
        public async Task ReturnsEmptyList_WhenNoKeysExist()
        {
            var service = CreateSut();
            
            var fetched = await service.GetAllAsync();
            Assert.Empty(fetched);
        }
    }
    
    public class GetActiveAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task ReturnsActiveKeys()
        {
            var service = CreateSut();
            
            await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.CreateAsync(SigningAlgorithm.Rsa);
            
            var fetched = await service.GetActiveAsync();
            Assert.Equal(2, fetched.Count());
        }
        
        [Fact]
        public async Task ReturnsActiveKeys_IgnoreRevoked()
        {
            var service = CreateSut();
            
            var active = await service.CreateAsync(SigningAlgorithm.Rsa);
            
            var revoked = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.RevokeAsync(revoked.KeyId);
            
            var fetched = (await service.GetActiveAsync()).ToArray();
            Assert.Single(fetched);
            Assert.Contains(active.KeyId, fetched.Select(k => k.KeyId));;
        }
        
        [Fact]
        public async Task ReturnsActiveKeys_IgnoreExpired()
        {
            var service = CreateSut();
            
            var active = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow.AddDays(-1));
            
            var fetched = (await service.GetActiveAsync()).ToArray();
            Assert.Single(fetched);
            Assert.Contains(active.KeyId, fetched.Select(k => k.KeyId));;
        }

        [Fact]
        public async Task ReturnsEmptyList_WhenNoKeysExist()
        {
            var service = CreateSut();
            
            var fetched = await service.GetActiveAsync();
            Assert.Empty(fetched);       
        }
    }
}