using Microsoft.EntityFrameworkCore;
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
            
            var fetched = await service.GetByIdAsync(key.Id);
            Assert.NotNull(fetched);
            Assert.Equal(key.Id, fetched.Id);
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
            Assert.Equal(key.Id, fetched.Id);
        }
        
        // [Fact]
        // public async Task ReturnsCurrentKey_WhenMultipleActiveKeysExists()
        // {
        //     var service = CreateSut();
        //
        //     await service.CreateAsync(SigningAlgorithm.Rsa);
        //     await Task.Delay(10);
        //     var key = await service.CreateAsync(SigningAlgorithm.Rsa);
        //     Assert.NotNull(key);
        //     
        //     var fetched = await service.GetCurrentAsync();
        //     Assert.NotNull(fetched);
        //     Assert.Equal(key.Id, fetched.Id);
        // }

        [Fact]
        public async Task ReturnsCurrentKey_WhenLatestKeyIsRevoked()
        {
            var service = CreateSut();
            
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var revoked = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.RevokeAsync(revoked.Id);
            
            var fetched = await service.GetCurrentAsync();
            Assert.NotNull(fetched);
            Assert.Equal(key.Id, fetched.Id);           
        }
        
        [Fact]
        public async Task ReturnsCurrentKey_WhenLatestKeyIsExpired()
        {
            var service = CreateSut();
            
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotNull(key);
            
            var expired = await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow);
            await service.RevokeAsync(expired.Id);
            
            var fetched = await service.GetCurrentAsync();
            Assert.NotNull(fetched);
            Assert.Equal(key.Id, fetched.Id);           
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
            
            await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.MinValue);
            
            var fetched = await service.GetAllAsync();
            Assert.False(fetched.First().IsActive(DateTime.MaxValue));
        }
        
        [Fact]
        public async Task ReturnsExistingRevokedKeys()
        {
            var service = CreateSut();
            
            var revoked = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.RevokeAsync(revoked.Id);
            
            var fetched = await service.GetAllAsync();
            Assert.False(fetched.First().IsActive(DateTime.MinValue));
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
            await service.RevokeAsync(revoked.Id);
            
            var fetched = (await service.GetActiveAsync()).ToArray();
            Assert.Single(fetched);
            Assert.Contains(active.Id, fetched.Select(k => k.Id));;
        }
        
        [Fact]
        public async Task ReturnsActiveKeys_IgnoreExpired()
        {
            var service = CreateSut();
            
            var active = await service.CreateAsync(SigningAlgorithm.Rsa);
            await service.CreateAsync(SigningAlgorithm.Rsa, DateTime.UtcNow.AddDays(-1));
            
            var fetched = (await service.GetActiveAsync()).ToArray();
            Assert.Single(fetched);
            Assert.Contains(active.Id, fetched.Select(k => k.Id));;
        }

        [Fact]
        public async Task ReturnsEmptyList_WhenNoKeysExist()
        {
            var service = CreateSut();
            
            var fetched = await service.GetActiveAsync();
            Assert.Empty(fetched);       
        }
    }

    public class CreateAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task PersistSigningKey_WhenCreated()
        {
            var service = CreateSut();
            var created = await service.CreateAsync(SigningAlgorithm.Rsa);
            
            await using var context = _fx.CreateContext();
            
            var fetched = await context.SigningKeys.FirstOrDefaultAsync(k => k.Id == created.Id);
            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
        }

        [Fact]
        public async Task CreatesUniqueKeyIds()
        {
            var service = CreateSut();
            
            var keyA = await service.CreateAsync(SigningAlgorithm.Rsa);
            var keyB = await service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.NotEqual(keyA.Id, keyB.Id);       
            
            var fetched = (await service.GetAllAsync()).ToArray();
            foreach (var key in fetched)
                Assert.Contains(fetched, k => k.Id == key.Id);
        }
    }

    public class RevokeAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task PersistChanges_WhenRevoked()
        {
            var service = CreateSut();
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            
            var revoked = await service.RevokeAsync(key.Id);
            Assert.True(revoked);

            // Create new context to ensure changes are persisted
            await using var context = _fx.CreateContext();
            
            var fetched = await context.SigningKeys.FirstOrDefaultAsync(k => k.Id == key.Id);
            Assert.NotNull(fetched);
            Assert.False(fetched.IsActive(DateTime.MaxValue));
        }
    }

    public class RemoveAsync(SqlServerFixture fx) : SigningKeyServiceTests(fx)
    {
        [Fact]
        public async Task PersistChanges_WhenRemoved()
        {
            var service = CreateSut();
            var key = await service.CreateAsync(SigningAlgorithm.Rsa);
            
            var removed = await service.RemoveAsync(key.Id);
            Assert.True(removed);
            
            await using var context = _fx.CreateContext();
            
            var fetched = await context.SigningKeys.FirstOrDefaultAsync(k => k.Id == key.Id);
            Assert.Null(fetched);       
        }
    }
}