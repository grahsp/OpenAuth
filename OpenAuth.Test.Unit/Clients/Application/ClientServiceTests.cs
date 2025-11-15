using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Factories;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients.Application;

public class ClientServiceTests
{
    private FakeClientRepository _repo;
    private IClientFactory _clientFactory;
    private IClientConfigurationFactory _configurationFactory;
    
    private IClientService _sut;

    public ClientServiceTests()
    {
        var time = new FakeTimeProvider();
        
        _repo = new FakeClientRepository();
        _clientFactory = Substitute.For<IClientFactory>();
        _configurationFactory = Substitute.For<IClientConfigurationFactory>();
        
        _sut = new ClientService(_repo, _clientFactory, _configurationFactory, time);
    }

    private void SetupDefaultClientConfiguration()
    {
        _clientFactory.Create(Arg.Any<ClientConfiguration>(), out _)
            .Returns(x =>
            {
                var config = x.Arg<ClientConfiguration>();
                x[1] = config.ApplicationType.AllowsClientSecrets
                    ? "plain-secret"
                    : null;

                var client = new ClientBuilder()
                    .WithName(config.Name)
                    .WithApplicationType(config.ApplicationType);
                
                foreach (var audience in config.AllowedAudiences)
                    client.WithAudience(audience.Name.ToString(), audience.Scopes.Select(s => s.ToString()).ToArray());

                foreach (var grantType in config.AllowedGrantTypes)
                    client.WithGrantType(grantType.Value);

                foreach (var redirectUri in config.RedirectUris)
                    client.WithRedirectUri(redirectUri.Value);

                return client.Build();
            });
        
        _configurationFactory.Create(Arg.Any<RegisterClientRequest>())
            .Returns(x =>
            {
                var request = x.Arg<RegisterClientRequest>();
                return DeriveConfiguration(request);
            });
    }

    private static ClientConfiguration DeriveConfiguration(RegisterClientRequest request)
    {
        var clientName = ClientName.Create(request.Name);
        var applicationType = ClientApplicationTypes.Parse(request.ApplicationType);
        
        var audiences = request.Permissions?
            .Select(permission =>
        {
            var audienceName = AudienceName.Create(permission.Key);
            var scopes = permission.Value.Select(s => new Scope(s));

            var scopeCollection = new ScopeCollection(scopes);
            var audience = new Audience(audienceName, scopeCollection);

            return audience;
        }) ?? [];
        
        var redirectUris = request.RedirectUris?
            .Select(RedirectUri.Create) ?? [];
            
        var config = new ClientConfiguration(
            clientName,
            applicationType,
            audiences,
            applicationType.DefaultGrantTypes,
            redirectUris
        );

        return config;
    }

    private static RegisterClientRequest CreateM2MRequest()
        => new(
            "m2m", 
            "test client", 
            new Dictionary<string, IEnumerable<string>>
            { { "api", ["read", "write"] } }, 
            []);
    
    private static RegisterClientRequest CreateSpaRequest()
        => new(
            "spa", 
            "test client", 
            [],
            ["https://example.com/callback"]);

    private async Task<RegisteredClientResponse> RegisterClientAsync(RegisterClientRequest request)
    {
        SetupDefaultClientConfiguration();
        return await _sut.RegisterAsync(request);
    }
    
    private async Task<RegisteredClientResponse> RegisterM2MClientAsync(RegisterClientRequest? request = null)
        => await RegisterClientAsync(request ?? CreateM2MRequest());

    private async Task<RegisteredClientResponse> RegisterSpaClientAsync(RegisterClientRequest? request = null)
        => await RegisterClientAsync(request ?? CreateSpaRequest());
    


    public class RegisterAsync : ClientServiceTests
    {
        [Fact]
        public async Task RegisterM2mClient_ReturnsSecret()
        {
            var result = await RegisterM2MClientAsync();

            Assert.NotNull(result.Client);
            Assert.NotNull(result.ClientSecret);
            
            Assert.NotEmpty(_repo.Clients);
            Assert.True(_repo.Saved);
        }

        [Fact]
        public async Task RegisterSpaClient_DoesNotReturnSecret()
        {
            var result = await RegisterSpaClientAsync();

            Assert.NotNull(result.Client);
            Assert.Null(result.ClientSecret);
            
            Assert.NotEmpty(_repo.Clients);
            Assert.True(_repo.Saved);           
        }

        [Fact]
        public async Task WhenNull_ThrowsException()
        {
            SetupDefaultClientConfiguration();
            
            await Assert.ThrowsAsync<ArgumentNullException>(()
                => RegisterClientAsync(null!));       
        }
    }

