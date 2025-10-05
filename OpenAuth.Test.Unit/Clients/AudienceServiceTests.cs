using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients;

public class AudienceServiceTests
{
    private readonly IClientRepository _repo;
    private readonly FakeTimeProvider _time;
    private readonly AudienceService _sut;

    public AudienceServiceTests()
    {
        _repo = Substitute.For<IClientRepository>();
        _time = new FakeTimeProvider();
        _sut = new AudienceService(_repo, _time);
    }

    public class AddAudienceAsync : AudienceServiceTests
    {
        [Fact]
        public async Task ReturnsAudienceInfo()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");

            // Act
            var result = await _sut.AddAudienceAsync(clientId, audienceName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(audienceName, result.Name);
            await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = new ClientId(Guid.NewGuid());
            var audienceName = new AudienceName("api.example.com");

            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns((Client?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AddAudienceAsync(clientId, audienceName));
            
            await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PassesCancellationToken()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _repo.GetByIdAsync(clientId, token)
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");

            // Act
            await _sut.AddAudienceAsync(clientId, audienceName, token);

            // Assert
            await _repo.Received(1).GetByIdAsync(clientId, token);
            await _repo.Received(1).SaveChangesAsync(token);
        }


    }

    public class RemoveAudienceAsync : AudienceServiceTests
    {
        [Fact]
        public async Task RemovesAudience()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            await _sut.RemoveAudienceAsync(clientId, audienceName);

            // Assert
            Assert.DoesNotContain(client.Audiences, a => a.Name == audienceName);
            await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns((Client?)null);
            
            var audienceName = new AudienceName("api.example.com");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.RemoveAudienceAsync(clientId, audienceName));
            
            await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PassesCancellationToken()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            
            _repo.GetByIdAsync(clientId, token)
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            await _sut.RemoveAudienceAsync(clientId, audienceName, token);

            // Assert
            await _repo.Received(1).GetByIdAsync(clientId, token);
            await _repo.Received(1).SaveChangesAsync(token);
        }
    }

    public class SetScopesAsync : AudienceServiceTests
    {
        [Fact]
        public async Task ReturnsUpdatedAudienceInfo()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());
            var newScopes = new List<Scope> { new("write"), new("delete") };

            // Act
            var result = await _sut.SetScopesAsync(clientId, audienceName, newScopes);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(audienceName, result.Name);
            Assert.Equal(2, result.Scopes.Count());
            Assert.Contains(new Scope("write"), result.Scopes);
            Assert.Contains(new Scope("delete"), result.Scopes);
            Assert.DoesNotContain(new Scope("read"), result.Scopes);
            
            await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns((Client?)null);
            
            var audienceName = new AudienceName("api.example.com");
            var scopes = new List<Scope> { new("read") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.SetScopesAsync(clientId, audienceName, scopes));
            
            await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WithEmptyScopes_ClearsAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write")], _time.GetUtcNow());
            var emptyScopes = Array.Empty<Scope>();

            // Act
            var result = await _sut.SetScopesAsync(clientId, audienceName, emptyScopes);

            // Assert
            Assert.Empty(result.Scopes);
        }

        [Fact]
        public async Task PassesCancellationToken()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            
            _repo.GetByIdAsync(clientId, token)
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            var scopes = new List<Scope> { new("read") };

            // Act
            await _sut.SetScopesAsync(clientId, audienceName, scopes, token);

            // Assert
            await _repo.Received(1).GetByIdAsync(clientId, token);
            await _repo.Received(1).SaveChangesAsync(token);
        }
    }

    public class GrantScopesAsync : AudienceServiceTests
    {
        [Fact]
        public async Task ReturnsUpdatedAudienceInfo()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());
            var scopesToGrant = new List<Scope> { new("write") };

            // Act
            var result = await _sut.GrantScopesAsync(clientId, audienceName, scopesToGrant);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Scopes.Count());
            Assert.Contains(new Scope("read"), result.Scopes);
            Assert.Contains(new Scope("write"), result.Scopes);
            
            await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns((Client?)null);
            
            var audienceName = new AudienceName("api.example.com");
            var scopes = new List<Scope> { new("read") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GrantScopesAsync(clientId, audienceName, scopes));
            
            await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WithMultipleScopes_GrantsAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            var scopesToGrant = new List<Scope> { new("read"), new("write"), new("delete") };

            // Act
            var result = await _sut.GrantScopesAsync(clientId, audienceName, scopesToGrant);

            // Assert
            Assert.Equal(3, result.Scopes.Count());
            foreach (var scope in scopesToGrant)
                Assert.Contains(scope, result.Scopes);
        }

        [Fact]
        public async Task PassesCancellationToken()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _repo.GetByIdAsync(clientId, token)
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            var scopes = new List<Scope> { new("read") };
            
            // Act
            await _sut.GrantScopesAsync(clientId, audienceName, scopes, token);

            // Assert
            await _repo.Received(1).GetByIdAsync(clientId, token);
            await _repo.Received(1).SaveChangesAsync(token);
        }
    }

    public class RevokeScopesAsync : AudienceServiceTests
    {
        [Fact]
        public async Task ReturnsUpdatedAudienceInfo()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write"), new Scope("delete")], _time.GetUtcNow());
            var scopesToRevoke = new List<Scope> { new("write"), new("delete") };

            // Act
            var result = await _sut.RevokeScopesAsync(clientId, audienceName, scopesToRevoke);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Scopes);
            Assert.Contains(new Scope("read"), result.Scopes);
            Assert.DoesNotContain(new Scope("write"), result.Scopes);
            Assert.DoesNotContain(new Scope("delete"), result.Scopes);
            
            await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns((Client?)null);
            
            var audienceName = new AudienceName("api.example.com");
            var scopes = new List<Scope> { new("read") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.RevokeScopesAsync(clientId, audienceName, scopes));
            
            await _repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WithMultipleScopes_RevokesAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            _repo.GetByIdAsync(clientId, Arg.Any<CancellationToken>())
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write"), new Scope("delete")], _time.GetUtcNow());
            var scopesToRevoke = new[] { new Scope("write"), new Scope("delete") };

            // Act
            var result = await _sut.RevokeScopesAsync(clientId, audienceName, scopesToRevoke);

            // Assert
            Assert.Single(result.Scopes);
            Assert.Contains(new Scope("read"), result.Scopes);
        }

        [Fact]
        public async Task PassesCancellationToken()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var clientId = new ClientId(Guid.NewGuid());
            
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            
            _repo.GetByIdAsync(clientId, token)
                .Returns(client);
            
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            var scopes = new[] { new Scope("read") };
            client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Act
            await _sut.RevokeScopesAsync(clientId, audienceName, scopes, token);

            // Assert
            await _repo.Received(1).GetByIdAsync(clientId, token);
            await _repo.Received(1).SaveChangesAsync(token);
        }
    }
}