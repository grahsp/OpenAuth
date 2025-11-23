using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Factories;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Fakes;

namespace OpenAuth.Test.Unit.Clients.Application;

public class ClientServiceTests
{
    private FakeClientRepository _repo;
    private IClientFactory _clientFactory;
    
    private IClientService _sut;

    public ClientServiceTests()
    {
        var time = new FakeTimeProvider();
        
        _repo = new FakeClientRepository();
        _clientFactory = Substitute.For<IClientFactory>();
        
        _sut = new ClientService(_repo, _clientFactory, time);
    }

    private void SetupDefaultClientConfiguration()
    {
        _clientFactory.Create(Arg.Any<CreateClientRequest>(), out _)
            .Returns(x =>
            {
                var config = x.Arg<CreateClientRequest>();
                x[1] = config.ApplicationType.AllowsClientSecrets
                    ? "plain-secret"
                    : null;

                var client = new ClientBuilder()
                    .WithName(config.Name)
                    .WithApplicationType(config.ApplicationType);
                
                foreach (var audience in config.Permissions)
                    client.WithAudience(audience.Name.ToString(), audience.Scopes.Select(s => s.ToString()).ToArray());

                foreach (var grantType in config.ApplicationType.DefaultGrantTypes)
                    client.WithGrantType(grantType.Value);

                foreach (var redirectUri in config.RedirectUris)
                    client.WithRedirectUri(redirectUri.Value);

                return client.Build();
            });
    }

    private static CreateClientRequest CreateM2MRequest()
        => new(
            ClientApplicationTypes.M2M,
            ClientName.Create("test client"),
            [new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"))],
            []
        );

    private static CreateClientRequest CreateSpaRequest()
        => new(
            ClientApplicationTypes.Spa,
            ClientName.Create("test client"),
            [],
            [RedirectUri.Create("https://example.com/callback")]
        );


    private async Task<RegisteredClientResponse> RegisterClientAsync(CreateClientRequest request)
    {
        SetupDefaultClientConfiguration();
        return await _sut.RegisterAsync(request);
    }
    
    private async Task<RegisteredClientResponse> RegisterM2MClientAsync(CreateClientRequest? request = null)
        => await RegisterClientAsync(request ?? CreateM2MRequest());

    private async Task<RegisteredClientResponse> RegisterSpaClientAsync(CreateClientRequest? request = null)
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
            
            var renamed = await _sut.RenameAsync(result.Client.Id, newName);
            
            Assert.Equal(newName.ToString(), renamed.Name);
        }

