using System.Collections;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Security.Keys;
using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients;

public class ClientServiceTests
{
    private readonly FakeClientRepository _repo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly IClientSecretFactory _secretFactory = new FakeClientSecretFactory();
    private readonly ISigningKeyFactory _keyFactory = new FakeSigningKeyFactory();

    private ClientService CreateSut() => new(_repo, _uow, _secretFactory, _keyFactory);

    private readonly Scope _read = new("read");
    private readonly Scope _write = new("write");

    
    public class Queries : ClientServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_ReturnsClient_WhenExists()
        {
            var service = CreateSut();
            var created = await service.RegisterAsync("client");

            var result = await service.GetByIdAsync(created.Id);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            var service = CreateSut();
            var result = await service.GetByIdAsync(ClientId.New());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsClient_WhenExists()
        {
            const string clientName = "client";
            var service = CreateSut();
            var created = await service.RegisterAsync(clientName);

            var result = await service.GetByNameAsync(clientName);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result.Id);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsNull_WhenNotExists()
        {
            var service = CreateSut();
            var result = await service.GetByNameAsync("missing");
            Assert.Null(result);
        }
    }

    
    public class Registration : ClientServiceTests
    {
        [Fact]
        public async Task RegisterAsync_AddsClient()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            Assert.NotNull(await service.GetByIdAsync(client.Id));
            Assert.True(_uow.Saved);
        }

        [Fact]
        public async Task RenameAsync_ChangesName()
        {
            const string updatedClientName = "cool client";
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var renamed = await service.RenameAsync(client.Id, updatedClientName);

            Assert.Equal((string?)updatedClientName, (string?)renamed.Name);
        }

