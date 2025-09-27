using Microsoft.Extensions.Time.Testing;
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
        var time = new FakeTimeProvider();
        _service = new SigningKeyService(_repository, _keyFactory, time);
    }
    
    private readonly ISigningKeyService _service;
    private readonly ISigningKeyRepository _repository = Substitute.For<ISigningKeyRepository>();
    private readonly ISigningKeyFactory _keyFactory = Substitute.For<ISigningKeyFactory>();


    public class CreateAsync : SigningKeyServiceTests
    {
        [Fact]
        public async Task CallsFactory_WithExpectedArgument()
        {
            var keyMaterial = new KeyMaterial(new Key("private-key"), SigningAlgorithm.RS256, KeyType.RSA);
            var expected = new SigningKey(keyMaterial, DateTime.MinValue, DateTime.MaxValue);
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTime>())
                .Returns(expected);
            
            await _service.CreateAsync(SigningAlgorithm.RS256);
            
            _keyFactory.Received(1).Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTime>());
        }

        [Fact]
        public async Task AddKeyToRepository_AndSaveChanges()
        {
            await _service.CreateAsync(SigningAlgorithm.RS256);

            _repository.Received(1).Add(Arg.Any<SigningKey>());
            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReturnsCreatedKey()
        {
            var keyMaterial = new KeyMaterial(new Key("private-key"), SigningAlgorithm.RS256, KeyType.RSA);
            var expected = new SigningKey(keyMaterial, DateTime.MinValue, DateTime.MaxValue);
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTime>())
                .Returns(expected);
            
            var created = await _service.CreateAsync(SigningAlgorithm.RS256);
            Assert.Same(expected, created);
        }
    }
}