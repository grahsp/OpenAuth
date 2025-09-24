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
            var expected = new SigningKey(SigningAlgorithm.Rsa, new Key("private-key"), DateTime.MinValue, DateTime.MaxValue);
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTime?>())
                .Returns(expected);
            
            await _service.CreateAsync(SigningAlgorithm.Rsa);
            
            _keyFactory.Received(1).Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTime?>());
        }

        [Fact]
        public async Task AddKeyToRepository_AndSaveChanges()
        {
            await _service.CreateAsync(SigningAlgorithm.Rsa);

            _repository.Received(1).Add(Arg.Any<SigningKey>());
            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReturnsCreatedKey()
        {
            var expected = new SigningKey(SigningAlgorithm.Rsa, new Key("private-key"), DateTime.MinValue, DateTime.MaxValue);
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTime?>())
                .Returns(expected);
            
            var created = await _service.CreateAsync(SigningAlgorithm.Rsa);
            Assert.Same(expected, created);
        }
    }
}