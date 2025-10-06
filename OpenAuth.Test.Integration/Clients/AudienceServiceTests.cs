using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Clients.Persistence;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients;

[Collection("sqlserver")]
public class AudienceServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly FakeTimeProvider _time;
    private readonly AudienceService _sut;

    public AudienceServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
        
        var context = _fx.CreateContext();
        var repository = new ClientRepository(context);
        _sut = new AudienceService(repository, _time);
    }

    public async Task InitializeAsync()
        => await _fx.ResetAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;

    public class AddAudienceAsync(SqlServerFixture fx) : AudienceServiceTests(fx)
    {
        [Fact]
        public async Task CreatesNewAudience_AndPersistsToDatabase()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                var result = await _sut.AddAudienceAsync(client.Id, audienceName);
                
                Assert.NotNull(result);
                Assert.Equal(audienceName, result.Name);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                Assert.Contains(retrievedClient.Audiences, a => a.Name == audienceName);
            }
        }

        [Fact]
        public async Task WhenClientDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());
            var audienceName = new AudienceName("api.example.com");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.AddAudienceAsync(nonExistentClientId, audienceName));
        }

        [Fact]
        public async Task WithMultipleAudiences_AddsAllSuccessfully()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("TestClient")
                .Build();
            
            var audience1 = new AudienceName("api1.example.com");
            var audience2 = new AudienceName("api2.example.com");

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                await _sut.AddAudienceAsync(client.Id, audience1);
                await _sut.AddAudienceAsync(client.Id, audience2);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                Assert.Equal(2, retrievedClient.Audiences.Count);
                Assert.Contains(retrievedClient.Audiences, a => a.Name == audience1);
                Assert.Contains(retrievedClient.Audiences, a => a.Name == audience2);
            }
        }

        [Fact]
        public async Task WithDuplicateName_HandlesAccordingToDomainRules()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act & Assert
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            await Assert.ThrowsAnyAsync<Exception>(
                () => _sut.AddAudienceAsync(client.Id, audienceName));
        }
    }

    public class RemoveAudienceAsync(SqlServerFixture fx) : AudienceServiceTests(fx)
    {
        [Fact]
        public async Task RemovesExistingAudience_AndPersistsToDatabase()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                await _sut.RemoveAudienceAsync(client.Id, audienceName);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                Assert.DoesNotContain(retrievedClient.Audiences, a => a.Name == audienceName);
            }
        }

        [Fact]
        public async Task WhenClientDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());
            var audienceName = new AudienceName("api.example.com");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.RemoveAudienceAsync(nonExistentClientId, audienceName));
        }

        [Fact]
        public async Task RemovesOnlySpecifiedAudience()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audience1 = new AudienceName("api1.example.com");
            var audience2 = new AudienceName("api2.example.com");
            client.AddAudience(audience1, _time.GetUtcNow());
            client.AddAudience(audience2, _time.GetUtcNow());

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                await _sut.RemoveAudienceAsync(client.Id, audience1);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                Assert.DoesNotContain(retrievedClient.Audiences, a => a.Name == audience1);
                Assert.Contains(retrievedClient.Audiences, a => a.Name == audience2);
            }
        }

        [Fact]
        public async Task AudienceDoesNotExist_ThrowsException()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var nonExistentAudience = new AudienceName("nonexistent.example.com");

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.RemoveAudienceAsync(client.Id, nonExistentAudience));
        }
    }

    public class SetScopesAsync(SqlServerFixture fx) : AudienceServiceTests(fx)
    {
        [Fact]
        public async Task ReplacesAllScopes_AndPersistsToDatabase()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write")], _time.GetUtcNow());

            var newScopes = new List<Scope> { new("admin"), new("delete") };

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                var result = await _sut.SetScopesAsync(client.Id, audienceName, newScopes);
                
                Assert.Equal(2, result.Scopes.Count());
                Assert.Contains(new Scope("admin"), result.Scopes);
                Assert.Contains(new Scope("delete"), result.Scopes);
                Assert.DoesNotContain(new Scope("read"), result.Scopes);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                var retrievedAudience = retrievedClient.GetAudience(audienceName);
                Assert.Equal(2, retrievedAudience.Scopes.Count);
                Assert.Contains(retrievedAudience.Scopes, s => s.Value == "admin");
                Assert.Contains(retrievedAudience.Scopes, s => s.Value == "delete");
            }
        }

        [Fact]
        public async Task WithEmptyList_RemovesAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write")], _time.GetUtcNow());

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                var result = await _sut.SetScopesAsync(client.Id, audienceName, Array.Empty<Scope>());
                Assert.Empty(result.Scopes);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                var retrievedAudience = retrievedClient.GetAudience(audienceName);
                Assert.Empty(retrievedAudience.Scopes);
            }
        }

        [Fact]
        public async Task WhenClientDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());
            var audienceName = new AudienceName("api.example.com");
            var scopes = new List<Scope> { new("read") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.SetScopesAsync(nonExistentClientId, audienceName, scopes));
        }

        [Fact]
        public async Task WithSingleScope_ReplacesCorrectly()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write"), new Scope("delete")], _time.GetUtcNow());

            var newScopes = new List<Scope> { new("admin") };

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            var result = await _sut.SetScopesAsync(client.Id, audienceName, newScopes);
                
            Assert.Single(result.Scopes);
            Assert.Contains(new Scope("admin"), result.Scopes);
        }
    }

    public class GrantScopesAsync(SqlServerFixture fx) : AudienceServiceTests(fx)
    {
        [Fact]
        public async Task AddsNewScopes_AndPersistsToDatabase()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());

            var scopesToGrant = new List<Scope> { new("write"), new("delete") };

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                var result = await _sut.GrantScopesAsync(client.Id, audienceName, scopesToGrant);
                
                Assert.Equal(3, result.Scopes.Count());
                Assert.Contains(new Scope("read"), result.Scopes);
                Assert.Contains(new Scope("write"), result.Scopes);
                Assert.Contains(new Scope("delete"), result.Scopes);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                var retrievedAudience = retrievedClient.GetAudience(audienceName);
                Assert.Equal(3, retrievedAudience.Scopes.Count);
            }
        }

        [Fact]
        public async Task WhenClientDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());
            var audienceName = new AudienceName("api.example.com");
            var scopes = new List<Scope> { new("read") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GrantScopesAsync(nonExistentClientId, audienceName, scopes));
        }

        [Fact]
        public async Task WithMultipleScopes_GrantsAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            var scopesToGrant = new List<Scope> { new("read"), new("write"), new("delete") };

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            var result = await _sut.GrantScopesAsync(client.Id, audienceName, scopesToGrant);
                
            Assert.Equal(3, result.Scopes.Count());
            foreach (var scope in scopesToGrant)
            {
                Assert.Contains(scope, result.Scopes);
            }
        }

        [Fact]
        public async Task WithDuplicateScope_HandlesAccordingToDomainRules()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());

            var duplicateScope = new List<Scope> { new("read") };

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            var result = await _sut.GrantScopesAsync(client.Id, audienceName, duplicateScope);
                
            Assert.Contains(new Scope("read"), result.Scopes);
        }

        [Fact]
        public async Task ToAudienceWithNoScopes_AddsSuccessfully()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());

            var scopesToGrant = new List<Scope> { new("admin") };

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            var result = await _sut.GrantScopesAsync(client.Id, audienceName, scopesToGrant);
                
            Assert.Single(result.Scopes);
            Assert.Contains(new Scope("admin"), result.Scopes);
        }
    }

    public class RevokeScopesAsync(SqlServerFixture fx) : AudienceServiceTests(fx)
    {
        [Fact]
        public async Task RemovesSpecifiedScopes_AndPersistsToDatabase()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write"), new Scope("delete")], _time.GetUtcNow());

            var scopesToRevoke = new List<Scope> { new("write"), new("delete") };

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                var result = await _sut.RevokeScopesAsync(client.Id, audienceName, scopesToRevoke);
                
                Assert.Single(result.Scopes);
                Assert.Contains(new Scope("read"), result.Scopes);
                Assert.DoesNotContain(new Scope("write"), result.Scopes);
                Assert.DoesNotContain(new Scope("delete"), result.Scopes);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                var retrievedAudience = retrievedClient.GetAudience(audienceName);
                Assert.Single(retrievedAudience.Scopes);
                Assert.Contains(retrievedAudience.Scopes, s => s.Value == "read");
            }
        }

        [Fact]
        public async Task WhenClientDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var nonExistentClientId = new ClientId(Guid.NewGuid());
            var audienceName = new AudienceName("api.example.com");
            var scopes = new List<Scope> { new("read") };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.RevokeScopesAsync(nonExistentClientId, audienceName, scopes));
        }

        [Fact]
        public async Task WithMultipleScopes_RevokesAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName,
            [new Scope("read"), new Scope("write"), new Scope("delete"), new Scope("admin")],
                _time.GetUtcNow());

            var scopesToRevoke = new List<Scope> { new("write"), new("delete"), new("admin") };

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            var result = await _sut.RevokeScopesAsync(client.Id, audienceName, scopesToRevoke);
                
            Assert.Single(result.Scopes);
            Assert.Contains(new Scope("read"), result.Scopes);
        }

        [Fact]
        public async Task WhenScopeDoesNotExist_HandlesAccordingToDomainRules()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api.example.com");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read")], _time.GetUtcNow());

            var nonExistentScope = new List<Scope> { new("nonexistent") };

            // Act
            await using var context = _fx.CreateContext();
            
            context.Clients.Add(client);
            await context.SaveChangesAsync();
                
            var result = await _sut.RevokeScopesAsync(client.Id, audienceName, nonExistentScope);
                
            Assert.Single(result.Scopes);
            Assert.Contains(new Scope("read"), result.Scopes);
        }

        [Fact]
        public async Task RevokingAllScopes_LeavesEmptyList()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());
            
            var scopes = new[] { new Scope("read"), new Scope("write") };
            client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Act
            await using (var context = _fx.CreateContext())
            {
                context.Clients.Add(client);
                await context.SaveChangesAsync();
                
                var result = await _sut.RevokeScopesAsync(client.Id, audienceName, scopes);
                Assert.Empty(result.Scopes);
            }

            // Assert
            await using (var context = _fx.CreateContext())
            {
                var retrievedClient = await context.Clients.SingleAsync(c => c.Id == client.Id);
                var retrievedAudience = retrievedClient.GetAudience(audienceName);
                Assert.Empty(retrievedAudience.Scopes);
            }
        }
    }
}