using NSubstitute;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Test.Unit.SigningKeys;

public class SigningKeyServiceTests
{
    public SigningKeyServiceTests()
    {
        _service = new SigningKeyService(_repository, _keyFactory);
    }
    
    private readonly ISigningKeyService _service;
    private readonly ISigningKeyRepository _repository = Substitute.For<ISigningKeyRepository>();
    private readonly ISigningKeyFactory _keyFactory = Substitute.For<ISigningKeyFactory>();


    public class CreateAsync : SigningKeyServiceTests
    {
        [Fact]
        public async Task CallsFactory_WithExpectedArgument()
        {
            var expectedKey = SigningKey.CreateAsymmetric(SigningAlgorithm.Rsa, "public-key", "private-key");
            _keyFactory.Create(SigningAlgorithm.Rsa).Returns(expectedKey);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa);
            
            _keyFactory.Received(1).Create(SigningAlgorithm.Rsa);
            Assert.Equal(expectedKey.KeyId, created.KeyId);
        }

        [Fact]
        public async Task AddKeyToRepository_AndSaveChanges()
        {
            var expectedKey = SigningKey.CreateAsymmetric(SigningAlgorithm.Rsa, "public-key", "private-key");
            _keyFactory.Create(SigningAlgorithm.Rsa).Returns(expectedKey);

            await _service.CreateAsync(SigningAlgorithm.Rsa);

            _repository.Received(1).Add(expectedKey);
            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReturnsCreatedKey()
        {
            var expectedKey = SigningKey.CreateAsymmetric(SigningAlgorithm.Rsa, "public-key", "private-key");
            _keyFactory.Create(SigningAlgorithm.Rsa).Returns(expectedKey);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa);
            
            _keyFactory.Received(1).Create(SigningAlgorithm.Rsa);
            Assert.Same(expectedKey, created);           
        }

        [Fact]
        public async Task SetCorrectExpiresAt()
        {
            var expiresAt = DateTime.UtcNow.AddDays(30);
            var expectedKey = SigningKey.CreateAsymmetric(SigningAlgorithm.Rsa, "public-key", "private-key", expiresAt);
            _keyFactory.Create(SigningAlgorithm.Rsa, expiresAt).Returns(expectedKey);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa, expiresAt);
            
            _keyFactory.Received(1).Create(SigningAlgorithm.Rsa, expiresAt);
            Assert.Equal(expiresAt, created.ExpiresAt);                      
        }
    }
}