        [Fact]
        public async Task RenameAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RenameAsync(ClientId.New(), "client"));
        }

        [Fact]
        public async Task DeleteAsync_RemovesClient()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var result = await service.DeleteAsync(client.Id);

            Assert.True((bool)result);
            Assert.Null(await service.GetByIdAsync(client.Id));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();
            var result = await service.DeleteAsync(ClientId.New());
            Assert.False((bool)result);
        }
    }

    
    public class EnableDisable : ClientServiceTests
    {
        [Fact]
        public async Task EnableDisable_TogglesEnabled()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var disabled = await service.DisableAsync(client.Id);
            Assert.False((bool)disabled.Enabled);

            var enabled = await service.EnableAsync(client.Id);
            Assert.True((bool)enabled.Enabled);
        }

        [Fact]
        public async Task EnableAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.EnableAsync(ClientId.New()));
        }

        [Fact]
        public async Task DisableAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DisableAsync(ClientId.New()));
        }
    }

    
    public class Scopes : ClientServiceTests
    {
        [Fact]
        public async Task GrantAndRevokeScopes_ModifiesClientScopes()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            var aud = new Audience("api");

            var afterGrant = await service.GrantScopesAsync(client.Id, aud, [_read, _write]);
            Assert.Contains(_read, afterGrant.GetAllowedScopes(aud));
            Assert.Contains(_write, afterGrant.GetAllowedScopes(aud));

            var afterRevoke = await service.RevokeScopesAsync(client.Id, aud, [_read]);
            Assert.DoesNotContain(_read, afterRevoke.GetAllowedScopes(aud));
            Assert.Contains(_write, afterRevoke.GetAllowedScopes(aud));
        }

        [Fact]
        public async Task GrantScopes_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            var aud = new Audience("api");
            
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GrantScopesAsync(ClientId.New(), aud, [_write]));
        }

        [Fact]
        public async Task RemoveAudience_RemovesAllScopes()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            var aud = new Audience("api");
            
            await service.GrantScopesAsync(client.Id, aud, [_read]);

            var afterRemove = await service.RemoveAudienceAsync(client.Id, aud);
            Assert.Empty((IEnumerable)afterRemove.GetAudiences());
        }
    }

    
    public class Secrets : ClientServiceTests
    {
        [Fact]
        public async Task AddSecretAsync_PersistsSecretWithProperties()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var plain = await service.AddSecretAsync(client.Id);

            var updated = await service.GetByIdAsync(client.Id);
            Assert.Single(updated!.Secrets);
            Assert.Equal((string?)"plain-secret-1", (string?)plain);
        }

        [Fact]
        public async Task AddSecretAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddSecretAsync(ClientId.New()));
        }

        [Fact]
        public async Task RevokeSecretAsync_RevokesSecret_WhenClientAndSecretExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            _ = await service.AddSecretAsync(client.Id);
            var secret = client.Secrets.First();

            var result = await service.RevokeSecretAsync(client.Id, secret.Id);

            Assert.True((bool)result);
            Assert.True((bool)secret.RevokedAt.HasValue);
        }

        [Fact]
        public async Task RevokeSecretAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();
            var result = await service.RevokeSecretAsync(ClientId.New(), SecretId.New());
            Assert.False((bool)result);
        }
        
        [Fact]
        public async Task RemoveSecretAsync_RemovesSecret_WhenClientAndSecretExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            
            await service.AddSecretAsync(client.Id);
            var secret = client.Secrets.First();

            var result = await service.RemoveSecretAsync(client.Id, secret.Id);
            var updated = await _repo.GetByIdAsync(client.Id);

            Assert.True((bool)result);
            Assert.Empty(updated!.Secrets);
        }

        [Fact]
        public async Task RemoveSecretAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();

            var result = await service.RemoveSecretAsync(ClientId.New(), SecretId.New());
            Assert.False((bool)result);
        }
    }

    public class SigningKeys : ClientServiceTests
    {
        [Fact]
        public async Task AddSigningKeyAsync_PersistsKeyWithProperties()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            var expiresAt = DateTime.UtcNow.AddDays(30);

            await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa, expiresAt);

            var updated = await _repo.GetByIdAsync(client.Id);
            var key = updated!.SigningKeys.Single();

            Assert.Equal(SigningAlgorithm.Rsa, key.Algorithm);
            Assert.Equal(expiresAt, key.ExpiresAt);
        }

        [Fact]
        public async Task AddSigningKeyAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.AddSigningKeyAsync(new ClientId(Guid.NewGuid()), SigningAlgorithm.Rsa));
        }
        
        [Fact]
        public async Task RevokeSigningKeyAsync_RevokesKey_WhenClientAndKeyExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var expiresAt = DateTime.UtcNow.AddDays(30);
            var key = await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa, expiresAt);

            var result = await service.RevokeSigningKeyAsync(client.Id, key.KeyId);

            var updated = await _repo.GetByIdAsync(client.Id);
            var revokedKey = updated!.SigningKeys.Single();
        
            Assert.True((bool)result);
            Assert.False(revokedKey.IsActive());
            Assert.NotNull(revokedKey.RevokedAt);
        }

        [Fact]
        public async Task RevokeSigningKeyAsync_ReturnsFalse_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            var result = await service.RevokeSigningKeyAsync(new ClientId(Guid.NewGuid()), SigningKeyId.New());

            Assert.False((bool)result);
        }
        
        [Fact]
        public async Task RemoveSigningKeyAsync_RemovesKey_WhenClientAndKeyExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            var key = await service.AddSigningKeyAsync(client.Id, SigningAlgorithm.Rsa);

            var result = await service.RemoveSigningKeyAsync(client.Id, key.KeyId);

            var updated = await _repo.GetByIdAsync(client.Id);

            Assert.True((bool)result);
            Assert.Empty(updated!.SigningKeys);
        }

        [Fact]
        public async Task RemoveSigningKeyAsync_ReturnsFalse_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            var result = await service.RemoveSigningKeyAsync(new ClientId(Guid.NewGuid()), SigningKeyId.New());
            Assert.False((bool)result);
        }
    }
}