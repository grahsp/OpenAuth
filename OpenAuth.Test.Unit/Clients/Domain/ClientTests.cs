using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Domain;

public class ClientTests
{
    private readonly FakeTimeProvider _time = new();
    
    
    [Fact]
    public void Constructor_SetsDefaults()
    {
        var clientName = new ClientName("Client");
        var now = _time.GetUtcNow();
        var client = new ClientBuilder()
            .WithName(clientName)
            .CreatedAt(now)
            .Build();

        Assert.Equal(clientName, client.Name);
        Assert.True(client.Enabled);
        Assert.NotEqual(TimeSpan.Zero, client.TokenLifetime);

        Assert.NotEqual(Guid.Empty, client.Id.Value);

        Assert.Equal(now, client.CreatedAt);
        Assert.Equal(now, client.UpdatedAt);
    }
    
    public class SetGrantTypes : ClientTests
    {
        private static readonly GrantType AuthorizationCode = GrantType.AuthorizationCode;
        private static readonly GrantType ClientCredentials = GrantType.ClientCredentials;
        
        
        [Fact]
        public void WhenValid_ReplaceExistingAndUpdateTimestamp()
        {
            var client = new ClientBuilder()
                .CreatedAt(_time.GetUtcNow())
                .Build();
            
            _time.Advance(TimeSpan.FromMinutes(5));
            
            var expectedGrantTypes = new[] { AuthorizationCode, ClientCredentials };
            var expectedTime = _time.GetUtcNow();
            
            client.SetGrantTypes(expectedGrantTypes, expectedTime);
            
            Assert.Equal(expectedGrantTypes, client.AllowedGrantTypes);
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void WhenSameGrantTypes_DoesNotUpdate()
        {
            var expectedTime = _time.GetUtcNow();
            var expectedGrantTypes = new[] { AuthorizationCode };
            
            var client = new ClientBuilder()
                .CreatedAt(expectedTime)
                .WithGrantType(AuthorizationCode)
                .Build();
            
            _time.Advance(TimeSpan.FromMinutes(5));
            client.SetGrantTypes(expectedGrantTypes, expectedTime);
            
            Assert.Equal(expectedGrantTypes, client.AllowedGrantTypes);
            Assert.Equal(expectedTime, client.UpdatedAt);
        }
        
        [Fact]
        public void WhenDuplicateGrantTypes_ThrowsException()
        {
            var client = new ClientBuilder().Build();
            
            var grantTypes = new[] { AuthorizationCode, AuthorizationCode };

            Assert.Throws<InvalidOperationException>(()
                => client.SetGrantTypes(grantTypes, _time.GetUtcNow()));
        }

        [Fact]
        public void WhenGrantTypeIsNotSupportedByApplicationType_ThrowsException()
        {
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.Spa)
                .Build();
            
            client.SetGrantTypes([ClientCredentials], _time.GetUtcNow());
            
            Assert.Throws<InvalidOperationException>(()
                => client.ValidateClient());
        }

        [Fact]
        public void WhenEmpty_ThrowsException()
        {
            var client = new ClientBuilder()
                .Build();

            Assert.Throws<InvalidOperationException>(()
                => client.SetGrantTypes([], _time.GetUtcNow()));
        }
        
        [Fact]
        public void WhenGrantTypesIsNull_ThrowsException()
        {
            var client = new ClientBuilder()
                .Build();

            Assert.Throws<ArgumentNullException>(()
                => client.SetGrantTypes(null!, _time.GetUtcNow()));
        }
    }
    
