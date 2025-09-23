using NSubstitute;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

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
            var expectedKey = new SigningKey(SigningAlgorithm.Rsa, new Key("private-key"));
            _keyFactory.Create(SigningAlgorithm.Rsa).Returns(expectedKey);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa);
            
            _keyFactory.Received(1).Create(SigningAlgorithm.Rsa);
            Assert.Equal(expectedKey.Id, created.Id);
        }

        [Fact]
        public async Task AddKeyToRepository_AndSaveChanges()
        {
            var expectedKey = new SigningKey(SigningAlgorithm.Rsa, new Key("private-key"));
            _keyFactory.Create(SigningAlgorithm.Rsa).Returns(expectedKey);

            await _service.CreateAsync(SigningAlgorithm.Rsa);

            _repository.Received(1).Add(expectedKey);
            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReturnsCreatedKey()
        {
            var expectedKey = new SigningKey(SigningAlgorithm.Rsa, new Key("private-key"));
            _keyFactory.Create(SigningAlgorithm.Rsa).Returns(expectedKey);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa);
            
            _keyFactory.Received(1).Create(SigningAlgorithm.Rsa);
            Assert.Same(expectedKey, created);           
        }

        [Fact]
        public async Task SetCorrectExpiresAt()
        {
            var expiresAt = DateTime.UtcNow.AddDays(30);
            var expectedKey = new SigningKey(SigningAlgorithm.Rsa, new Key("private-key"), expiresAt);
            _keyFactory.Create(SigningAlgorithm.Rsa, expiresAt).Returns(expectedKey);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa, expiresAt);
            
            _keyFactory.Received(1).Create(SigningAlgorithm.Rsa, expiresAt);
            Assert.Equal(expiresAt, created.ExpiresAt);                      
        }
    }
}