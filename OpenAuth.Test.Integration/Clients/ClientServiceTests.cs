using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Clients;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients;


[Collection("sqlserver")]
public class ClientServiceTests : IAsyncLifetime
{
    private SqlServerFixture _fx;
    private AppDbContext _context;
    private IClientRepository _repo;
    private IClientFactory _factory;
    private TimeProvider _time;

    public ClientServiceTests(SqlServerFixture fx)
    {
        _fx = fx;
        _context = _fx.CreateContext();
        _time = new FakeTimeProvider();
        _repo = new ClientRepository(_context);
        _factory = new ClientFactory(_time);
    }
    

    public async Task InitializeAsync()
        => await _fx.ResetAsync();
    public Task DisposeAsync()
        => Task.CompletedTask;

    private ClientService CreateSut()
    {
        return new ClientService(
            _repo,
            _factory,
            _time);
    }

    public class RegisterClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RegisterAsync_CreatesAndPersistsClient()
        {
            var service = CreateSut();

            var created = await service.RegisterAsync(new ClientName("client"));
            var fetched = await _context.Clients.SingleAsync(c => c.Id == created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(created.Name, fetched.Name);
            Assert.Equal(created.Id, fetched.Id);
        }

        [Fact]
        public async Task RegisterAsync_AllowsMultipleClients()
        {
            var service = CreateSut();

            var clientInfo1 = await service.RegisterAsync(new ClientName("client-A"));
            var clientInfo2 = await service.RegisterAsync(new ClientName("client-B"));

            var fetchedA = await _context.Clients.SingleAsync(c => c.Id == clientInfo1.Id);
            var fetchedB = await _context.Clients.SingleAsync(c => c.Id == clientInfo2.Id);

            Assert.NotNull(fetchedA);
            Assert.NotNull(fetchedB);
        }
    }
    
    public class RenameClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RenameAsync_UpdatesName_WhenClientExists()
        {
            var service = CreateSut();

            var initialClientName = new ClientName("new-client");
            var updatedClientName = new ClientName("old-client");
            
            var client = await service.RegisterAsync(updatedClientName);
            var updated = await service.RenameAsync(client.Id, initialClientName);

            Assert.Equal(initialClientName, updated.Name);

            var fetched = await _context.Clients.SingleAsync(c => c.Id == client.Id);
            Assert.NotNull(fetched);
            Assert.Equal(initialClientName, fetched.Name);
        }

