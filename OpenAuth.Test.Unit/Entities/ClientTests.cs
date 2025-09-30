using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Unit.Entities;

public class ClientTests
{
    private static readonly TimeProvider Time = new FakeTimeProvider();
    
    
    [Fact]
    public void Constructor_SetsDefaults()
    {
        const string clientName = "App";
        var before = DateTime.UtcNow;
        var client = new Client(clientName, Time.GetUtcNow());
        var after = DateTime.UtcNow;

        Assert.Equal(clientName, client.Name);
        Assert.True(client.Enabled);
        Assert.NotEqual(TimeSpan.Zero, client.TokenLifetime);

        Assert.NotEqual(Guid.Empty, client.Id.Value);
        Assert.Empty(client.Audiences);

        // created/updated should be set near "now"
        Assert.InRange(client.CreatedAt, before, after);
        Assert.InRange(client.UpdatedAt, before, after);

        // updated should not be earlier than created
        Assert.True(client.UpdatedAt >= client.CreatedAt);
    }

    public class GrantScopes
    {
        [Fact]
        public void GrantScopes_AddsScopes_AddScopes_NoDuplicates()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");

            client.TryAddAudience(aud, Time.GetUtcNow());
            client.GrantScopes(aud, [read, write, write], Time.GetUtcNow());

            var scopes = client.GetAllowedScopes(aud);
            Assert.Equal(2, scopes.Count);
            Assert.Contains(read, scopes);
            Assert.Contains(write, scopes);
            Assert.Contains(aud, client.Audiences);
        }

        [Fact]
        public void GrantScopes_TouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");
            var read = new Scope("read");

            var before = client.UpdatedAt;

            client.TryAddAudience(aud, Time.GetUtcNow());
            client.GrantScopes(aud, [read], Time.GetUtcNow());