        [Fact]
        public async Task WhenInvalidName_ThrowsException()
        {
            var result = await RegisterSpaClientAsync();
            
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.RenameAsync(result.Client.Id, null!));
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.RenameAsync(ClientId.New(), new ClientName("test")));
        }
    }

    public class DeleteAsync : ClientServiceTests
    {
        [Fact]
        public async Task WhenClientExists_DeleteClient()
        {
            var result = await RegisterSpaClientAsync();

            await _sut.DeleteAsync(result.Client.Id);
            
            Assert.Empty(_repo.Clients);
        }

        [Fact]
        public async Task WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.DeleteAsync(ClientId.New()));
        }
    }
    
    public class GrantTypeMethods : ClientServiceTests
    {
        private static readonly GrantType ClientCredentials = GrantType.ClientCredentials;
        private static readonly GrantType RefreshToken = GrantType.RefreshToken;
        
        
        [Fact]
        public async Task SetGrantTypesAsync_WhenValidTypes_ReplaceExistingTypes()
        {
            var result = await RegisterM2MClientAsync();
            var client = result.Client;
            
            var grantTypes = new[] { ClientCredentials, RefreshToken };
            
            await _sut.SetGrantTypesAsync(client.Id, grantTypes);
    
            Assert.Equal(2, client.AllowedGrantTypes.Count);
            Assert.Contains(client.AllowedGrantTypes, a => a == ClientCredentials);
            Assert.Contains(client.AllowedGrantTypes, a => a == RefreshToken);
            
            Assert.True(_repo.Saved);
        }
        
        [Fact]
        public async Task SetRedirectUrisAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.SetGrantTypesAsync(ClientId.New(), [RefreshToken]));
        }
    
        [Fact]
        public async Task AddGrantTypeAsync_WhenValid_AppendsWithoutAffectingExistingTypes()
        {
            var result = await RegisterM2MClientAsync();
            var client = result.Client;
            
            await _sut.AddGrantTypeAsync(client.Id, RefreshToken);
            
            Assert.Contains(client.AllowedGrantTypes, a => a == RefreshToken);
            Assert.True(_repo.Saved);
        }
    
        [Fact]
        public async Task AddGrantTYpeAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.AddGrantTypeAsync(ClientId.New(), RefreshToken));
        }
        
        [Fact]
        public async Task RemoveGrantTypeAsync_WhenTypeNotFound_DoesNothing()
        {
            var result = await RegisterM2MClientAsync();
            var client = result.Client;
    
            var expected = client.AllowedGrantTypes.ToArray();
            await _sut.RemoveGrantTypeAsync(client.Id, RefreshToken);
            
            Assert.Equal(expected, client.AllowedGrantTypes);
        }
    
        [Fact]
        public async Task RemoveGrantTypeAsync_WhenTypeExists_RemoveType()
        {
            var result = await RegisterM2MClientAsync();
            var client = result.Client;
            
            await _sut.AddGrantTypeAsync(client.Id, RefreshToken);
            
            await _sut.RemoveGrantTypeAsync(client.Id, ClientCredentials);
            
            Assert.DoesNotContain(client.AllowedGrantTypes, a => a == ClientCredentials);
            Assert.True(_repo.Saved);
        }
        
        [Fact]
        public async Task RemoveGrantTypeAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.RemoveGrantTypeAsync(ClientId.New(), ClientCredentials));
        }
    }
    
    public class RedirectUriMethods : ClientServiceTests
    {
        private static readonly RedirectUri UriA = RedirectUri.Create("https://a.com/callback");
        private static readonly RedirectUri UriB = RedirectUri.Create("https://b.com/callback");
        
        
        [Fact]
        public async Task SetRedirectUrisAsync_WhenValidUris_ReplaceExistingUris()
        {
            var result = await RegisterM2MClientAsync();
            var client = result.Client;
            
            var redirectUris = new[] { UriA, UriB };
            
            await _sut.SetRedirectUrisAsync(client.Id, redirectUris);

            Assert.Equal(2, client.RedirectUris.Count);
            Assert.Contains(client.RedirectUris, a => a == UriA);
            Assert.Contains(client.RedirectUris, a => a == UriB);
            
            Assert.True(_repo.Saved);
        }
        
        [Fact]
        public async Task SetRedirectUrisAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.SetRedirectUrisAsync(ClientId.New(), [UriA]));
        }

        [Fact]
        public async Task AddRedirectUriAsync_WhenValid_AppendsWithoutAffectingExistingUris()
        {
            var result = await RegisterSpaClientAsync();
            var client = result.Client;
            
            await _sut.AddRedirectUriAsync(client.Id, UriA);
            
            Assert.Contains(client.RedirectUris, a => a == UriA);
            Assert.True(_repo.Saved);
        }

        [Fact]
        public async Task AddRedirectUriAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.AddRedirectUriAsync(ClientId.New(), UriA));
        }
        
        [Fact]
        public async Task RemoveRedirectUriAsync_WhenUriNotFound_DoesNothing()
        {
            var result = await RegisterSpaClientAsync();
            var client = result.Client;

            var expected = client.RedirectUris.ToArray();
            await _sut.RemoveRedirectUriAsync(client.Id, UriA);
            
            Assert.Equal(expected, client.RedirectUris);
        }

        [Fact]
        public async Task RemoveRedirectUriAsync_WhenUriExists_RemoveUri()
        {
            var result = await RegisterSpaClientAsync();
            var client = result.Client;
            
            await _sut.AddRedirectUriAsync(client.Id, UriA);
            
            await _sut.RemoveRedirectUriAsync(client.Id, UriA);
            
            Assert.DoesNotContain(client.RedirectUris, a => a == UriA);
            Assert.True(_repo.Saved);
        }
 
        [Fact]
        public async Task RemoveRedirectUriAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.RemoveRedirectUriAsync(ClientId.New(), UriA));
        }
    }

    public class AudienceMethods : ClientServiceTests
    {
        private static readonly Audience ApiAudience = new(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
        private static readonly Audience WebAudience = new(AudienceName.Create("web"), ScopeCollection.Parse("read"));
        
        
        [Fact]
        public async Task SetAudiencesAsync_WhenValidAudiences_ReplaceExistingAudiences()
        {
            var result = await RegisterSpaClientAsync();
            var client = result.Client;
            
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
            var client = result.Client;
            
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
            var client = result.Client;

            var expected = client.AllowedAudiences.ToArray();
            await _sut.RemoveAudienceAsync(client.Id, AudienceName.Create("non-existent"));
            
            Assert.Equal(expected, client.AllowedAudiences);
        }

        [Fact]
        public async Task RemoveAudienceAsync_WhenAudienceExists_RemoveAudience()
        {
            var result = await RegisterSpaClientAsync();
            var client = result.Client;
            
            await _sut.AddAudienceAsync(client.Id, ApiAudience);
            await _sut.RemoveAudienceAsync(client.Id, ApiAudience.Name);
            
            Assert.DoesNotContain(client.AllowedAudiences, a => a == ApiAudience);
            Assert.True(_repo.Saved);
        }
        
        [Fact]
        public async Task RemoveAudiencesAsync_WhenClientNotFound_ThrowsException()
        {
            await Assert.ThrowsAnyAsync<InvalidOperationException>(()
                => _sut.RemoveAudienceAsync(ClientId.New(), ApiAudience.Name));
        }
    }
    
    public class EnableDisable : ClientServiceTests
    {
        [Fact]
        public async Task EnableAndDisable_TogglesEnabled()
        {
            var result = await RegisterSpaClientAsync();
            var client = result.Client;

            await _sut.DisableAsync(client.Id);
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