using OpenAuth.Application.Clients;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Test.Common.Fakes;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients;


[Collection("sqlserver")]
public class ClientServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    public ClientServiceTests(SqlServerFixture fx) => _fx = fx;    

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ClientService CreateSut()
    {
        var context = _fx.CreateContext();
        return new ClientService(new ClientRepository(context), new FakeClientSecretFactory(), new FakeSigningKeyFactory());
    }

    public class GetClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task GetByIdAsync_ReturnsClient_WhenExists()
        {
            var service = CreateSut();

            var created = await service.RegisterAsync("client");
            var fetched = await service.GetByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Equal(created.Name, fetched.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var service = CreateSut();

            var fetched = await service.GetByIdAsync(ClientId.New());
            Assert.Null(fetched);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsClient_WhenExists()
        {
            var service = CreateSut();

            const string name = "client";
            var created = await service.RegisterAsync(name);
            var fetched = await service.GetByNameAsync(name);

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Equal(created.Name, fetched.Name);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsNull_WhenNotFound()
        {
            var service = CreateSut();

            var fetched = await service.GetByNameAsync("missing-client");
            Assert.Null(fetched);
        }
    }

    public class RegisterClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RegisterAsync_CreatesAndPersistsClient()
        {
            var service = CreateSut();

            var created = await service.RegisterAsync("client");
            var fetched = await service.GetByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(created.Name, fetched.Name);
            Assert.Equal(created.Id, fetched.Id);
        }

        [Fact]
        public async Task RegisterAsync_AllowsMultipleClients()
        {
            var service = CreateSut();

            var client1 = await service.RegisterAsync("client-1");
            var client2 = await service.RegisterAsync("client-2");

            var fetched1 = await service.GetByIdAsync(client1.Id);
            var fetched2 = await service.GetByIdAsync(client2.Id);

            Assert.NotNull(fetched1);
            Assert.NotNull(fetched2);
            Assert.Equal(client1.Name, fetched1.Name);
            Assert.Equal(client2.Name, fetched2.Name);
        }
    }
    
    public class RenameClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RenameAsync_UpdatesName_WhenClientExists()
        {
            var service = CreateSut();

            const string newName = "new-client";
            const string oldName = "old-client";
            
            var client = await service.RegisterAsync(oldName);
            var updated = await service.RenameAsync(client.Id, newName);

            Assert.Equal(newName, updated.Name);

            var fetched = await service.GetByIdAsync(client.Id);

            Assert.NotNull(fetched);
            Assert.Equal(newName, fetched.Name);
        }

        [Fact]
        public async Task RenameAsync_Throws_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RenameAsync(ClientId.New(), "client"));
        }
    }

    public class DeleteClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task DeleteAsync_RemovesClient_WhenExists()
        {
            var service = CreateSut();

            var client = await service.RegisterAsync("client");
            
            var result = await service.DeleteAsync(client.Id);
            Assert.True(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            var result = await service.DeleteAsync(ClientId.New());
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_RemovesOnlyTargetClient()
        {
            var service = CreateSut();

            var client1 = await service.RegisterAsync("client-1");
            var client2 = await service.RegisterAsync("client-2");

            var result = await service.DeleteAsync(client1.Id);
            Assert.True(result);

            var stillThere = await service.GetByIdAsync(client2.Id);
            Assert.NotNull(stillThere);
            Assert.Equal(client2.Name, stillThere.Name);
        } 
    }

    public class ClientEnabled(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task EnableAsync_EnablesClient_WhenDisabled()
        {
            var service = CreateSut();

            var client = await service.RegisterAsync("client");
            await service.DisableAsync(client.Id);

            var updated = await service.EnableAsync(client.Id);

            Assert.True(updated.Enabled);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.True(fetched!.Enabled);
        }

        [Fact]
        public async Task DisableAsync_DisablesClient_WhenEnabled()
        {
            var service = CreateSut();

            var client = await service.RegisterAsync("client");
            var updated = await service.DisableAsync(client.Id);

            Assert.False(updated.Enabled);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.False(fetched!.Enabled);
        }

        [Fact]
        public async Task EnableAsync_Throws_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.EnableAsync(ClientId.New()));
        }

        [Fact]
        public async Task DisableAsync_Throws_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.DisableAsync(ClientId.New()));
        }
    }

    public class GrantScopes(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        private Scope Read { get; } = new Scope("read");
        private Scope Write { get; } = new Scope("write");
        
        [Fact]
        public async Task GrantScopesAsync_AddsScopes_WhenClientExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];
            client.TryAddAudience(audience);

            var updated = await service.GrantScopesAsync(client.Id, audience, scopes);

            Assert.Contains(updated.GetAudiences(), a => a == audience);
            Assert.Equal(updated.GetAllowedScopes(audience), scopes);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Contains(fetched.GetAudiences(), a => a == audience);
            Assert.Equal(fetched.GetAllowedScopes(audience), scopes);
        }

        [Fact]
        public async Task GrantScopesAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();

            var audience = new Audience("api");
            Scope[] scopes = [Read];

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.GrantScopesAsync(ClientId.New(), audience, scopes));
        }

        [Fact]
        public async Task GrantScopesAsync_Idempotent_WhenGrantingSameScopeTwice()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var audience = new Audience("api");
            Scope[] scopes = [Read];
            client.TryAddAudience(audience);

            await service.GrantScopesAsync(client.Id, audience, scopes);
            await service.GrantScopesAsync(client.Id, audience, scopes);
            
            var updated = await service.GrantScopesAsync(client.Id, audience, scopes);
            Assert.Single(updated.GetAudiences());
            Assert.Single(updated.GetAllowedScopes(audience));
        } 
    }


    public class RevokeScopes(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        private Scope Read { get; } = new Scope("read");
        private Scope Write { get; } = new Scope("write");
        
        [Fact]
        public async Task RevokeScopesAsync_RemovesScopes_WhenClientExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];

            client.TryAddAudience(audience);

            await service.GrantScopesAsync(client.Id, audience, scopes);
            Assert.Contains(client.GetAudiences(), a => a == audience);
            Assert.Equal(client.GetAllowedScopes(audience), scopes);

            var updated = await service.RevokeScopesAsync(client.Id, audience, scopes);

            Assert.DoesNotContain(updated.GetAudiences(), a => a == audience);
            Assert.Empty(updated.GetAllowedScopes(audience));

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.DoesNotContain(fetched.GetAudiences(), a => a == audience);
            Assert.Empty(fetched.GetAllowedScopes(audience));
        }

        [Fact]
        public async Task RevokeScopesAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();

            var audience = new Audience("api");
            Scope[] scopes = [Read];

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RevokeScopesAsync(ClientId.New(), audience, scopes));
        }

        [Fact]
        public async Task RevokeScopesAsync_DoesNothing_WhenScopeDoesNotExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var audience = new Audience("api");
            Scope[] scopes = [Read];
            client.TryAddAudience(audience);

            await service.GrantScopesAsync(client.Id, audience, scopes);

            var updated = await service.RevokeScopesAsync(client.Id, audience, [Write]);
            Assert.Contains(updated.GetAudiences(), a => a == audience);
            Assert.Equal(scopes, updated.GetAllowedScopes(audience));
        } 
    }

    public class RevokeAudience(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        private Scope Read { get; } = new Scope("read");
        private Scope Write { get; } = new Scope("write");
        
        [Fact]
        public async Task RemoveAudienceAsync_RemovesAudience_WhenClientExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];
            client.TryAddAudience(audience);

            await service.GrantScopesAsync(client.Id, audience, scopes);

            var updated = await service.RemoveAudienceAsync(client.Id, audience);
            Assert.Empty(updated.GetAudiences());

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.GetAudiences());
            Assert.Empty(fetched.GetAllowedScopes(audience));
        }

        [Fact]
        public async Task RemoveAudienceAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RemoveAudienceAsync(ClientId.New(), new Audience("api")));
        }

        [Fact]
        public async Task RemoveAudienceAsync_DoesNothing_WhenAudienceDoesNotExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var updated = await service.RemoveAudienceAsync(client.Id, new Audience("missing"));
            Assert.Empty(updated.GetAudiences());

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.GetAudiences());
        }
    }

    public class AddSecret(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task AddSecretAsync_AddsSecretAndReturnsPlainText()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var creationResult = await service.AddSecretAsync(client.Id);
            Assert.Equal("plain-secret-1", creationResult.Plain); // from FakeClientSecretFactory

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Single(fetched.Secrets);
        }

        [Fact]
        public async Task AddSecretAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.AddSecretAsync(ClientId.New()));
        }

        [Fact]
        public async Task AddSecretAsync_SetsExpiration_WhenProvided()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var expiresAt = DateTime.UtcNow.AddDays(7);
            await service.AddSecretAsync(client.Id, expiresAt);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Single(fetched.Secrets);
            
            var secret = fetched.Secrets.First();
            Assert.True(secret.IsActive());
            Assert.Equal(expiresAt, secret.ExpiresAt);
        } 
    }

    public class RevokeSecret(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RevokeSecretAsync_RevokesSecret_NotRemoves_WhenExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            await service.AddSecretAsync(client.Id);
            var updated = await service.GetByIdAsync(client.Id);
            var secret = updated!.Secrets.First();

            var result = await service.RevokeSecretAsync(secret.Id);
            Assert.True(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);

            var revoked = fetched.Secrets.First();
            Assert.False(revoked.IsActive());
            Assert.NotNull(revoked.RevokedAt);
            Assert.Contains(fetched.Secrets, s => s.Id == revoked.Id);
        }

        [Fact]
        public async Task RevokeSecretAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();

            var result = await service.RevokeSecretAsync(new SecretId(Guid.NewGuid()));
            Assert.False(result);
        }

        [Fact]
        public async Task RevokeSecretAsync_ReturnsFalse_WhenSecretNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var result = await service.RevokeSecretAsync(new SecretId(Guid.NewGuid()));
            Assert.False(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.Secrets);
        }
    }

    public class RemoveSecret(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RemoveSecretAsync_RemovesSecret_WhenExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            await service.AddSecretAsync(client.Id);
            var updated= await service.GetByIdAsync(client.Id);
            var secret = updated!.Secrets.Single();

            var result = await service.RemoveSecretAsync(secret.Id);
            Assert.True(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.Secrets);
        }

        [Fact]
        public async Task RemoveSecretAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();

            var result = await service.RemoveSecretAsync(new SecretId(Guid.NewGuid()));
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveSecretAsync_ReturnsFalse_WhenSecretNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var result = await service.RemoveSecretAsync(new SecretId(Guid.NewGuid()));
            Assert.False(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.Secrets);
        } 
    }

    public class AddSigningKey(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task AddSigningKeyAsync_AddsKeyToClient()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var key = await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Single(fetched.SigningKeys);
            Assert.Contains(fetched.SigningKeys, k => k == key);
        }

        [Fact]
        public async Task AddSigningKeyAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.AddSigningKeyAsync(ClientId.New(), SigningAlgorithm.Rsa));
        }

        [Fact]
        public async Task AddSigningKeyAsync_SetsExpiration_WhenProvided()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var expiresAt = DateTime.UtcNow.AddDays(30);
            var key = await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa, expiresAt);
            Assert.Equal(expiresAt, key.ExpiresAt);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Single(fetched.SigningKeys);
            Assert.Equal(expiresAt, fetched.SigningKeys.Single().ExpiresAt);
        } 
    }

    public class RevokeSigningKey(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RevokeSigningKeyAsync_RevokesKey_WhenExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var key = await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa);

            var result = await service.RevokeSigningKeyAsync(key.KeyId);
            Assert.True(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);

            var signingKey = fetched.SigningKeys.First();
            Assert.False(signingKey.IsActive());
            Assert.NotNull(signingKey.RevokedAt);
            Assert.Contains(fetched.SigningKeys, k => k.KeyId == key.KeyId);
        }

        [Fact]
        public async Task RevokeSigningKeyAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();

            var result = await service.RevokeSigningKeyAsync(new SigningKeyId(Guid.NewGuid()));
            Assert.False(result);
        }

        [Fact]
        public async Task RevokeSigningKeyAsync_ReturnsFalse_WhenKeyNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var result = await service.RevokeSigningKeyAsync(new SigningKeyId(Guid.NewGuid()));
            Assert.False(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.SigningKeys);
        } 
    }

    public class RemoveSigningKey(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RemoveSigningKeyAsync_RemovesKey_WhenExists()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var key = await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa);

            var result = await service.RemoveSigningKeyAsync(key.KeyId);
            Assert.True(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.SigningKeys);
        }

        [Fact]
        public async Task RemoveSigningKeyAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();

            var result = await service.RemoveSigningKeyAsync(new SigningKeyId(Guid.NewGuid()));
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveSigningKeyAsync_ReturnsFalse_WhenKeyNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var result = await service.RemoveSigningKeyAsync( new SigningKeyId(Guid.NewGuid()));

            Assert.False(result);

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.SigningKeys);
        }
    }
}