    public class SetRedirectUris : ClientTests
    {
        private static readonly RedirectUri Uri = RedirectUri.Create("https://example.com/callback");
        
        
        [Fact]
        public void WhenValid_ReplaceExistingAndUpdateTimestamp()
        {
            var client = new ClientBuilder()
                .CreatedAt(_time.GetUtcNow())
                .Build();
            
            _time.Advance(TimeSpan.FromMinutes(5));
            
            var expectedUris = new[] { Uri };
            var expectedTime = _time.GetUtcNow();
            
            client.SetRedirectUris(expectedUris, expectedTime);
            
            Assert.Equal(expectedUris, client.RedirectUris);
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void WhenSameUris_DoesNotUpdate()
        {
            var expectedTime = _time.GetUtcNow();
            var expectedUris = new[] { Uri };
            
            var client = new ClientBuilder()
                .CreatedAt(expectedTime)
                .Build();
            
            _time.Advance(TimeSpan.FromMinutes(5));
            client.SetRedirectUris(expectedUris, expectedTime);
            
            Assert.Equal(expectedUris, client.RedirectUris);
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void WhenEmptyCollectionAndIsNotRequiredByApplicationType_UpdateCollection()
        {
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.M2M)
                .Build();
            
            client.SetRedirectUris([], _time.GetUtcNow());
            client.ValidateClient();
            
            Assert.Empty(client.RedirectUris);
        }

        [Fact]
        public void WhenEmptyCollectionAndIsRequiredByApplicationType_ThrowsException()
        {
            
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.Spa)
                .Build();
            
            client.SetRedirectUris([], _time.GetUtcNow());

            Assert.Throws<InvalidOperationException>(()
                => client.ValidateClient());
        }
        
        [Fact]
        public void WhenDuplicateUris_ThrowsException()
        {
            var client = new ClientBuilder().Build();
            
            var redirectUris = new[] { Uri, Uri };

            Assert.Throws<InvalidOperationException>(()
                => client.SetRedirectUris(redirectUris, _time.GetUtcNow()));
        }
        
        [Fact]
        public void WhenRedirectUrisNull_ThrowsException()
        {
            var client = new ClientBuilder()
                .Build();

            Assert.Throws<ArgumentNullException>(()
                => client.SetRedirectUris(null!, _time.GetUtcNow()));
        }
    }
    
    public class SetAudiences : ClientTests
    {
        private static readonly Audience ApiAudience =
            new(AudienceName.Create("api"), ScopeCollection.Parse("read write"));
        
        private static readonly Audience WebAudience =
            new(AudienceName.Create("web"), ScopeCollection.Parse("read"));
        
        
        [Fact]
        public void WhenValid_ReplaceExistingAndUpdateTimestamp()
        {
            var client = new ClientBuilder()
                .CreatedAt(_time.GetUtcNow())
                .Build();
            
            _time.Advance(TimeSpan.FromMinutes(5));
            
            var expectedAudiences = new[] { ApiAudience, WebAudience };
            var expectedTime = _time.GetUtcNow();
            
            client.SetAudiences(expectedAudiences, expectedTime);
            
            Assert.Equal(expectedAudiences, client.AllowedAudiences);
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void WhenSameAudiences_DoesNotUpdate()
        {
            var expectedTime = _time.GetUtcNow();
            var expectedAudiences = new[] { ApiAudience };
            
            var client = new ClientBuilder()
                .WithAudience(ApiAudience)
                .CreatedAt(expectedTime)
                .Build();

            _time.Advance(TimeSpan.FromMinutes(5));
            client.SetAudiences(expectedAudiences, _time.GetUtcNow());
            
            Assert.Equal(expectedAudiences, client.AllowedAudiences);
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void WhenDuplicateNames_ThrowsException()
        {
            var client = new ClientBuilder().Build();
            
            var audiences = new[] { ApiAudience, ApiAudience };

            Assert.Throws<InvalidOperationException>(()
                => client.SetAudiences(audiences, _time.GetUtcNow()));
        }

        [Fact]
        public void WhenEmptyCollectionAndApplicationTypeDoesNotRequireIt_ClientIsInValidState()
        {
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.Spa)
                .WithAudience(ApiAudience)
                .Build();
            
            client.SetAudiences([], _time.GetUtcNow());
            client.ValidateClient();

            Assert.Empty(client.AllowedAudiences);
        }

        [Fact]
        public void WhenEmptyCollectionAndApplicationTypeRequiresIt_ThrowsException()
        {
            var client = new ClientBuilder()
                .WithApplicationType(ClientApplicationTypes.M2M)
                .WithAudience(ApiAudience)
                .Build();
            
            client.SetAudiences([], _time.GetUtcNow());
            
            Assert.Throws<InvalidOperationException>(()
                => client.ValidateClient());
        }

        [Fact]
        public void WhenAudiencesNull_ThrowsException()
        {
            var client = new ClientBuilder()
                .Build();

            Assert.Throws<ArgumentNullException>(()
                => client.SetAudiences(null!, _time.GetUtcNow()));
        }
    }