            Assert.True(client.UpdatedAt > before);
        }
    }

    public class RevokeScopes
    {
        [Fact]
        public void RevokeScopes_RemovesScopes_RemoveAllButOneScope_AudienceRemains()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            
            client.TryAddAudience(aud, Time.GetUtcNow());
            client.GrantScopes(aud, [read, write], Time.GetUtcNow());

            client.RevokeScopes(aud, [read], Time.GetUtcNow());

            var scopes = client.GetAllowedScopes(aud);
            Assert.Single(scopes);
            Assert.Contains(write, scopes);
        }

        [Fact]
        public void RevokeScopes_TouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");
            var read = new Scope("read");
            
            client.TryAddAudience(aud, Time.GetUtcNow());
            client.GrantScopes(aud, [read], Time.GetUtcNow());
        
            var before = client.UpdatedAt;
            client.RevokeScopes(aud, [read], Time.GetUtcNow());

            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void RevokeScopes_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("unknown");
            client.TryAddAudience(aud, Time.GetUtcNow());

            var before = client.UpdatedAt;
            client.RevokeScopes(aud, [new Scope("does-not-exist")], Time.GetUtcNow());
        
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class TryAddAudience
    {
        [Fact]
        public void ReturnsTrue_WhenAudienceDoesNotExists()
        {
            var client = new Client("App", Time.GetUtcNow());
            var api = new Audience("api");

            var result = client.TryAddAudience(api, Time.GetUtcNow());
            
            Assert.True(result);
            Assert.Contains(api, client.Audiences);
        }
        
        [Fact]
        public void ReturnsFalse_WhenAudienceAlreadyExists()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            // Ensure adding is not done by reference
            var apiA = new Audience("api");
            var apiB = new Audience("api");

            client.TryAddAudience(apiA, Time.GetUtcNow());
            var result = client.TryAddAudience(apiB, Time.GetUtcNow());
            
            Assert.False(result);
            Assert.Single(client.Audiences);
        }
        
        [Fact]
        public void ReturnsFalse_WhenAudienceExistWithDifferentCasing()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            // Ensure adding is not done by reference
            var apiA = new Audience("api");
            var apiB = new Audience("API");

            client.TryAddAudience(apiA, Time.GetUtcNow());
            var result = client.TryAddAudience(apiB, Time.GetUtcNow());
            
            Assert.False(result);
            Assert.Single(client.Audiences);
        }

        [Fact]
        public void Throws_WhenAudienceIsNull()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            Assert.Throws<ArgumentNullException>(()
                => client.TryAddAudience(null!, Time.GetUtcNow()));
        }

        [Fact]
        public void UpdatesUpdatedAt_WhenAudienceIsAdded()
        {
            var client = new Client("App", Time.GetUtcNow());
            var api = new Audience("api");

            var before = client.UpdatedAt;
            var result = client.TryAddAudience(api, Time.GetUtcNow());
            
            Assert.True(result);
            Assert.NotEqual(before, client.UpdatedAt);
        }

        [Fact]
        public void DoesNotUpdateUpdatedAt_WhenAudienceAlreadyExists()
        {
            var client = new Client("App", Time.GetUtcNow());
            var api = new Audience("api");
            
            client.TryAddAudience(api, Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            var result = client.TryAddAudience(api, Time.GetUtcNow());
            
            Assert.False(result);
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class TryRemoveAudience
    {
        [Fact]
        public void TryRemoveAudience_RemovesAudience_AndScopes()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            client.TryAddAudience(aud, Time.GetUtcNow());
            client.GrantScopes(aud, [read, write], Time.GetUtcNow());
            
            var isRemoved = client.TryRemoveAudience(aud, Time.GetUtcNow());
            Assert.True(isRemoved);
            Assert.Empty(client.GetAllowedScopes(aud));
            Assert.Empty(client.Audiences);
        }
        
        [Fact]
        public void TryRemoveAudience_ThrowsWhen_Null()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            Assert.Throws<ArgumentNullException>(()
                => client.TryRemoveAudience(null!, Time.GetUtcNow()));
        }

        [Fact]
        public void TryRemoveAudience_TouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");
            client.TryAddAudience(aud, Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            var isRemoved = client.TryRemoveAudience(aud, Time.GetUtcNow());
            
            Assert.True(isRemoved);
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void TryRemoveAudience_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var aud = new Audience("api");

            var before = client.UpdatedAt;
            var isRemoved = client.TryRemoveAudience(aud, Time.GetUtcNow());
            
            Assert.False(isRemoved);
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class Rename
    {
        [Fact]
        public void Rename_UpdateName_WhenValid()
        {
            var client = new Client("App", Time.GetUtcNow());
            const string name = "New App";
            
            client.Rename(name, Time.GetUtcNow());
            Assert.Equal(name, client.Name);
        }
        
        [Fact]
        public void Rename_ThrowsWhen_Null()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            Assert.Throws<ArgumentNullException>(() =>
                client.Rename(null!, Time.GetUtcNow()));
        }

        [Fact]
        public void Rename_ThrowsWhen_ValueTooShort()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                client.Rename("A", Time.GetUtcNow()));
        }
        
        [Fact]
        public void Rename_ThrowsWhen_ValueTooLong()
        {
            var client = new Client("App", Time.GetUtcNow());
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                client.Rename(new string('a', 64), Time.GetUtcNow()));
        }
        
        [Fact]
        public void Rename_ThrowsWhen_ValueWhitespace()
        {
            var client = new Client("App", Time.GetUtcNow());
            Assert.Throws<ArgumentException>(() =>
                client.Rename(new string(' ', 16), Time.GetUtcNow()));
        }

        [Fact]
        public void Rename_TouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var before = client.UpdatedAt;
            
            client.Rename("Anything", Time.GetUtcNow());
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void Rename_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var before = client.UpdatedAt;
            
            client.Rename(client.Name, Time.GetUtcNow());
            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class SetTokenLifetime
    {
        [Fact]
        public void SetTokenLifetime_SetTokenLifetime_WhenValid()
        {
            var client = new Client("App", Time.GetUtcNow());
            var newLifetime = TimeSpan.FromSeconds(1);
            
            client.SetTokenLifetime(newLifetime, Time.GetUtcNow());
            
            Assert.Equal(newLifetime, client.TokenLifetime);
        }

        [Fact]
        public void SetTokenLifetime_ThrowsOn_ZeroValue()
        {
            var client = new Client("App", Time.GetUtcNow());
            Assert.Throws<ArgumentOutOfRangeException>(()
                => client.SetTokenLifetime(TimeSpan.Zero, Time.GetUtcNow()));
        }

        [Fact]
        public void SetTokenLifetime_TouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.SetTokenLifetime(TimeSpan.FromSeconds(1), Time.GetUtcNow());
            
            Assert.True(client.UpdatedAt > before);           
        }

        [Fact]
        public void SetTokenLifetime_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.SetTokenLifetime(client.TokenLifetime, Time.GetUtcNow());
            
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class AddSecret
    {
        [Fact]
        public void AddSecret_Adds_Secret_To_Client()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.AddSecret(secret, Time.GetUtcNow());

            Assert.Contains(secret, client.Secrets);
        }

        [Fact]
        public void AddSecret_Updates_Timestamp()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            
            var before = client.UpdatedAt;
            client.AddSecret(secret, Time.GetUtcNow());

            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void AddSecret_DoesNot_Update_After_Exception()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret, Time.GetUtcNow());

            var before = client.UpdatedAt;
            Assert.Throws<InvalidOperationException>(() => client.AddSecret(secret, Time.GetUtcNow()));
            Assert.Equal(client.UpdatedAt, before);           
        }
        
        [Fact]
        public void AddSecret_Throws_When_Null()
        {
            var client = new Client("App", Time.GetUtcNow());
            Assert.Throws<ArgumentNullException>(() => client.AddSecret(null!, Time.GetUtcNow()));
        }
        
        [Fact]
        public void AddSecret_Throws_When_Duplicate_Id()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.AddSecret(secret, Time.GetUtcNow());

            Assert.Throws<InvalidOperationException>(()
                => client.AddSecret(secret, Time.GetUtcNow()));
        }
    }

    public class RevokeSecret
    {
        [Fact]
        public void RevokeSecret_Revokes_If_Exists()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret, Time.GetUtcNow());

            client.RevokeSecret(secret.Id, Time.GetUtcNow());

            Assert.Contains(secret, client.Secrets);
            Assert.False(secret.IsActive());
        }

        [Fact]
        public void RevokeSecret_DoesNothing_If_NotFound()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.RevokeSecret(secret.Id, Time.GetUtcNow());
            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RevokeSecret_Updates_Timestamp_If_Exists()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret, Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.RevokeSecret(secret.Id, Time.GetUtcNow());

            Assert.True(client.UpdatedAt > before);
        }
        
        [Fact]
        public void RevokeSecret_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new Client("App", Time.GetUtcNow());

            var before = client.UpdatedAt;
            client.RevokeSecret(SecretId.New(), Time.GetUtcNow());

            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class RemoveSecret
    {
        [Fact]
        public void RemoveSecret_Removes_If_Exists()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret, Time.GetUtcNow());

            client.RemoveSecret(secret.Id, Time.GetUtcNow());

            Assert.DoesNotContain(secret, client.Secrets);
        }

        [Fact]
        public void RemoveSecret_DoesNothing_If_NotFound()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.RemoveSecret(secret.Id, Time.GetUtcNow());

            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RemoveSecret_Updates_Timestamp_If_Exists()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret, Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.RemoveSecret(secret.Id, Time.GetUtcNow());

            Assert.True(client.UpdatedAt > before);
        }
        
        [Fact]
        public void RemoveSecret_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new Client("App", Time.GetUtcNow());

            var before = client.UpdatedAt;
            client.RemoveSecret(SecretId.New(), Time.GetUtcNow());

            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class GetActiveSecrets
    {
        [Fact]
        public void ActiveSecrets_Only_Returns_NonRevoked_And_NonExpired()
        {
            var client = new Client("App", Time.GetUtcNow());
            var active = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
            var expired = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(-5));
            var revoked = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
            revoked.Revoke();

            client.AddSecret(active, Time.GetUtcNow());
            client.AddSecret(expired, Time.GetUtcNow());
            client.AddSecret(revoked, Time.GetUtcNow());

            var result = client.ActiveSecrets().ToList();

            Assert.Contains(active, result);
            Assert.DoesNotContain(expired, result);
            Assert.DoesNotContain(revoked, result);
        }

        [Fact]
        public void Revoke_Secret_Changes_ActiveSecrets_Result()
        {
            var client = new Client("App", Time.GetUtcNow());
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
            client.AddSecret(secret, Time.GetUtcNow());

            Assert.Contains(secret, client.ActiveSecrets());

            secret.Revoke();

            Assert.DoesNotContain(secret, client.ActiveSecrets());
        } 
    }

    public class Enabled
    {
        [Fact]
        public void EnableAndDisable_TogglesEnable_And_TouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.Disable(Time.GetUtcNow());
            Assert.False(client.Enabled);
            Assert.True(client.UpdatedAt > before);

            before = client.UpdatedAt;
            client.Enable(Time.GetUtcNow());
            Assert.True(client.Enabled);
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void Enable_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            var before = client.UpdatedAt;

            client.Enable(Time.GetUtcNow());
            Assert.Equal(before, client.UpdatedAt);           
        }

        [Fact]
        public void Disable_NoOup_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App", Time.GetUtcNow());
            client.Disable(Time.GetUtcNow());
            
            var before = client.UpdatedAt;
            client.Disable(Time.GetUtcNow());
            Assert.Equal(before, client.UpdatedAt);
        }
    }
}