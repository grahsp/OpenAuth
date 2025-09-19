using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Unit.Entities;

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
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");

            client.TryAddAudience(aud);
            client.GrantScopes(aud, read, write, write);

            var scopes = client.GetAllowedScopes(aud);
            Assert.Equal(2, scopes.Count);
            Assert.Contains(read, scopes);
            Assert.Contains(write, scopes);
            Assert.Contains(aud, client.Audiences);
        }

        [Fact]
        public void GrantScopes_TouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");

            var before = client.UpdatedAt;

            client.TryAddAudience(aud);
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
            
            client.TryAddAudience(aud);
            client.GrantScopes(aud, read, write);

            client.RevokeScopes(aud, read);

            var scopes = client.GetAllowedScopes(aud);
            Assert.Single(scopes);
            Assert.Contains(write, scopes);
        }

        [Fact]
        public void RevokeScopes_TouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            
            client.TryAddAudience(aud);
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
            client.TryAddAudience(aud);

            var before = client.UpdatedAt;
            client.RevokeScopes(aud, new Scope("does-not-exist"));
        
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class TryAddAudience
    {
        [Fact]
        public void ReturnsTrue_WhenAudienceDoesNotExists()
        {
            var client = new Client("App");
            var api = new Audience("api");

            var result = client.TryAddAudience(api);
            
            Assert.True(result);
            Assert.Contains(api, client.Audiences);
        }
        
        [Fact]
        public void ReturnsFalse_WhenAudienceAlreadyExists()
        {
            var client = new Client("App");
            
            // Ensure adding is not done by reference
            var apiA = new Audience("api");
            var apiB = new Audience("api");

            client.TryAddAudience(apiA);
            var result = client.TryAddAudience(apiB);
            
            Assert.False(result);
            Assert.Single(client.Audiences);
        }
        
        [Fact]
        public void ReturnsFalse_WhenAudienceExistWithDifferentCasing()
        {
            var client = new Client("App");
            
            // Ensure adding is not done by reference
            var apiA = new Audience("api");
            var apiB = new Audience("API");

            client.TryAddAudience(apiA);
            var result = client.TryAddAudience(apiB);
            
            Assert.False(result);
            Assert.Single(client.Audiences);
        }

        [Fact]
        public void Throws_WhenAudienceIsNull()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentNullException>(() => client.TryAddAudience(null!));
        }

        [Fact]
        public void UpdatesUpdatedAt_WhenAudienceIsAdded()
        {
            var client = new Client("App");
            var api = new Audience("api");

            var before = client.UpdatedAt;
            var result = client.TryAddAudience(api);
            
            Assert.True(result);
            Assert.NotEqual(before, client.UpdatedAt);
        }

        [Fact]
        public void DoesNotUpdateUpdatedAt_WhenAudienceAlreadyExists()
        {
            var client = new Client("App");
            var api = new Audience("api");
            
            client.TryAddAudience(api);
            
            var before = client.UpdatedAt;
            var result = client.TryAddAudience(api);
            
            Assert.False(result);
            Assert.Equal(before, client.UpdatedAt);
        }
    }

    public class TryRemoveAudience
    {
        [Fact]
        public void TryRemoveAudience_RemovesAudience_AndScopes()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            var read = new Scope("read");
            var write = new Scope("write");
            client.TryAddAudience(aud);
            client.GrantScopes(aud, read, write);
            
            var isRemoved = client.TryRemoveAudience(aud);
            Assert.True(isRemoved);
            Assert.Empty(client.GetAllowedScopes(aud));
            Assert.Empty(client.Audiences);
        }
        
        [Fact]
        public void TryRemoveAudience_ThrowsWhen_Null()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentNullException>(() => client.TryRemoveAudience(null!));
        }

        [Fact]
        public void TryRemoveAudience_TouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");
            client.TryAddAudience(aud);
            
            var before = client.UpdatedAt;
            var isRemoved = client.TryRemoveAudience(aud);
            
            Assert.True(isRemoved);
            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void TryRemoveAudience_NoOp_DoesNotTouchUpdatedAt()
        {
            var client = new Client("App");
            var aud = new Audience("api");

            var before = client.UpdatedAt;
            var isRemoved = client.TryRemoveAudience(aud);
            
            Assert.False(isRemoved);
            Assert.Equal(before, client.UpdatedAt);
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

    public class RevokeSecret
    {
        [Fact]
        public void RevokeSecret_Revokes_If_Exists()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret);

            client.RevokeSecret(secret.Id);

            Assert.Contains(secret, client.Secrets);
            Assert.False(secret.IsActive());
        }

        [Fact]
        public void RevokeSecret_DoesNothing_If_NotFound()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));

            client.RevokeSecret(secret.Id);
            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RevokeSecret_Updates_Timestamp_If_Exists()
        {
            var client = new Client("App");
            var secret = new ClientSecret(new SecretHash("v1$1$abc$xyz"));
            client.AddSecret(secret);
            
            var before = client.UpdatedAt;
            client.RevokeSecret(secret.Id);

            Assert.True(client.UpdatedAt > before);
        }
        
        [Fact]
        public void RevokeSecret_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new Client("App");

            var before = client.UpdatedAt;
            client.RevokeSecret(SecretId.New());

            Assert.Equal(client.UpdatedAt, before);
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

            client.RemoveSecret(secret.Id);

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
    
    public class AddSigningKey
    {
        [Fact]
        public void AddSigningKey_Adds_Secret_To_Client()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");

            client.AddSigningKey(signingKey);

            Assert.Contains(signingKey, client.SigningKeys);
        }

        [Fact]
        public void AddSigningKey_Updates_Timestamp()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            
            var before = client.UpdatedAt;
            client.AddSigningKey(signingKey);

            Assert.True(client.UpdatedAt > before);
        }

        [Fact]
        public void AddSigningKey_DoesNot_Update_After_Exception()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            client.AddSigningKey(signingKey);

            var before = client.UpdatedAt;
            Assert.Throws<InvalidOperationException>(() => client.AddSigningKey(signingKey));
            Assert.Equal(client.UpdatedAt, before);           
        }
        
        [Fact]
        public void AddSigningKey_Throws_When_Null()
        {
            var client = new Client("App");
            Assert.Throws<ArgumentNullException>(() => client.AddSigningKey(null!));
        }
        
        [Fact]
        public void AddSigningKey_Throws_When_Duplicate_Id()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");

            client.AddSigningKey(signingKey);

            Assert.Throws<InvalidOperationException>(() => client.AddSigningKey(signingKey));
        }
    }
    
    public class RevokeSigningKey
    {
        [Fact]
        public void RevokeSigningKey_Revokes_If_Exists()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            client.AddSigningKey(signingKey);

            client.RevokeSigningKey(signingKey.KeyId);

            Assert.Contains(signingKey, client.SigningKeys);
            Assert.False(signingKey.IsActive());
        }

        [Fact]
        public void RevokeSigningKey_DoesNothing_If_NotFound()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");

            client.RevokeSigningKey(signingKey.KeyId);
            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RevokeSigningKey_Updates_Timestamp_If_Exists()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            
            client.AddSigningKey(signingKey);
            var before = client.UpdatedAt;
            client.RevokeSigningKey(signingKey.KeyId);

            Assert.True(client.UpdatedAt > before);
        }
        
        [Fact]
        public void RevokeSigningKey_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new Client("App");

            var before = client.UpdatedAt;
            client.RemoveSigningKey(SigningKeyId.New());

            Assert.Equal(client.UpdatedAt, before);
        }
    }

    public class RemoveSigningKey
    {
        [Fact]
        public void RemoveSigningKey_Removes_If_Exists()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            client.AddSigningKey(signingKey);

            client.RemoveSigningKey(signingKey.KeyId);

            Assert.DoesNotContain(signingKey, client.SigningKeys);
        }

        [Fact]
        public void RemoveSigningKey_DoesNothing_If_NotFound()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");

            client.RemoveSigningKey(signingKey.KeyId);
            Assert.Empty(client.Secrets);
        }
        
        [Fact]
        public void RemoveSigningKey_Updates_Timestamp_If_Exists()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            
            client.AddSigningKey(signingKey);
            var before = client.UpdatedAt;
            client.RemoveSigningKey(signingKey.KeyId);

            Assert.True(client.UpdatedAt > before);
        }
        
        [Fact]
        public void RemoveSigningKey_DoesNot_Update_Timestamp_If_NotFound()
        {
            var client = new Client("App");

            var before = client.UpdatedAt;
            client.RemoveSigningKey(SigningKeyId.New());

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
    
    public class GetActiveSigningKeys
    {
        [Fact]
        public void ActiveSigningKeys_Only_Returns_NonRevoked_And_NonExpired()
        {
            var client = new Client("App");
            var active = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            var expired = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key", DateTime.UtcNow.AddMinutes(-1));
            var revoked = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            revoked.Revoke();

            client.AddSigningKey(active);
            client.AddSigningKey(expired);
            client.AddSigningKey(revoked);

            var result = client.ActiveSigningKeys().ToList();

            Assert.Contains(active, result);
            Assert.DoesNotContain(expired, result);
            Assert.DoesNotContain(revoked, result);
        }

        [Fact]
        public void Revoke_SigningKey_Changes_ActiveSecrets_Result()
        {
            var client = new Client("App");
            var signingKey = SigningKey.CreateSymmetric(SigningAlgorithm.Hmac, "secret-key");
            client.AddSigningKey(signingKey);

            Assert.Contains(signingKey, client.ActiveSigningKeys());

            signingKey.Revoke();

            Assert.DoesNotContain(signingKey, client.ActiveSigningKeys());
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