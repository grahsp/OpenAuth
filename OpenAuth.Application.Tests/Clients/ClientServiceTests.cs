using OpenAuth.Application.Clients;
using OpenAuth.Application.Tests.Stubs;
using OpenAuth.Domain.Abstractions;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Tests.Clients;

public class ClientServiceTests
{
    private readonly FakeClientRepository _repo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly IClientSecretFactory _factory = new FakeClientSecretFactory();

    private ClientService CreateSut() => new(_repo, _uow, _factory);

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

            Assert.NotNull(await _repo.GetByIdAsync(client.Id));
            Assert.True(_uow.Saved);
        }

        [Fact]
        public async Task RenameAsync_ChangesName()
        {
            const string updatedClientName = "cool client";
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var renamed = await service.RenameAsync(client.Id, updatedClientName);

            Assert.Equal(updatedClientName, renamed.Name);
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

            Assert.True(result);
            Assert.Null(await _repo.GetByIdAsync(client.Id));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();
            var result = await service.DeleteAsync(ClientId.New());
            Assert.False(result);
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
            Assert.False(disabled.Enabled);

            var enabled = await service.EnableAsync(client.Id);
            Assert.True(enabled.Enabled);
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
            Assert.Empty(afterRemove.GetAudiences());
        }
    }

    
    public class Secrets : ClientServiceTests
    {
        [Fact]
        public async Task AddSecretAsync_AddsSecret()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var plain = await service.AddSecretAsync(client.Id);

            var updated = await _repo.GetByIdAsync(client.Id);
            Assert.Single(updated!.Secrets);
            Assert.Equal("plain-secret-1", plain);
        }

        [Fact]
        public async Task AddSecretAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AddSecretAsync(ClientId.New()));
        }

        [Fact]
        public async Task RevokeSecretAsync_RevokesSecret()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");
            _ = await service.AddSecretAsync(client.Id);
            var secret = client.Secrets.First();

            var result = await service.RevokeSecretAsync(client.Id, secret.Id);

            Assert.True(result);
            Assert.True(secret.RevokedAt.HasValue);
        }

        [Fact]
        public async Task RevokeSecretAsync_ReturnsFalse_WhenClientNotFound()
        {
            var service = CreateSut();
            var result = await service.RevokeSecretAsync(ClientId.New(), SecretId.New());
            Assert.False(result);
        }

        [Fact]
        public async Task RevokeSecretAsync_ReturnsTrue_EvenIfSecretNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync("client");

            var result = await service.RevokeSecretAsync(client.Id, SecretId.New());

            Assert.True(result);
        }
    }
}