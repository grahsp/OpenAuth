using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Infrastructure.Fixtures;

namespace OpenAuth.Test.Integration.Clients.Infrastructure;

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
                    .Include(c => c.Secrets)
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.NotNull(loaded);
                Assert.Equal(client.Name, loaded.Name);
                Assert.Empty(loaded.Secrets);
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


    public class AllowedGrantTypes(SqlServerFixture fx) : ClientPersistenceTests(fx)
    {
        [Fact]
        public async Task SerializesAndDeserializesCorrectly()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.Spa)
                .WithGrantType(GrantTypes.AuthorizationCode)
                .WithGrantType(GrantTypes.RefreshToken)
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
                Assert.Equal(2, loaded.AllowedGrantTypes.Count);
                Assert.Contains(loaded.AllowedGrantTypes, g => g == GrantType.AuthorizationCode);
                Assert.Contains(loaded.AllowedGrantTypes, g => g == GrantType.RefreshToken);
            }
        }
    
        [Fact]
        public async Task Mutations_AreTrackedByEF()
        {
            // Arrange
            var client = new ClientBuilder()
                .Build();
    
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Add grant type
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);

                loaded.SetGrantTypes([GrantType.RefreshToken], _time.GetUtcNow());
                await ctx.SaveChangesAsync();
            }

            // Assert
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);
            
                var grantType = Assert.Single(loaded.AllowedGrantTypes);
                Assert.Contains(loaded.AllowedGrantTypes, g => g == grantType);
            }
        }
    }


    public class RedirectUris(SqlServerFixture fx) : ClientPersistenceTests(fx)
    {
        [Fact]
        public async Task SerializesAndDeserializesCorrectly()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithRedirectUri("https://example.com/callback")
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
                var actual = Assert.Single(loaded.RedirectUris);
                Assert.Equal(client.RedirectUris.Single(), actual);
            }
        }
    
        [Fact]
        public async Task EmptyCollection_SerializesCorrectly()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                Assert.NotNull(loaded.RedirectUris);
                Assert.Empty(loaded.RedirectUris);
            }
        }
    
        [Fact]
        public async Task Mutations_AreTrackedByEF()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.Spa)
                .Build();
    
            await using (var ctx = _fx.CreateContext())
            {
                ctx.Add(client);
                await ctx.SaveChangesAsync();
            }

            // Act - Add redirect URIs
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);
            
                loaded.SetRedirectUris([RedirectUri.Create("https://example.com/callback")], _time.GetUtcNow());
                await ctx.SaveChangesAsync();
            }

            // Assert
            await using (var ctx = _fx.CreateContext())
            {
                var loaded = await ctx.Clients
                    .SingleAsync(x => x.Id == client.Id);
            
                var actual = Assert.Single(loaded.RedirectUris);
                Assert.Equal(loaded.RedirectUris.Single(), actual);
            }
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
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                var loadedSecret = await ctx.Set<Secret>()
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
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                var remainingSecrets = await ctx.Set<Secret>().CountAsync();
                Assert.Equal(0, remainingSecrets);
            }
        }
        
        [Fact]
        public async Task ClientWithMultipleSecrets_LoadsAllSecrets()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.M2M)
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
                .Build();

            var api = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            var web = new Audience(AudienceName.Create("web"), ScopeCollection.Parse("read write"));
            var audiences = new[] { api, web };
            client.SetAudiences(audiences, _time.GetUtcNow());
        
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
                Assert.Contains(loaded.AllowedAudiences, a => a.Name == api.Name);
                Assert.Contains(loaded.AllowedAudiences, a => a.Name == web.Name);
            }
        }
        
        [Fact]
        public async Task ClientAudiences_AreAutomaticallyLoaded_AsOwnedEntities()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();
            
            var api = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            var audiences = new[] { api };
            client.SetAudiences(audiences, _time.GetUtcNow());
        
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

                Assert.Contains(loaded.AllowedAudiences, a => a.Name == api.Name);
            }
        }
        
        [Fact]
        public async Task DeletingClient_AutomaticallyDeletes_AllAudiences()
        {
            // Arrange
            var client1 = new ClientBuilder()
                .WithName("client1")
                .Build();
            
            var api = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            client1.SetAudiences([api], _time.GetUtcNow());
                
            var client2 = new ClientBuilder()
                .WithName("client2")
                .Build();
            
            var web = new Audience(AudienceName.Create("web"), ScopeCollection.Parse("read write"));
            client2.SetAudiences([web], _time.GetUtcNow());
        
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
                Assert.Contains(remaining.AllowedAudiences, a => a.Name == web.Name);
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