        [Fact]
        public async Task RenameAsync_Throws_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RenameAsync(ClientId.New(), new ClientName("client")));
        }
    }

    public class DeleteClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task DeleteAsync_RemovesClient_WhenExists()
        {
            var service = CreateSut();

            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            await service.DeleteAsync(clientInfo.Id);

            var fetched = await _context.Clients.SingleOrDefaultAsync(c => c.Id == clientInfo.Id);
            Assert.Null(fetched);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenClientDoesNotExist()
        {
            var service = CreateSut();

            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => service.DeleteAsync(ClientId.New()));
        }

        [Fact]
        public async Task DeleteAsync_RemovesOnlyTargetClient()
        {
            var service = CreateSut();

            var clientInfo1 = await service.RegisterAsync(new ClientName("client-A"));
            var clientInfo2 = await service.RegisterAsync(new ClientName("client-B"));

            await service.DeleteAsync(clientInfo1.Id);

            var fetched= await _context.Clients.SingleAsync(c => c.Id == clientInfo2.Id);
            Assert.NotNull(fetched);
            Assert.Equal(clientInfo2.Name, fetched.Name);
        } 
    }

    public class ClientEnabled(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task EnableAsync_EnablesClient_WhenDisabled()
        {
            var service = CreateSut();

            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            var client = await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);
            
            await service.DisableAsync(clientInfo.Id);
            Assert.False(client.Enabled);

            await service.EnableAsync(clientInfo.Id);
            Assert.True(client.Enabled);

            var fetched= await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);
            Assert.True(fetched.Enabled);
        }

        [Fact]
        public async Task DisableAsync_DisablesClient_WhenEnabled()
        {
            var service = CreateSut();

            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            await service.DisableAsync(clientInfo.Id);

            var fetched= await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);
            Assert.False(fetched.Enabled);
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
            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            var client = await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];
            client.TryAddAudience(audience, _time.GetUtcNow());

            var updated = await service.GrantScopesAsync(client.Id, audience, scopes);

            Assert.Contains(updated.Audiences, a => a.Equals(audience));
            Assert.Equal(updated.GetAllowedScopes(audience), scopes);

            var fetched= await _context.Clients.SingleAsync(c => c.Id == client.Id);
            Assert.NotNull(fetched);
            Assert.Contains(fetched.Audiences, a => a.Equals(audience));
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
        public async Task GrantScopesAsync_Throws_WhenAudienceNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync(new ClientName("client"));
            var audience = new Audience("api");
            Scope[] scopes = [Read];

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.GrantScopesAsync(client.Id, audience, scopes));
        }

        [Fact]
        public async Task GrantScopesAsync_Idempotent_WhenGrantingSameScopeTwice()
        {
            var service = CreateSut();
            
            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            var client = await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);

            var audience = new Audience("api");
            Scope[] scopes = [Read];
            client.TryAddAudience(audience, _time.GetUtcNow());

            await service.GrantScopesAsync(client.Id, audience, scopes);
            await service.GrantScopesAsync(client.Id, audience, scopes);
            
            var updated = await service.GrantScopesAsync(client.Id, audience, scopes);
            Assert.Single(updated.Audiences);
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
            
            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            var client = await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];

            client.TryAddAudience(audience, _time.GetUtcNow());

            await service.GrantScopesAsync(client.Id, audience, scopes);
            Assert.Contains(client.Audiences, a => a.Equals(audience));
            Assert.Equal(client.GetAllowedScopes(audience), scopes);

            var updated = await service.RevokeScopesAsync(client.Id, audience, scopes);
            Assert.Empty(updated.GetAllowedScopes(audience));

            var fetched= await _context.Clients.SingleAsync(c => c.Id == client.Id);
            Assert.NotNull(fetched);
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
        public async Task RevokeScopesAsync_Throws_WhenAudienceNotFound()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync(new ClientName("client"));
            var audience = new Audience("api");
            Scope[] scopes = [Read];

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RevokeScopesAsync(client.Id, audience, scopes));
        }

        [Fact]
        public async Task RevokeScopesAsync_DoesNothing_WhenScopeDoesNotExist()
        {
            var service = CreateSut();
            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            var client = await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);

            var audience = new Audience("api");
            Scope[] scopes = [Read];
            client.TryAddAudience(audience, _time.GetUtcNow());

            await service.GrantScopesAsync(client.Id, audience, scopes);

            var updated = await service.RevokeScopesAsync(client.Id, audience, [Write]);
            Assert.Contains(updated.Audiences, a => a.Equals(audience));
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
            var clientInfo = await service.RegisterAsync(new ClientName("client"));
            var client = await _context.Clients.SingleAsync(c => c.Id == clientInfo.Id);

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];
            client.TryAddAudience(audience, _time.GetUtcNow());

            await service.GrantScopesAsync(client.Id, audience, scopes);

            var updated = await service.TryRemoveAudienceAsync(client.Id, audience);
            Assert.NotNull(updated);
            Assert.Empty(updated.Audiences);

            var fetched= await _context.Clients.SingleAsync(c => c.Id == client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.Audiences);
            Assert.Empty(fetched.GetAllowedScopes(audience));
        }

        [Fact]
        public async Task RemoveAudienceAsync_ReturnsNull_WhenClientNotFound()
        {
            var service = CreateSut();

            var client = await service.TryRemoveAudienceAsync(ClientId.New(), new Audience("api"));
            Assert.Null(client);
        }

        [Fact]
        public async Task RemoveAudienceAsync_DoesNothing_WhenAudienceDoesNotExist()
        {
            var service = CreateSut();
            var client = await service.RegisterAsync(new ClientName("client"));

            var updated = await service.TryRemoveAudienceAsync(client.Id, new Audience("missing"));
            Assert.Null(updated);

            var fetched= await _context.Clients.SingleAsync(c => c.Id == client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.Audiences);
        }
    }
}