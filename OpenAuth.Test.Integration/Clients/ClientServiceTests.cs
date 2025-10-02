using Microsoft.Extensions.Time.Testing;
using OpenAuth.Application.Clients;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Clients;
using OpenAuth.Infrastructure.Repositories;
using OpenAuth.Test.Common.Fakes;
using OpenAuth.Test.Integration.Fixtures;

namespace OpenAuth.Test.Integration.Clients;


[Collection("sqlserver")]
public class ClientServiceTests : IAsyncLifetime
{
    private readonly SqlServerFixture _fx;
    private readonly TimeProvider _time = new FakeTimeProvider();
    
    public ClientServiceTests(SqlServerFixture fx) => _fx = fx;    

    public async Task InitializeAsync() => await _fx.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private ClientService CreateSut()
    {
        var context = _fx.CreateContext();
        return new ClientService(
            new ClientRepository(context),
            new ClientFactory(_time),
            _time);
    }

    public class GetClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task GetByIdAsync_ReturnsClient_WhenExists()
        {
            var service = CreateSut();

            var created = await service.RegisterAsync(new ClientName("client"));
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

            var clientName = new ClientName("client");
            var created = await service.RegisterAsync(clientName);
            var fetched = await service.GetByNameAsync(clientName);

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched.Id);
            Assert.Equal(created.Name, fetched.Name);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsNull_WhenNotFound()
        {
            var service = CreateSut();

            var fetched = await service.GetByNameAsync(new ClientName("missing"));
            Assert.Null(fetched);
        }
    }

    public class RegisterClient(SqlServerFixture fx) : ClientServiceTests(fx)
    {
        [Fact]
        public async Task RegisterAsync_CreatesAndPersistsClient()
        {
            var service = CreateSut();

            var created = await service.RegisterAsync(new ClientName("client"));
            var fetched = await service.GetByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal(created.Name, fetched.Name);
            Assert.Equal(created.Id, fetched.Id);
        }

        [Fact]
        public async Task RegisterAsync_AllowsMultipleClients()
        {
            var service = CreateSut();

            var clientA = await service.RegisterAsync(new ClientName("client-A"));
            var clientB = await service.RegisterAsync(new ClientName("client-B"));

            var fetchedA = await service.GetByIdAsync(clientA.Id);
            var fetchedB = await service.GetByIdAsync(clientB.Id);

            Assert.NotNull(fetchedA);
            Assert.NotNull(fetchedB);
            
            Assert.Same(clientA, fetchedA);
            Assert.Same(clientB, fetchedB);
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

            var fetched = await service.GetByIdAsync(client.Id);

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

            var client = await service.RegisterAsync(new ClientName("client"));
            
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

            var client1 = await service.RegisterAsync(new ClientName("client-A"));
            var client2 = await service.RegisterAsync(new ClientName("client-B"));

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

            var client = await service.RegisterAsync(new ClientName("client"));
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

            var client = await service.RegisterAsync(new ClientName("client"));
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
            var client = await service.RegisterAsync(new ClientName("client"));

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];
            client.TryAddAudience(audience, _time.GetUtcNow());

            var updated = await service.GrantScopesAsync(client.Id, audience, scopes);

            Assert.Contains(updated.Audiences, a => a.Equals(audience));
            Assert.Equal(updated.GetAllowedScopes(audience), scopes);

            var fetched = await service.GetByIdAsync(client.Id);
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
            var client = await service.RegisterAsync(new ClientName("client"));

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
            var client = await service.RegisterAsync(new ClientName("client"));

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];

            client.TryAddAudience(audience, _time.GetUtcNow());

            await service.GrantScopesAsync(client.Id, audience, scopes);
            Assert.Contains(client.Audiences, a => a.Equals(audience));
            Assert.Equal(client.GetAllowedScopes(audience), scopes);

            var updated = await service.RevokeScopesAsync(client.Id, audience, scopes);
            Assert.Empty(updated.GetAllowedScopes(audience));

            var fetched = await service.GetByIdAsync(client.Id);
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
            var client = await service.RegisterAsync(new ClientName("client"));

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
            var client = await service.RegisterAsync(new ClientName("client"));

            var audience = new Audience("api");
            Scope[] scopes = [Read, Write];
            client.TryAddAudience(audience, _time.GetUtcNow());

            await service.GrantScopesAsync(client.Id, audience, scopes);

            var updated = await service.TryRemoveAudienceAsync(client.Id, audience);
            Assert.NotNull(updated);
            Assert.Empty(updated.Audiences);

            var fetched = await service.GetByIdAsync(client.Id);
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

            var fetched = await service.GetByIdAsync(client.Id);
            Assert.NotNull(fetched);
            Assert.Empty(fetched.Audiences);
        }
    }
}