    public class Rename : ClientTests
    {
        [Fact]
        public void Rename_UpdateName_WhenValid()
        {
            var client = new ClientBuilder().Build();
            var newClientName = new ClientName("New App");
            
            client.Rename(newClientName, _time.GetUtcNow());
            Assert.Equal(newClientName, client.Name);
        }
        
        [Fact]
        public void Rename_ThrowsWhen_Null()
        {
            var client = new ClientBuilder().Build();
            
            Assert.Throws<ArgumentNullException>(() =>
                client.Rename(null!, _time.GetUtcNow()));
        }

        [Fact]
        public void Rename_TouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.Rename(new ClientName("new-client"), expected);
            
            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void Rename_NoOp_DoesNotTouchUpdatedAt()
        {
            var now = _time.GetUtcNow();
            var client = new ClientBuilder()
                .CreatedAt(now)
                .Build();
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var updatedTime = _time.GetUtcNow();
            client.Rename(client.Name, updatedTime);
            
            Assert.NotEqual(updatedTime, client.UpdatedAt);
            Assert.Equal(now, client.UpdatedAt);
        }
    }

    public class SetTokenLifetime : ClientTests
    {
        [Fact]
        public void SetTokenLifetime_SetTokenLifetime_WhenValid()
        {
            var client = new ClientBuilder().Build();
            var newLifetime = TimeSpan.FromSeconds(1);
            
            client.SetTokenLifetime(newLifetime, _time.GetUtcNow());
            
            Assert.Equal(newLifetime, client.TokenLifetime);
        }

        [Fact]
        public void SetTokenLifetime_ThrowsOn_ZeroValue()
        {
            var client = new ClientBuilder().Build();
            Assert.Throws<ArgumentOutOfRangeException>(()
                => client.SetTokenLifetime(TimeSpan.Zero, _time.GetUtcNow()));
        }

