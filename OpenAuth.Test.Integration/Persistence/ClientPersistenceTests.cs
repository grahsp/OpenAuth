using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Entities;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Persistence;

[Collection("sqlserver")]
public class ClientPersistenceTests : IAsyncLifetime
{
    private SqlServerFixture _fx;
    private FakeTimeProvider _time;

    public ClientPersistenceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _time = new FakeTimeProvider();
    }

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;


    public class Basic(SqlServerFixture fx) : ClientPersistenceTests(fx)
    {
        [Fact]
        public async Task SavedClient_CanBeRetrieved_WithAllFieldsIntact()
        {
            // Arrange
            var client = new ClientBuilder()
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.Equal(client.Id, loaded.Id);
                Assert.Equal(client.Name, loaded.Name);
                Assert.Equal(client.CreatedAt, loaded.CreatedAt);
                Assert.Equal(client.UpdatedAt, loaded.UpdatedAt);
            }
        }
    
        [Fact]
        public async Task ClientWithoutSecretsOrAudiences_SavesSuccessfully()
        {
            // Arrange
            var client = new ClientBuilder()
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.NotNull(loaded);
                Assert.Equal(client.Name, loaded.Name);
                Assert.Empty(loaded.Secrets);
                Assert.Empty(loaded.Audiences);
            }
        }
        
        [Fact]
        public async Task MultipleClients_CanExist_WithDifferentNames()
        {
            // Arrange
            var client1 = new ClientBuilder().WithName("client-one").Build();
            var client2 = new ClientBuilder().WithName("client-two").Build();
        
            await using var ctx = _fx.CreateContext();
            ctx.AddRange(client1, client2);
            await ctx.SaveChangesAsync();

            // Act
            var count = await ctx.Clients.CountAsync();

            // Assert
            Assert.Equal(2, count);
        }
    }
    

    public class Secrets(SqlServerFixture fx) : ClientPersistenceTests(fx)
    {
        [Fact]
        public async Task ClientWithSecret_SavesWithCorrectExpirationDates()
        {
            // Arrange
            var utcNow = _time.GetUtcNow();
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("$2a$10$hash1abcdefghijklmnopqrstuvwxyz123456789012345678")
                .CreatedAt(utcNow)
                .Build();

        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(x => x.Secrets)
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                var secret = Assert.Single(loaded.Secrets);
                Assert.Equal(utcNow, secret.CreatedAt);
                Assert.Equal(utcNow.AddDays(7), secret.ExpiresAt);
                Assert.Null(secret.RevokedAt);
            }
        }
    
        [Fact]
        public async Task ClientSecret_PersistsAllFields_IncludingHashAndTimestamps()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("$2a$10$hashabcdefghijklmnopqrstuvwxyz12345678901234567")
                .Build();

            var secret = client.Secrets.Single();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(x => x.Secrets)
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                var loadedSecret = Assert.Single(loaded.Secrets);
                Assert.Equal(secret.Id, loadedSecret.Id);
                Assert.Equal(client.Id, loadedSecret.ClientId);
                Assert.Equal(secret.Hash, loadedSecret.Hash);
                Assert.Equal(secret.CreatedAt, loadedSecret.CreatedAt);
                Assert.Equal(secret.ExpiresAt, loadedSecret.ExpiresAt);
                Assert.Null(loadedSecret.RevokedAt);
            }
        }
    
        [Fact]
        public async Task RevokedSecret_PersistsRevocationTimestamp()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("$2a$10$hashabcdefghijklmnopqrstuvwxyz12345678901234567")
                .WithSecret("$2a$10$hashabcdefghijklmnopqrstuvwxyz12345678901234568")
                .Build();

            var secretToRevoke = client.Secrets.First();
            var utcNow = _time.GetUtcNow();
            var revocationTime = utcNow.AddDays(10);
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Revoke secret
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(x => x.Secrets)
                    .SingleAsync(x => x.Id == client.Id);
            
                loaded.RevokeSecret(secretToRevoke.Id, revocationTime);
                await ctx.SaveChangesAsync();
            }

            // Assert
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(x => x.Secrets)
                    .SingleAsync(x => x.Id == client.Id);
            
                var revokedSecret = loaded.Secrets.Single(s => s.Id == secretToRevoke.Id);
                Assert.NotNull(revokedSecret.RevokedAt);
                Assert.Equal(revocationTime, revokedSecret.RevokedAt);
                
                // Ensure other secret remains active
                var activeSecret = loaded.Secrets.Single(s => s.Id != secretToRevoke.Id);
                Assert.Null(activeSecret.RevokedAt);
            }
        }
    
        [Fact]
        public async Task ClientSecret_CanBeQueriedDirectly_WithoutLoadingClient()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("$2a$10$hashabcdefghijklmnopqrstuvwxyz12345678901234567")
                .Build();

            var secret = client.Secrets.Single();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Query secrets directly (not through Client)
            await using (var ctx = _fx.CreateContext())
            {
                var loadedSecret = await ctx.Set<ClientSecret>()
                    .SingleAsync(s => s.Id == secret.Id);

                // Assert
                Assert.Equal(secret.Id, loadedSecret.Id);
                Assert.Equal(client.Id, loadedSecret.ClientId);
                Assert.Equal(secret.Hash, loadedSecret.Hash);
            }
        }
        
        [Fact]
        public async Task DeletingClient_CascadesDeleteTo_AllSecrets()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("$2a$10$hashabcdefghijklmnopqrstuvwxyz12345678901234567")
                .WithSecret("$2a$10$hashabcdefghijklmnopqrstuvwxyz12345678901234568")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Delete client
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients.SingleAsync(x => x.Id == client.Id);
                ctx.Remove(loaded);
                await ctx.SaveChangesAsync();
            }

            // Assert - No secrets should remain
            await using (var ctx = _fx.CreateContext())
            {
                var remainingSecrets = await ctx.Set<ClientSecret>().CountAsync();
                Assert.Equal(0, remainingSecrets);
            }
        }
        
        [Fact]
        public async Task ClientWithMultipleSecrets_LoadsAllSecrets()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithSecret("$2a$10$hash1abcdefghijklmnopqrstuvwxyz12345678901234567")
                .WithSecret("$2a$10$hash2abcdefghijklmnopqrstuvwxyz12345678901234567")
                .WithSecret("$2a$10$hash3abcdefghijklmnopqrstuvwxyz12345678901234567")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .Include(x => x.Secrets)
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.Equal(3, loaded.Secrets.Count);
                Assert.All(loaded.Secrets, s => Assert.Equal(client.Id, s.ClientId));
            }
        }
    }

    
    
    public class Audiences(SqlServerFixture fx) : ClientPersistenceTests(fx)
    {
        [Fact]
        public async Task ClientWithMultipleAudiences_LoadsAllAudiences()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithAudience("https://api.example.com")
                .WithAudience("https://api2.example.com")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.Equal(2, loaded.Audiences.Count);
                Assert.Contains(loaded.Audiences, a => a.Value == "https://api.example.com");
                Assert.Contains(loaded.Audiences, a => a.Value == "https://api2.example.com");
            }
        }
        
        [Fact]
        public async Task ClientAudiences_AreAutomaticallyLoaded_AsOwnedEntities()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithAudience("https://api.example.com")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Load without explicit Include
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert - Owned entities should be loaded automatically
                Assert.NotEmpty(loaded.Audiences);
                Assert.Single(loaded.Audiences);
                Assert.Equal("https://api.example.com", loaded.Audiences.First().Value);
            }
        }
        
        [Fact]
        public async Task DeletingClient_AutomaticallyDeletes_AllAudiences()
        {
            // Arrange
            var client1 = new ClientBuilder()
                .WithName("client1")
                .WithAudience("https://api.example.com")
                .WithAudience("https://api2.example.com")
                .Build();
                
            var client2 = new ClientBuilder()
                .WithName("client2")
                .WithAudience("https://api3.example.com")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.AddRange(client1, client2);
                await ctx.SaveChangesAsync();
            }

            // Act - Delete first client
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients.SingleAsync(x => x.Id == client1.Id);
                ctx.Remove(loaded);
                await ctx.SaveChangesAsync();
            }

            // Assert - First client's audiences are gone, second client's remain
            await using (var ctx = _fx.CreateContext())
            {
                var remaining = await ctx.Clients.SingleAsync(x => x.Id == client2.Id);
                Assert.Single(remaining.Audiences);
                Assert.Equal("https://api3.example.com", remaining.Audiences.First().Value);
            }
        }
        
        [Fact]
        public async Task ClientCanBeQueried_ByAudienceValue()
        {
            // Arrange
            var client1 = new ClientBuilder()
                .WithName("client1")
                .WithAudience("https://api.example.com")
                .Build();
                
            var client2 = new ClientBuilder()
                .WithName("client2")
                .WithAudience("https://other.example.com")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.AddRange(client1, client2);
                await ctx.SaveChangesAsync();
            }

            // Act - Query for client by audience
            await using (var ctx = _fx.CreateContext())
            {
                var found = await ctx.Clients
                    .Where(c => c.Audiences.Any(a => a.Value == "https://api.example.com"))
                    .ToListAsync();

                // Assert
                Assert.Single(found);
                Assert.Equal(client1.Id, found[0].Id);
            }
        }
        
        [Fact]
        public async Task AudienceWithScopes_PersistsAllScopes()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithAudience("https://api.example.com", "read", "write", "delete")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                var audience = Assert.Single(loaded.Audiences);
                Assert.Equal(3, audience.Scopes.Count);
                Assert.Contains(audience.Scopes, s => s.Value == "read");
                Assert.Contains(audience.Scopes, s => s.Value == "write");
                Assert.Contains(audience.Scopes, s => s.Value == "delete");
            }
        }
        
        [Fact]
        public async Task MultipleAudiences_WithDifferentScopes_AllPersist()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithAudience("https://api.example.com", "read", "write")
                .WithAudience("https://api2.example.com", "admin")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.Equal(2, loaded.Audiences.Count);
                
                var audience1 = loaded.Audiences.Single(a => a.Value == "https://api.example.com");
                Assert.Equal(2, audience1.Scopes.Count);
                Assert.Contains(audience1.Scopes, s => s.Value == "read");
                Assert.Contains(audience1.Scopes, s => s.Value == "write");
                
                var audience2 = loaded.Audiences.Single(a => a.Value == "https://api2.example.com");
                Assert.Single(audience2.Scopes);
                Assert.Contains(audience2.Scopes, s => s.Value == "admin");
            }
        }
        
        [Fact]
        public async Task AudienceWithoutScopes_SavesSuccessfully()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .WithAudience("https://api.example.com")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                var audience = Assert.Single(loaded.Audiences);
                Assert.Empty(audience.Scopes);
            }
        }
        
        [Fact]
        public async Task ClientCanBeQueried_ByAudienceScope()
        {
            // Arrange
            var client1 = new ClientBuilder()
                .WithName("client1")
                .WithAudience("https://api.example.com", "read", "write")
                .Build();
                
            var client2 = new ClientBuilder()
                .WithName("client2")
                .WithAudience("https://api.example.com", "admin")
                .Build();
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.AddRange(client1, client2);
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = _fx.CreateContext())
            {
                var found = await ctx.Clients
                    .Where(c => c.Audiences.Any(a => 
                        a.Value == "https://api.example.com" && 
                        a.Scopes.Any(s => s.Value == "write")))
                    .ToListAsync();

                // Assert
                Assert.Single(found);
                Assert.Equal(client1.Id, found[0].Id);
            }
        }
    }
    

    public class Constraints(SqlServerFixture fx) : ClientPersistenceTests(fx)
    {
        [Fact]
        public async Task DuplicateClientName_ThrowsException()
        {
            // Arrange
            var client1 = new ClientBuilder().WithName("duplicate").Build();
            var client2 = new ClientBuilder().WithName("duplicate").Build();
        
            await using var ctx = _fx.CreateContext();
        
            ctx.Add(client1);
            await ctx.SaveChangesAsync();

            // Act & Assert
            ctx.Add(client2);
            await Assert.ThrowsAnyAsync<DbUpdateException>(
                () => ctx.SaveChangesAsync()
            );
        }
        
        [Fact]
        public async Task ClientName_IsCaseInsensitiveUnique()
        {
            // Arrange
            var client1 = new ClientBuilder().WithName("MyClient").Build();
            var client2 = new ClientBuilder().WithName("myclient").Build();
        
            await using var ctx = _fx.CreateContext();
        
            ctx.Add(client1);
            await ctx.SaveChangesAsync();

            // Act & Assert
            ctx.Add(client2);
            await Assert.ThrowsAnyAsync<DbUpdateException>(
                () => ctx.SaveChangesAsync()
            );
        }
    }
}