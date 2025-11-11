using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Secrets.Interfaces;
using OpenAuth.Application.Secrets.Services;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Shared.Interfaces;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients.Application;

public class SecretServiceTests
{
    private FakeClientRepository _repo;
    private ISecretGenerator _generator;
    private IHasher _hasher;
    private FakeTimeProvider _time;

    public SecretServiceTests()
    {
        _repo = new FakeClientRepository();
        _generator = Substitute.For<ISecretGenerator>();
        _hasher = Substitute.For<IHasher>();
        _time = new FakeTimeProvider();
        
        _generator.Generate().Returns("secret");
        _hasher.Hash(Arg.Any<string>()).Returns("$2a$10$hash");
    }

    private SecretService CreateSut()
        => new(_repo, _generator, _hasher, _time);


    public class AddSecretAsync : SecretServiceTests
    {
        [Fact]
        public async Task Success_AddsSecretToClient()
        {
            // Arrange
            var service = CreateSut();
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            // Act
            await service.AddSecretAsync(client.Id);
            
            // Assert
            Assert.Single(client.Secrets);
        }

        [Fact]
        public async Task Success_ReturnsCorrectResponse()
        {
            // Arrange
            var service = CreateSut();
            var client = new ClientBuilder().Build();
            _repo.Add(client);

            var expectedPlain = "secret";
            var expectedHash = "$2a$10$hash";
            _generator.Generate().Returns(expectedPlain);
            _hasher.Hash(Arg.Any<string>()).Returns(expectedHash);
            
            _time.Advance(TimeSpan.FromHours(1));
            var expectedUtcNow = _time.GetUtcNow();
            
            // Act
            var result = await service.AddSecretAsync(client.Id);
            
            // Assert
            Assert.Equal(expectedPlain, result.PlainTextSecret);
            Assert.Equal(expectedUtcNow, result.CreatedAt);
            
            var secret = client.Secrets.Single();
            Assert.Equal(expectedHash, secret.Hash.Value);
            Assert.Equal(secret.Id, result.SecretId);
        }
        
        [Fact]
        public async Task ClientNotFound_ThrowsException()
        {
            // Arrange
            var service = CreateSut();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(()
                => service.AddSecretAsync(ClientId.New()));
        }

        [Fact]
        public async Task CallsSecretGenerator()
        {
            // Arrange
            var service = CreateSut();
            var client = new ClientBuilder().Build();
            _repo.Add(client);

            // Act
            await service.AddSecretAsync(client.Id);
            
            // Assert
            _generator.Received(1).Generate();
        }

        [Fact]
        public async Task Success_PassGeneratedSecretToHasher()
        {
            // Arrange
            var service = CreateSut();
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            var expected = "generated-secret";
            _generator.Generate().Returns(expected);
            
            // Act
            await service.AddSecretAsync(client.Id);
            
            // Assert
            _hasher.Received(1).Hash(expected);
        }

        [Fact]
        public async Task UsesCurrentTimeOfTimeProvider()
        {
            // Arrange
            var service = CreateSut();
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            _time.Advance(TimeSpan.FromHours(5));
            var expected = _time.GetUtcNow();
            
            // Act
            await service.AddSecretAsync(client.Id);
            
            // Assert
            var secret = client.Secrets.Single();
            Assert.Equal(expected, secret.CreatedAt);
        }

        [Fact]
        public async Task SaveChanges()
        {
            // Arrange
            var service = CreateSut();
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            // Act
            await service.AddSecretAsync(client.Id);
            
            // Assert
            Assert.True(_repo.Saved);
        }
    }

    public class RevokeSecretAsync : SecretServiceTests
    {
        [Fact]
        public async Task ClientNotFound_ThrowsException()
        {
            // Arrange
            var service = CreateSut();

            // Act & Assert
            Assert.False(_repo.Saved);
            await Assert.ThrowsAsync<InvalidOperationException>(()
                => service.RevokeSecretAsync(ClientId.New(), SecretId.New()));
        }

        [Fact]
        public async Task Success_RevokeSecret()
        {
            // Arrange
            var service = CreateSut();
            
            // Add additional secret because the last active secret cannot be revoked.
            var client = new ClientBuilder()
                .WithSecret("hash")
                .WithSecret("hash")
                .Build();
            
            _repo.Add(client);
            var expected = _time.GetUtcNow();
            
            // Act
            var secret = client.Secrets.First();
            await service.RevokeSecretAsync(client.Id, secret.Id);

            // Assert
            Assert.Equal(expected, secret.RevokedAt);
        }
        
        [Fact]
        public async Task Success_SaveChanges()
        {
            // Arrange
            var service = CreateSut();
            
            // Add additional secret because the last active secret cannot be revoked.
            var client = new ClientBuilder()
                .WithSecret("hash")
                .WithSecret("hash")
                .Build();
            
            _repo.Add(client);
            
            // Act
            await service.RevokeSecretAsync(client.Id, client.Secrets.First().Id);
            
            // Assert
            Assert.True(_repo.Saved);
        }
    }
}