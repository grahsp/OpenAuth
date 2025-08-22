using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Domain.Tests.Entities;

public class ClientTests
{
    [Fact]
    public void Constructor_SetsDefaults()
    {
        const string clientName = "App";
        var before = DateTime.UtcNow;
        var client = new Client(clientName);
        var after = DateTime.UtcNow;

        Assert.Equal(clientName, client.Name);
        Assert.True(client.Enabled);
        Assert.NotEqual(TimeSpan.Zero, client.TokenLifetime);

        Assert.NotEqual(Guid.Empty, client.Id.Value);
        Assert.Empty(client.GetAudiences);

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
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");

            client.GrantScopes(aud, read, write, write);

            var scopes = client.GetAllowedScopes(aud);
            Assert.Equal(2, scopes.Count);
            Assert.Contains(read, scopes);
            Assert.Contains(write, scopes);
            Assert.Contains(aud, client.GetAudiences);
        }

        [Fact]
        public void GrantScopes_TouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");

            var before = client.UpdatedAt;

            client.GrantScopes(aud, read);

            Assert.True(client.UpdatedAt > before);
        }
    }

    public class RevokeScopes
    {
        [Fact]
        public void RevokeScopes_RemovesScopes_RemoveAllButOneScope_AudienceRemains()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            client.GrantScopes(aud, read, write);

            client.RevokeScopes(aud, read);

            var scopes = client.GetAllowedScopes(aud);
            Assert.Single(scopes);
            Assert.Contains(write, scopes);
        }

        [Fact]
        public void RevokeScopes_RemoveAudience_When_NoScopes()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            client.GrantScopes(aud, read, write);
        
            client.RevokeScopes(aud, read, write);

            var scopes = client.GetAllowedScopes(aud);
            Assert.Empty(scopes);
            Assert.DoesNotContain(aud, client.GetAudiences);
        }

        [Fact]
        public void RevokeScopes_TouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            client.GrantScopes(aud, read);
        
            var before = client.UpdatedAt;
            client.RevokeScopes(aud, read);

            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void RevokeScopes_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("unknown");

            var before = client.UpdatedAt;
            client.RevokeScopes(aud, new Scope("does-not-exist"));
        
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class RemoveAudience
    {
        [Fact]
        public void RemoveAudience_RemovesAudience_AndScopes()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            client.GrantScopes(aud, read, write);
            
            client.RemoveAudience(aud);
            Assert.Empty(client.GetAllowedScopes(aud));
            Assert.Empty(client.GetAudiences);
        }

        [Fact]
        public void RemoveAudience_TouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            client.GrantScopes(aud);
            
            var before = client.UpdatedAt;
            client.RemoveAudience(aud);
            
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void RemoveAudience_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");

            var before = client.UpdatedAt;
            client.RemoveAudience(aud);
            
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class PolicyMap
    {
        [Fact]
        public void PolicyMap_Getter_ReturnsStrings_And_DistinctScopes()
        {
            const string api = "api";
            const string billing = "billing";
            const string read = "read";
            const string write = "write";

            var client = new Client("App");
            client.GrantScopes(new Audience(api), new Scope(read), new Scope(write));
            client.GrantScopes(new Audience(billing), new Scope(read));

            var snapshot = client.Grants;

            Assert.Equal(2, snapshot.Count);
            Assert.Contains(api, snapshot.Keys);
            Assert.Contains(billing, snapshot.Keys);

            var apiScopes = snapshot[api];
            Assert.Equal(2, apiScopes.Count);
            Assert.Contains(read, apiScopes);
            Assert.Contains(write, apiScopes); 
            
            var billingScopes = snapshot[billing];
            Assert.Single(billingScopes);
            Assert.Contains(read, billingScopes);
        }

        [Fact]
        public void PolicyMap_Setter_SetsInternalAudiencesAndScopes()
        {
            var api = new Audience("api");
            var billing = new Audience("billing");
            var read = new Scope("read");
            var write = new Scope("write");
            
            var original = new Client("Original");
            original.GrantScopes(api, read, write);
            original.GrantScopes(new Audience("billing"), read);

            var snapshot = original.Grants;
            var restored = new Client("Restored") { Grants = snapshot };
            
            Assert.Equal(2, restored.GetAudiences.Count);
            Assert.Contains(api, restored.GetAudiences);
            Assert.Contains(billing, restored.GetAudiences);

            var apiScopes = restored.GetAllowedScopes(api);
            Assert.Equal(2, apiScopes.Count);
            Assert.Contains(read, apiScopes);
            Assert.Contains(write, apiScopes);

            var billingScopes = restored.GetAllowedScopes(billing);
            Assert.Single(billingScopes);
            Assert.Contains(read, billingScopes);
        }
    }

    public class Rename
    {
        [Fact]
        public void Rename_UpdateName_WhenValid()
        {
            var client = new Client("App");
            const string name = "New App";
            
            client.Rename(name);
            Assert.Equal(name, client.Name);
        }
        
        [Fact]
        public void Rename_ThrowsWhen_Null()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentNullException>(() =>
                client.Rename(null!));
        }

        [Fact]
        public void Rename_ThrowsWhen_ValueTooShort()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                client.Rename("A"));
        }
        
        [Fact]
        public void Rename_ThrowsWhen_ValueTooLong()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                client.Rename(new string('a', 64)));
        }
        
        [Fact]
        public void Rename_ThrowsWhen_ValueWhitespace()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentException>(() =>
                client.Rename(new string(' ', 16)));
        }

        [Fact]
        public void Rename_TouchUpdatedAt()
        {
            var client = new Client("App");
            var before = client.UpdatedAt;
            
            client.Rename("Anything");
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void Rename_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            var before = client.UpdatedAt;
            
            client.Rename(client.Name);
            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class SetTokenLifetime
    {
        [Fact]
        public void SetTokenLifetime_SetTokenLifetime_WhenValid()
        {
            var client = new Client("App");
            var newLifetime = TimeSpan.FromSeconds(1);
            
            client.SetTokenLifetime(newLifetime);
            
            Assert.Equal(newLifetime, client.TokenLifetime);
        }

        [Fact]
        public void SetTokenLifetime_ThrowsOn_ZeroValue()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentOutOfRangeException>(() => client.SetTokenLifetime(TimeSpan.Zero));
        }

        [Fact]
        public void SetTokenLifetime_TouchUpdatedAt()
        {
            var client = new Client("App");
            
            var before = client.UpdatedAt;
            client.SetTokenLifetime(TimeSpan.FromSeconds(1));
            
            Assert.True(client.UpdatedAt > before);           
        }

        [Fact]
        public void SetTokenLifetime_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            
            var before = client.UpdatedAt;
            client.SetTokenLifetime(client.TokenLifetime);
            
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class AddSecret
    {
        [Fact]
        public void AddSecret_Adds_Secret_To_Client()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.AddSecret(secret);

            Assert.Contains(secret, client.Secrets);
        }

        [Fact]
        public void AddSecret_Updates_Timestamp()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            
            var before = client.UpdatedAt;
            client.AddSecret(secret);

            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void AddSecret_DoesNot_Update_After_Exception()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret);

            var before = client.UpdatedAt;
            Assert.Throws<InvalidOperationException>(() => client.AddSecret(secret));
            Assert.Equal(client.UpdatedAt, before);           
        }
        
        [Fact]
        public void AddSecret_Throws_When_Null()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentNullException>(() => client.AddSecret(null!));
        }
        
        [Fact]
        public void AddSecret_Throws_When_Duplicate_Id()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.AddSecret(secret);

            Assert.Throws<InvalidOperationException>(() => client.AddSecret(secret));
        }
    }

    public class RemoveSecret
    {
        [Fact]
        public void RemoveSecret_Removes_If_Exists()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret);

            client.RemoveSecret(secret.Id);

            Assert.DoesNotContain(secret, client.Secrets);
        }

        [Fact]
        public void RemoveSecret_DoesNothing_If_NotFound()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.RemoveSecret(secret.Id); // should not throw

            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RemoveSecret_Updates_Timestamp_If_Exists()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret);
            
            var before = client.UpdatedAt;
            client.RemoveSecret(secret.Id);

            Assert.True(client.UpdatedAt > before);
        }
        
        [Fact]
        public void RemoveSecret_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new Client("App");

            var before = client.UpdatedAt;
            client.RemoveSecret(SecretId.New());

            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class GetActiveSecrets
    {
        [Fact]
        public void ActiveSecrets_Only_Returns_NonRevoked_And_NonExpired()
        {
            var client = new Client("App");
            var active = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
            var expired = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(-5));
            var revoked = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
            revoked.Revoke();

            client.AddSecret(active);
            client.AddSecret(expired);
            client.AddSecret(revoked);

            var result = client.ActiveSecrets().ToList();

            Assert.Contains(active, result);
            Assert.DoesNotContain(expired, result);
            Assert.DoesNotContain(revoked, result);
        }

        [Fact]
        public void Revoke_Secret_Changes_ActiveSecrets_Result()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"), DateTime.UtcNow.AddMinutes(5));
            client.AddSecret(secret);

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
            var client = new Client("App");
            
            var before = client.UpdatedAt;
            client.Disable();
            Assert.False(client.Enabled);
            Assert.True(client.UpdatedAt > before);

            before = client.UpdatedAt;
            client.Enable();
            Assert.True(client.Enabled);
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void Enable_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            var before = client.UpdatedAt;

            client.Enable();
            Assert.Equal(before, client.UpdatedAt);           
        }

        [Fact]
        public void Disable_NoOup_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            client.Disable();
            
            var before = client.UpdatedAt;
            client.Disable();
            Assert.Equal(before, client.UpdatedAt);
        }
    }
}