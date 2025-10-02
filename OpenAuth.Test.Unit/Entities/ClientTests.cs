using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Entities;

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
        Assert.Empty(client.Audiences);

        Assert.Equal(now, client.CreatedAt);
        Assert.Equal(now, client.UpdatedAt);
    }

    public class GrantScopes : ClientTests
    {
        [Fact]
        public void GrantScopes_AddsScopes_AddScopes_NoDuplicates()
        {
            var client = new ClientBuilder().Build();
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");

            client.TryAddAudience(aud, _time.GetUtcNow());
            client.GrantScopes(aud, [read, write, write], _time.GetUtcNow());

            var scopes = client.GetAllowedScopes(aud);
            Assert.Equal(2, scopes.Count);
            Assert.Contains(read, scopes);
            Assert.Contains(write, scopes);
            Assert.Contains(aud, client.Audiences);
        }

        [Fact]
        public void GrantScopes_TouchUpdatedAt()
        {
            var now = _time.GetUtcNow();
            var client = new ClientBuilder()
                .CreatedAt(now)
                .Build();
            
            var aud = new Audience("api");
            var read = new Scope("read");
            client.TryAddAudience(aud, now);
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.GrantScopes(aud, [read], expected);

            Assert.Equal(expected, client.UpdatedAt);
        }
    }

    public class RevokeScopes : ClientTests
    {
        [Fact]
        public void RevokeScopes_RemovesScopes_RemoveAllButOneScope_AudienceRemains()
        {
            var client = new ClientBuilder().Build();
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            
            client.TryAddAudience(aud, _time.GetUtcNow());
            client.GrantScopes(aud, [read, write], _time.GetUtcNow());

            client.RevokeScopes(aud, [read], _time.GetUtcNow());

            var scopes = client.GetAllowedScopes(aud);
            Assert.Single(scopes);
            Assert.Contains(write, scopes);
        }

        [Fact]
        public void RevokeScopes_TouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            var aud = new Audience("api");
            var read = new Scope("read");
            
            client.TryAddAudience(aud, _time.GetUtcNow());
            client.GrantScopes(aud, [read], _time.GetUtcNow());
        
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.RevokeScopes(aud, [read], _time.GetUtcNow());

            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void RevokeScopes_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            var aud = new Audience("unknown");
            client.TryAddAudience(aud, _time.GetUtcNow());

            var before = client.UpdatedAt;
            client.RevokeScopes(aud, [new Scope("does-not-exist")], _time.GetUtcNow());
        
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class TryAddAudience : ClientTests
    {
        [Fact]
        public void ReturnsTrue_WhenAudienceDoesNotExists()
        {
            var client = new ClientBuilder().Build();
            var api = new Audience("api");

            var result = client.TryAddAudience(api, _time.GetUtcNow());
            
            Assert.True(result);
            Assert.Contains(api, client.Audiences);
        }
        
        [Fact]
        public void ReturnsFalse_WhenAudienceAlreadyExists()
        {
            var client = new ClientBuilder().Build();
            
            // Ensure adding is not done by reference
            var apiA = new Audience("api");
            var apiB = new Audience("api");

            client.TryAddAudience(apiA, _time.GetUtcNow());
            var result = client.TryAddAudience(apiB, _time.GetUtcNow());
            
            Assert.False(result);
            Assert.Single(client.Audiences);
        }
        
        [Fact]
        public void ReturnsFalse_WhenAudienceExistWithDifferentCasing()
        {
            var client = new ClientBuilder().Build();
            
            // Ensure adding is not done by reference
            var apiA = new Audience("api");
            var apiB = new Audience("API");

            client.TryAddAudience(apiA, _time.GetUtcNow());
            var result = client.TryAddAudience(apiB, _time.GetUtcNow());
            
            Assert.False(result);
            Assert.Single(client.Audiences);
        }

        [Fact]
        public void Throws_WhenAudienceIsNull()
        {
            var client = new ClientBuilder().Build();
            
            Assert.Throws<ArgumentNullException>(()
                => client.TryAddAudience(null!, _time.GetUtcNow()));
        }

        [Fact]
        public void UpdatesUpdatedAt_WhenAudienceIsAdded()
        {
            var client = new ClientBuilder().Build();
            var api = new Audience("api");

            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            var result = client.TryAddAudience(api, expected);
            
            Assert.True(result);
            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void DoesNotUpdateUpdatedAt_WhenAudienceAlreadyExists()
        {
            var now = _time.GetUtcNow();
            var client = new ClientBuilder().Build();
            var api = new Audience("api");
            
            client.TryAddAudience(api, now);
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var before = _time.GetUtcNow();
            var result = client.TryAddAudience(api, before);
            
            Assert.False(result);
            Assert.NotEqual(before, client.UpdatedAt);
        }
    }

    public class TryRemoveAudience : ClientTests
    {
        [Fact]
        public void TryRemoveAudience_RemovesAudience_AndScopes()
        {
            var client = new ClientBuilder().Build();
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            client.TryAddAudience(aud, _time.GetUtcNow());
            client.GrantScopes(aud, [read, write], _time.GetUtcNow());
            
            var isRemoved = client.TryRemoveAudience(aud, _time.GetUtcNow());
            Assert.True(isRemoved);
            Assert.Empty(client.GetAllowedScopes(aud));
            Assert.Empty(client.Audiences);
        }
        
        [Fact]
        public void TryRemoveAudience_ThrowsWhen_Null()
        {
            var client = new ClientBuilder().Build();
            
            Assert.Throws<ArgumentNullException>(()
                => client.TryRemoveAudience(null!, _time.GetUtcNow()));
        }

        [Fact]
        public void TryRemoveAudience_TouchUpdatedAt()
        {
            var now = _time.GetUtcNow();
            var client = new ClientBuilder().Build();
            
            var aud = new Audience("api");
            client.TryAddAudience(aud, now);
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            var isRemoved = client.TryRemoveAudience(aud, expected);
            
            Assert.True(isRemoved);
            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void TryRemoveAudience_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new ClientBuilder().Build();
            var aud = new Audience("api");

            var before = client.UpdatedAt;
            var isRemoved = client.TryRemoveAudience(aud, _time.GetUtcNow());
            
            Assert.False(isRemoved);
            Assert.Equal(before, client.UpdatedAt);
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
            client.Rename(new ClientName("client"), expected);
            
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
        public void AddSecret_Adds_Secret_To_Client()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();

            client.AddSecret(secret, _time.GetUtcNow());

            Assert.Contains(secret, client.Secrets);
        }

        [Fact]
        public void AddSecret_Updates_Timestamp()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.AddSecret(secret, expected);

            Assert.Equal(expected, client.UpdatedAt);
        }

        [Fact]
        public void AddSecret_DoesNot_Update_After_Exception()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();
            client.AddSecret(secret, _time.GetUtcNow());

            var before = client.UpdatedAt;
            Assert.Throws<InvalidOperationException>(() => client.AddSecret(secret, _time.GetUtcNow()));
            Assert.Equal(client.UpdatedAt, before);           
        }
        
        [Fact]
        public void AddSecret_Throws_When_Null()
        {
            var client = new ClientBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => client.AddSecret(null!, _time.GetUtcNow()));
        }
        
        [Fact]
        public void AddSecret_Throws_When_Duplicate_Id()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();

            client.AddSecret(secret, _time.GetUtcNow());

            Assert.Throws<InvalidOperationException>(()
                => client.AddSecret(secret, _time.GetUtcNow()));
        }
    }

    public class RevokeSecret : ClientTests
    {
        [Fact]
        public void RevokeSecret_Revokes_If_Exists()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();
            client.AddSecret(secret, _time.GetUtcNow());

            client.RevokeSecret(secret.Id, _time.GetUtcNow());

            Assert.Contains(secret, client.Secrets);
            Assert.False(secret.IsActive(_time.GetUtcNow()));
        }

        [Fact]
        public void RevokeSecret_DoesNothing_If_NotFound()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();

            client.RevokeSecret(secret.Id, _time.GetUtcNow());
            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RevokeSecret_Updates_Timestamp_If_Exists()
        {
            var now = _time.GetUtcNow();
            var client = new ClientBuilder().Build();
            
            var secret = new ClientSecretBuilder().Build();
            client.AddSecret(secret, now);
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.RevokeSecret(secret.Id, expected);

            Assert.Equal(expected, client.UpdatedAt);
        }
        
        [Fact]
        public void RevokeSecret_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new ClientBuilder().Build();

            var before = client.UpdatedAt;
            client.RevokeSecret(SecretId.New(), _time.GetUtcNow());

            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class RemoveSecret : ClientTests
    {
        [Fact]
        public void RemoveSecret_Removes_If_Exists()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();
            client.AddSecret(secret, _time.GetUtcNow());

            client.RemoveSecret(secret.Id, _time.GetUtcNow());

            Assert.DoesNotContain(secret, client.Secrets);
        }

        [Fact]
        public void RemoveSecret_DoesNothing_If_NotFound()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();

            client.RemoveSecret(secret.Id, _time.GetUtcNow());

            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RemoveSecret_Updates_Timestamp_If_Exists()
        {
            var now = _time.GetUtcNow();
            var client = new ClientBuilder().Build();
            
            var secret = new ClientSecretBuilder().Build();
            client.AddSecret(secret, now);
            
            _time.Advance(TimeSpan.FromSeconds(1));
            var expected = _time.GetUtcNow();
            client.RemoveSecret(secret.Id, expected);

            Assert.Equal(expected, client.UpdatedAt);
        }
        
        [Fact]
        public void RemoveSecret_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new ClientBuilder().Build();

            var before = client.UpdatedAt;
            client.RemoveSecret(SecretId.New(), _time.GetUtcNow());

            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class GetActiveSecrets : ClientTests
    {
        [Fact]
        public void ActiveSecrets_Only_Returns_NonRevoked_And_NonExpired()
        {
            var client = new ClientBuilder().Build();
            
            var active = new ClientSecretBuilder()
                .WithCreatedAt(_time.GetUtcNow())
                .WithLifetime(TimeSpan.FromDays(30)).Build();
            var expired = new ClientSecretBuilder()
                .WithCreatedAt(_time.GetUtcNow())
                .WithLifetime(TimeSpan.FromMinutes(10))
                .Build();
            var revoked = new ClientSecretBuilder()
                .WithCreatedAt(_time.GetUtcNow())
                .Build();
            revoked.Revoke(_time.GetUtcNow());

            client.AddSecret(active, _time.GetUtcNow());
            client.AddSecret(expired, _time.GetUtcNow());
            client.AddSecret(revoked, _time.GetUtcNow());

            _time.Advance(TimeSpan.FromDays(10));
            var result = client.ActiveSecrets(_time.GetUtcNow()).ToList();

            Assert.Contains(active, result);
            Assert.DoesNotContain(expired, result);
            Assert.DoesNotContain(revoked, result);
        }

        [Fact]
        public void Revoke_Secret_Changes_ActiveSecrets_Result()
        {
            var client = new ClientBuilder().Build();
            var secret = new ClientSecretBuilder().Build();
            client.AddSecret(secret, _time.GetUtcNow());

            Assert.Contains(secret, client.ActiveSecrets(_time.GetUtcNow()));

            secret.Revoke(_time.GetUtcNow());

            Assert.DoesNotContain(secret, client.ActiveSecrets(_time.GetUtcNow()));
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