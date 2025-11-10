using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients.Application;

public class ClientServiceTests
{
    private FakeClientRepository _repo;
    private IClientFactory _clientFactory;
    
    private TimeProvider _time;
    private IClientService _sut;

    public ClientServiceTests()
    {
        _time = new FakeTimeProvider();
        
        _repo = new FakeClientRepository();
        _clientFactory = new ClientFactory(_time);

        _sut = new ClientService(_repo, _clientFactory, _time);
    }

    
    public class Registration : ClientServiceTests
    {
        [Fact]
        public async Task RegisterAsync_AddsClient()
        {
            var service = _sut;
            await service.RegisterAsync(new ClientName("test"));

            Assert.NotEmpty(_repo.Clients);
            Assert.True(_repo.Saved);
        }

        [Fact]
        public async Task RenameAsync_ChangesName()
        {
            var service = _sut;
            
            var expectedName = new ClientName("cool client");
            var client = await service.RegisterAsync(new ClientName("test"));

            var renamed = await service.RenameAsync(client.Id, expectedName);
            Assert.Equal(expectedName, renamed.Name);
        }

        [Fact]
        public async Task RenameAsync_Throws_WhenClientNotFound()
        {
            var service = _sut;
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RenameAsync(ClientId.New(), new ClientName("test")));
        }

        [Fact]
        public async Task DeleteAsync_RemovesClient()
        {
            var service = _sut;
            var client = await service.RegisterAsync(new ClientName("test"));

            await service.DeleteAsync(client.Id);
            
            Assert.Empty(_repo.Clients);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsException_WhenClientNotFound()
        {
            var service = _sut;
            
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => service.DeleteAsync(ClientId.New()));
        }
    }

    public class SetAudiences : ClientServiceTests
    {

        [Fact]
        public async Task CallsSetAudiences_AndSavesChanges()
        {
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            var api = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            var web = new Audience(AudienceName.Create("web"), ScopeCollection.Parse("read"));
            var audiences = new[] { api, web };
            
            await _sut.SetAudiencesAsync(client.Id, audiences);

            Assert.Equal(2, client.AllowedAudiences.Count);
            Assert.Contains(client.AllowedAudiences, a => a == api);
            Assert.Contains(client.AllowedAudiences, a => a == web);
        }

        [Fact]
        public async Task WhenDuplicateAudiences_ThrowsException()
        {
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            var audiences = new[]
            {
                new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write")),
                new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read"))
            };
            
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.SetAudiencesAsync(client.Id, audiences));
        }

        [Fact]
        public async Task WhenEmptyAudiences_ThrowsException()
        {
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.SetAudiencesAsync(client.Id, []));           
        }
        
        [Fact]
        public async Task Throws_WhenClientNotFound()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.SetAudiencesAsync(ClientId.New(), [], CancellationToken.None));
        }
    }

    public class AddAudience : ClientServiceTests
    {
        [Fact]
        public async Task WhenValid_AppendsWithoutRemovingExistingAudiences()
        {
            var client = new ClientBuilder()
                .WithAudience("existing", "read")
                .Build();
            _repo.Add(client);
            
            var audience = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            await _sut.AddAudienceAsync(client.Id, audience);
            
            Assert.Contains(client.AllowedAudiences, a => a == audience);
            Assert.Contains(client.AllowedAudiences, a => a.Name == AudienceName.Create("existing"));
        }
        
        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            var audience = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.AddAudienceAsync(ClientId.New(), audience));
        }

        [Fact]
        public async Task WhenAudienceExists_ThrowsException()
        {
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            var audience = new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
            await _sut.AddAudienceAsync(client.Id, audience);
            
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.AddAudienceAsync(client.Id, audience));
        }
    }

    public class RemoveAudienceAsync : ClientServiceTests
    {
        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.RemoveAudienceAsync(ClientId.New(), AudienceName.Create("non-existent")));
        }

        [Fact]
        public async Task WhenAudienceNotFound_DoesNothing()
        {
            var client = new ClientBuilder().Build();
            _repo.Add(client);

            var expected = client.AllowedAudiences.ToArray();
            await _sut.RemoveAudienceAsync(client.Id, AudienceName.Create("non-existent"));
            
            Assert.Equal(expected, client.AllowedAudiences);
        }

        [Fact]
        public async Task WhenValid_RemovesAudience()
        {
            var client = new ClientBuilder().Build();
            _repo.Add(client);
            
            var audience = new Audience(AudienceName.Create("unique"), ScopeCollection.Parse("read write"));
            await _sut.AddAudienceAsync(client.Id, audience);
            
            await _sut.RemoveAudienceAsync(client.Id, audience.Name);
            
            Assert.DoesNotContain(client.AllowedAudiences, a => a == audience);
        }
    }
    
    public class EnableDisable : ClientServiceTests
    {
        [Fact]
        public async Task EnableDisable_TogglesEnabled()
        {
            var service = _sut;
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
            var service = _sut;
            
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.EnableAsync(ClientId.New()));
        }

        [Fact]
        public async Task DisableAsync_Throws_WhenClientNotFound()
        {
            var service = _sut;
            
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DisableAsync(ClientId.New()));
        }
    }
}