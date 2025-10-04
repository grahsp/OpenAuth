using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Clients;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients;

public class ClientServiceTests
{
    private FakeClientRepository _repo;
    private IClientFactory _clientFactory;
    private TimeProvider _time;

    private readonly Scope _read = new("read");
    private readonly Scope _write = new("write");

    public ClientServiceTests()
    {
        _repo = new FakeClientRepository();
        _time = new FakeTimeProvider();
        _clientFactory = new ClientFactory(_time);
    }

    private ClientService CreateSut()
        => new(_repo, _clientFactory, _time);

    
    public class Registration : ClientServiceTests
    {
        [Fact]
        public async Task RegisterAsync_AddsClient()
        {
            var service = CreateSut();
            await service.RegisterAsync(new ClientName("test"));

            Assert.NotEmpty(_repo.Clients);
            Assert.True(_repo.Saved);
        }

        [Fact]
        public async Task RenameAsync_ChangesName()
        {
            var service = CreateSut();
            
            var expectedName = new ClientName("cool client");
            var client = await service.RegisterAsync(new ClientName("test"));

            var renamed = await service.RenameAsync(client.Id, expectedName);
            Assert.Equal(expectedName, renamed.Name);
        }

        [Fact]
        public async Task RenameAsync_Throws_WhenClientNotFound()
        {
            var service = CreateSut();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RenameAsync(ClientId.New(), new ClientName("test")));
        }

        [Fact]
        public async Task DeleteAsync_RemovesClient()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync(new ClientName("test"));

            await service.DeleteAsync(client.Id);
            
            Assert.Empty(_repo.Clients);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsException_WhenClientNotFound()
        {
            var service = CreateSut();
            
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => service.DeleteAsync(ClientId.New()));
        }
    }

    
    public class EnableDisable : ClientServiceTests
    {
        [Fact]
        public async Task EnableDisable_TogglesEnabled()
        {
            var service = CreateSut();
            var clientInfo = await service.RegisterAsync(new ClientName("test"));
            var client = _repo.Clients.Single();

            await service.DisableAsync(clientInfo.Id);
            Assert.False(client.Enabled);

            await service.EnableAsync(client.Id);
            Assert.True(client.Enabled);
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
            var clientInfo = await service.RegisterAsync(new ClientName("test"));
            var client = _repo.Clients.Single();
            
            var aud = new Audience("api");
            client.TryAddAudience(aud, _time.GetUtcNow());

            var afterGrant = await service.GrantScopesAsync(clientInfo.Id, aud, [_read, _write]);
            Assert.Contains(_read, afterGrant.GetAllowedScopes(aud));
            Assert.Contains(_write, afterGrant.GetAllowedScopes(aud));

            var afterRevoke = await service.RevokeScopesAsync(clientInfo.Id, aud, [_read]);
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
            var clientInfo = await service.RegisterAsync(new ClientName("test"));
            var client = _repo.Clients.Single();
            
            var aud = new Audience("api");
            client.TryAddAudience(aud, _time.GetUtcNow());
            
            await service.GrantScopesAsync(clientInfo.Id, aud, [_read]);

            var afterRemove = await service.TryRemoveAudienceAsync(clientInfo.Id, aud);
            Assert.NotNull(afterRemove);
            Assert.Empty(afterRemove.Audiences);
        }
    }

}