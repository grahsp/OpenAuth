using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Integration.Infrastructure;

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
                    .SingleAsync(x => x.Id == client.Id);

                // Assert
                Assert.NotNull(loaded);
                Assert.Equal(client.Name, loaded.Name);
                Assert.Empty(loaded.Secrets);
                Assert.Empty(loaded.AllowedAudiences);
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
                .WithGrantType()
                .WithGrantType(GrantTypes.AuthorizationCode)
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
                Assert.Contains(loaded.AllowedGrantTypes, g => g == GrantType.ClientCredentials);
            }
        }
    
        [Fact]
        public async Task EmptyCollection_SerializesCorrectly()
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
                Assert.NotNull(loaded.AllowedGrantTypes);
                Assert.Empty(loaded.AllowedGrantTypes);
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

                loaded.AddGrantType(GrantType.AuthorizationCode, _time.GetUtcNow());
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
            
                loaded.AddRedirectUri(RedirectUri.Create("https://example.com/callback"), _time.GetUtcNow());
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

            var apiAudienceName = new AudienceName("api");
            var webAudienceName = new AudienceName("web");
            
            client.AddAudience(apiAudienceName, _time.GetUtcNow());
            client.AddAudience(webAudienceName, _time.GetUtcNow());
        
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
                Assert.Equal(2, loaded.AllowedAudiences.Count);
                Assert.Contains(loaded.AllowedAudiences, a => a.Name == apiAudienceName);
                Assert.Contains(loaded.AllowedAudiences, a => a.Name == webAudienceName);
            }
        }
        
        [Fact]
        public async Task ClientAudiences_AreAutomaticallyLoaded_AsOwnedEntities()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();
            
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());
        
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

                Assert.Single(loaded.AllowedAudiences);
                Assert.Equal(audienceName, loaded.AllowedAudiences.First().Name);
            }
        }
        
        [Fact]
        public async Task DeletingClient_AutomaticallyDeletes_AllAudiences()
        {
            // Arrange
            var client1 = new ClientBuilder()
                .WithName("client1")
                .Build();
            
            client1.AddAudience(new AudienceName("api"), _time.GetUtcNow());
            client1.AddAudience(new AudienceName("web"), _time.GetUtcNow());
                
            var client2 = new ClientBuilder()
                .WithName("client2")
                .Build();
            
            var audienceName = new AudienceName("api");
            client2.AddAudience(audienceName, _time.GetUtcNow());
        
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
                Assert.Single(remaining.AllowedAudiences);
                Assert.Equal(audienceName, remaining.AllowedAudiences.First().Name);
            }
        }
        
        [Fact]
        public async Task ClientCanBeQueried_ByAudienceValue()
        {
            // Arrange
            var client1 = new ClientBuilder()
                .WithName("client1")
                .Build();
            
            var audienceName = new AudienceName("api");
            client1.AddAudience(audienceName, _time.GetUtcNow());
                
            var client2 = new ClientBuilder()
                .WithName("client2")
                .Build();
            
            client2.AddAudience(new AudienceName("web"), _time.GetUtcNow());
        
            await using (var ctx = _fx.CreateContext())
            {
                ctx.AddRange(client1, client2);
                await ctx.SaveChangesAsync();
            }

            // Act
            await using (var ctx = _fx.CreateContext())
            {
                var found = await ctx.Clients
                    .Where(c => c.AllowedAudiences.Any(a => a.Name == audienceName))
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
                .Build();
            
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, [new Scope("read"), new Scope("write")], _time.GetUtcNow());
        
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
                var audience = Assert.Single(loaded.AllowedAudiences);
                Assert.Equal(2, audience.AllowedScopes.Count);
                Assert.Contains(audience.AllowedScopes, s => s.Value == "read");
                Assert.Contains(audience.AllowedScopes, s => s.Value == "write");
            }
        }
        
        [Fact]
        public async Task MultipleAudiences_WithDifferentScopes_AllPersist()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();
            
            var apiAudienceName = new AudienceName("api");
            client.AddAudience(apiAudienceName, _time.GetUtcNow());
            client.GrantScopes(apiAudienceName, [new Scope("read"), new Scope("write")], _time.GetUtcNow());
            
            var webAudienceName = new AudienceName("web");
            client.AddAudience(webAudienceName, _time.GetUtcNow());
            client.GrantScopes(webAudienceName, [new Scope("admin")], _time.GetUtcNow());
            
        
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
                Assert.Equal(2, loaded.AllowedAudiences.Count);
                
                var audience1 = loaded.AllowedAudiences.Single(a => a.Name == apiAudienceName);
                Assert.Equal(2, audience1.AllowedScopes.Count);
                Assert.Contains(audience1.AllowedScopes, s => s.Value == "read");
                Assert.Contains(audience1.AllowedScopes, s => s.Value == "write");
                
                var audience2 = loaded.AllowedAudiences.Single(a => a.Name == webAudienceName);
                Assert.Single(audience2.AllowedScopes);
                Assert.Contains(audience2.AllowedScopes, s => s.Value == "admin");
            }
        }
        
        [Fact]
        public async Task AudienceWithoutScopes_SavesSuccessfully()
        {
            // Arrange
            var client = new ClientBuilder()
                .WithName("test-client")
                .Build();
            
            client.AddAudience(new AudienceName("api"), _time.GetUtcNow());
        
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
                var audience = Assert.Single(loaded.AllowedAudiences);
                Assert.Empty(audience.AllowedScopes);
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