        [Fact]
        public void SetTokenLifetime_TouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.SetTokenLifetime(TimeSpan.FromSeconds(1), expected);
            
            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void SetTokenLifetime_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            
            var before = client.UpdatedAt;
            client.SetTokenLifetime(client.TokenLifetime, _time.GetUtcNow());
            
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class AddSecret : ClientTests
    {
        [Fact]
        public void AddsSecret_WhenValid()
        {
            var client = new ClientBuilder().Build();
            var hash = new SecretHash("hash");
            
            var utcNow = _time.GetUtcNow();
            var secretId = client.AddSecret(hash, utcNow);

            Assert.Contains(client.Secrets, s => s.Id == secretId && s.IsActive(utcNow));
        }
        
        [Fact]
        public void UpdateTimestamp_WhenSuccess()
        {
            var client = new ClientBuilder().Build();
            var hash = new SecretHash("hash");
            
            _time.Advance(TimeSpan.FromHours(1));
            var expected = _time.GetUtcNow();
            
            client.AddSecret(hash, expected);
            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void Throws_WhenExceedsMaxSecrets()
        {
            var client = new ClientBuilder().Build();
            var hash = new SecretHash("hash");
            
            var utcNow = _time.GetUtcNow();

            for (var i = 0; i < Client.MaxSecrets; i++)
                client.AddSecret(hash, utcNow);
            
            Assert.Throws<InvalidOperationException>(()
                => client.AddSecret(hash, utcNow));
        }
    }

    public class RevokeSecret : ClientTests
    {
        [Fact]
        public void Throws_WhenSecretNotFound()
        {
            var client = new ClientBuilder().Build();

            Assert.Throws<InvalidOperationException>(()
                => client.RevokeSecret(SecretId.New(), _time.GetUtcNow()));
        }

        [Fact]
        public void DoesNothing_WhenSecretIsInactive()
        {
            var client = new ClientBuilder()
                .WithSecret("hash")
                .WithSecret("hash")
                .Build();

            _time.Advance(TimeSpan.FromMinutes(30));
            var expected = _time.GetUtcNow();
            
            var secret = client.Secrets.First();
            client.RevokeSecret(secret.Id, expected);

            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void Throws_WhenRevokingLastSecret()
        {
            var client = new ClientBuilder()
                .WithSecret("hash")
                .Build();

            var secret = client.Secrets.First();
            Assert.Throws<InvalidOperationException>(()
                => client.RevokeSecret(secret.Id, _time.GetUtcNow()));           
        }

        [Fact]
        public void MarksSecretAsInactive_WhenValid()
        {
            var client = new ClientBuilder()
                .WithSecret("hash")
                .WithSecret("hash")
                .Build();
            
            var secret = client.Secrets.First();
            client.RevokeSecret(secret.Id, _time.GetUtcNow());
            
            Assert.False(secret.IsActive(_time.GetUtcNow()));
        }
        
        [Fact]
        public void UpdateTimestamp_WhenSuccessful()
        {
            var client = new ClientBuilder()
                .WithSecret("hash")
                .WithSecret("hash")
                .Build();
            
            _time.Advance(TimeSpan.FromHours(1));
            var expected = _time.GetUtcNow();
            
            var secret = client.Secrets.First();
            client.RevokeSecret(secret.Id, expected);

            Assert.Equal(expected, client.UpdatedAt);
        }
    }

    public class GetActiveSecrets : ClientTests
    {
        [Fact]
        public void ReturnOnlyActiveSecrets()
        {
            var client = new ClientBuilder().Build();
            var expired = new SecretHash("expired");
            var revoked = new SecretHash("revoked");
            var active = new SecretHash("active");
            
            // Advance time to expire token immediately.
            client.AddSecret(expired, _time.GetUtcNow());
            _time.Advance(TimeSpan.FromDays(365));
            
            // Revoke secret immediately.
            client.AddSecret(revoked, _time.GetUtcNow());
            client.Secrets.First(s => s.Hash == revoked)
                .Revoke(_time.GetUtcNow());
            
            // Add active secret.
            client.AddSecret(active, _time.GetUtcNow());
        
            var result = client
                .ActiveSecrets(_time.GetUtcNow())
                .ToArray();
        
            Assert.Contains(result, r => r.Hash == active);
            Assert.DoesNotContain(result, r => r.Hash == expired);
            Assert.DoesNotContain(result, r => r.Hash == revoked);
        }
    }

    public class Enabled : ClientTests
    {
        [Fact]
        public void EnableAndDisable_TogglesEnable_And_TouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            
            // Act - Disable
            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedAfterDisable = _time.GetUtcNow();
            client.Disable(_time.GetUtcNow());
            
            Assert.False(client.Enabled);
            Assert.Equal(expectedAfterDisable, client.UpdatedAt);

            // Act - Enable
            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedAfterEnable = _time.GetUtcNow();
            client.Enable(_time.GetUtcNow());
            
            Assert.True(client.Enabled);
            Assert.Equal(expectedAfterEnable, client.UpdatedAt);
        }

        [Fact]
        public void Enable_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            var before = client.UpdatedAt;

            client.Enable(_time.GetUtcNow());
            Assert.Equal(before, client.UpdatedAt);           
        }

        [Fact]
        public void Disable_NoOup_DoesNotTouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            client.Disable(_time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.Disable(_time.GetUtcNow());
            Assert.Equal(before, client.UpdatedAt);
        }
    }
}