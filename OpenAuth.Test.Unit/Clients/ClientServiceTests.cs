using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Clients;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients;

public class ClientServiceTests
{
    private FakeClientRepository _repo;
    private IClientFactory _clientFactory;
    private TimeProvider _time;

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
}