    public class RenameAsync : ClientServiceTests {
        [Fact]
        public async Task WhenValidName_UpdatesClientName()
        {
            var result = await RegisterSpaClientAsync();
            var newName = new ClientName("cool client");
            
            var renamed = await _sut.RenameAsync(ClientId.Create(result.Client.Id), newName);
            
            Assert.Equal(newName.ToString(), renamed.Name);
        }

        [Fact]
        public async Task WhenInvalidName_ThrowsException()
        {
            var result = await RegisterSpaClientAsync();
            
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.RenameAsync(ClientId.Create(result.Client.Id), null!));
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            var service = _sut;
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RenameAsync(ClientId.New(), new ClientName("test")));
        }
    }

    public class DeleteAsync : ClientServiceTests
    {
        [Fact]
        public async Task WhenClientExists_DeleteClient()
        {
            var result = await RegisterSpaClientAsync();

            await _sut.DeleteAsync(ClientId.Create(result.Client.Id));
            
            Assert.Empty(_repo.Clients);
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.DeleteAsync(ClientId.New()));
        }
    }

    public class Audiences : ClientServiceTests
    {
        private static readonly Audience ApiAudience = new(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
        private static readonly Audience WebAudience = new(AudienceName.Create("web"), ScopeCollection.Parse("read"));
        
        
        [Fact]
        public async Task SetAudiencesAsync_WhenValidAudiences_ReplaceExistingAudiences()
        {
            var result = await RegisterSpaClientAsync();
            var client = _repo.Clients.Single(c => c.Id == ClientId.Create(result.Client.Id));
            
            var audiences = new[] { ApiAudience, WebAudience };
            
            await _sut.SetAudiencesAsync(client.Id, audiences);

            Assert.Equal(2, client.AllowedAudiences.Count);
            Assert.Contains(client.AllowedAudiences, a => a == ApiAudience);
            Assert.Contains(client.AllowedAudiences, a => a == WebAudience);
            
            Assert.True(_repo.Saved);
        }
        
        [Fact]
        public async Task SetAudiencesAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.SetAudiencesAsync(ClientId.New(), []));
        }

        [Fact]
        public async Task AddAudienceAsync_WhenValid_AppendsWithoutAffectingExistingAudiences()
        {
            var result = await RegisterSpaClientAsync();
            var client = _repo.Clients.Single(c => c.Id == ClientId.Create(result.Client.Id));
            
            await _sut.AddAudienceAsync(client.Id, ApiAudience);
            
            Assert.Contains(client.AllowedAudiences, a => a == ApiAudience);
            Assert.True(_repo.Saved);
        }

        [Fact]
        public async Task AddAudienceAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.AddAudienceAsync(ClientId.New(), ApiAudience));
        }
        
        [Fact]
        public async Task RemoveAudienceAsync_WhenAudienceNotFound_DoesNothing()
        {
            var result = await RegisterSpaClientAsync();
            var client = _repo.Clients.Single(c => c.Id == ClientId.Create(result.Client.Id));

            var expected = client.AllowedAudiences.ToArray();
            await _sut.RemoveAudienceAsync(client.Id, AudienceName.Create("non-existent"));
            
            Assert.Equal(expected, client.AllowedAudiences);
        }

        [Fact]
        public async Task RemoveAudienceAsync_WhenAudienceExists_RemoveAudience()
        {
            var result = await RegisterSpaClientAsync();
            var client = _repo.Clients.Single(c => c.Id == ClientId.Create(result.Client.Id));
            
            await _sut.AddAudienceAsync(client.Id, ApiAudience);
            
            await _sut.RemoveAudienceAsync(client.Id, ApiAudience.Name);
            
            Assert.DoesNotContain(client.AllowedAudiences, a => a == ApiAudience);
            Assert.True(_repo.Saved);
        }
        
        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.RemoveAudienceAsync(ClientId.New(), AudienceName.Create("non-existent")));
        }
    }
    
    public class EnableDisable : ClientServiceTests
    {
        [Fact]
        public async Task EnableAndDisable_TogglesEnabled()
        {
            var result = await RegisterSpaClientAsync();
            var client = _repo.Clients.Single(c => c.Id == ClientId.Create(result.Client.Id));

            await _sut.DisableAsync(ClientId.Create(result.Client.Id));
            Assert.False(client.Enabled);

            await _sut.EnableAsync(client.Id);
            Assert.True(client.Enabled);
        }

        [Fact]
        public async Task EnableAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.EnableAsync(ClientId.New()));
        }

        [Fact]
        public async Task DisableAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DisableAsync(ClientId.New()));
        }
    }
}