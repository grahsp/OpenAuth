using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Dtos.Mappings;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.SigningKeys;

public class SigningKeyServiceTests
{
    public SigningKeyServiceTests()
    {
        _time = new FakeTimeProvider();
        _service = new SigningKeyService(_repository, _keyFactory, _time);
    }
    
    private readonly ISigningKeyService _service;
    private readonly ISigningKeyRepository _repository = Substitute.For<ISigningKeyRepository>();
    private readonly ISigningKeyFactory _keyFactory = Substitute.For<ISigningKeyFactory>();
    private readonly TimeProvider _time;


    public class CreateAsync : SigningKeyServiceTests
    {
        [Fact]
        public async Task CallsFactory_WithExpectedArgument()
        {
            var expected = TestSigningKey.CreateRsaSigningKey(_time);
            
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTimeOffset>())
                .Returns(expected);
            
            await _service.CreateAsync(SigningAlgorithm.RS256);
            
            _keyFactory.Received(1).Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public async Task AddKeyToRepository_AndSaveChanges()
        {
            var expected = TestSigningKey.CreateRsaSigningKey(_time);
            
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTimeOffset>())
                .Returns(expected);
            
            await _service.CreateAsync(SigningAlgorithm.RS256);

            _repository.Received(1).Add(Arg.Any<SigningKey>());
            await _repository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ReturnsCreatedKey()
        {
            var expected = TestSigningKey.CreateRsaSigningKey(_time);
            
            _keyFactory
                .Create(Arg.Any<SigningAlgorithm>(), Arg.Any<DateTimeOffset>())
                .Returns(expected);
            
            var created = await _service.CreateAsync(SigningAlgorithm.RS256);
            Assert.Equal(expected.ToSigningKeyInfo(), created);
        }
    }
}