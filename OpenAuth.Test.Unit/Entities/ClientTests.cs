using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
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

   public class AddAudience : ClientTests
    {
        [Fact]
        public void AddAudience_WithNewName_AddsAudienceToCollection()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            // Act
            var audience = client.AddAudience(audienceName, _time.GetUtcNow());

            // Assert
            Assert.Single(client.Audiences);
            Assert.Contains(client.Audiences, a => a.Name == audienceName);
            Assert.Equal(audienceName, audience.Name);
        }

        [Fact]
        public void AddAudience_WithExistingName_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                client.AddAudience(audienceName, _time.GetUtcNow()));

            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public void AddAudience_IsCaseInsensitive()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            client.AddAudience(new AudienceName("api"), _time.GetUtcNow());

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                client.AddAudience(new AudienceName("API"), _time.GetUtcNow()));

            Assert.Contains("already exists", exception.Message);
        }

        [Fact]
        public void AddAudience_UpdatesUpdatedAt()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            client.AddAudience(audienceName, expectedTime);

            // Assert
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void AddAudience_ReturnsCreatedAudience()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var now = _time.GetUtcNow();

            // Act
            var audience = client.AddAudience(audienceName, now);

            // Assert
            Assert.NotNull(audience);
            Assert.Equal(audienceName, audience.Name);
            Assert.Equal(now, audience.CreatedAt);
            Assert.Equal(now, audience.UpdatedAt);
            Assert.Empty(audience.Scopes);
        }
    }

    public class RemoveAudience : ClientTests
    {
        [Fact]
        public void RemoveAudience_WithExistingAudience_RemovesFromCollection()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            client.RemoveAudience(audienceName, _time.GetUtcNow());

            // Assert
            Assert.Empty(client.Audiences);
        }

        [Fact]
        public void RemoveAudience_WithNonExistentAudience_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                client.RemoveAudience(audienceName, _time.GetUtcNow()));

            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public void RemoveAudience_UpdatesUpdatedAt()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            client.RemoveAudience(audienceName, expectedTime);

            // Assert
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void RemoveAudience_RemovesAllAssociatedScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var scopes = new[] { new Scope("read"), new Scope("write") };
            
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Act
            client.RemoveAudience(audienceName, _time.GetUtcNow());

            // Assert
            Assert.Empty(client.Audiences);
        }
    }

    public class SetScopes : ClientTests
    {
        [Fact]
        public void SetScopes_ReplacesAllExistingScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, new[] { new Scope("read"), new Scope("write") }, _time.GetUtcNow());

            // Act
            var newScopes = new[] { new Scope("admin"), new Scope("delete") };
            var audience = client.SetScopes(audienceName, newScopes, _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
            Assert.Contains(audience.Scopes, s => s.Value == "admin");
            Assert.Contains(audience.Scopes, s => s.Value == "delete");
            Assert.DoesNotContain(audience.Scopes, s => s.Value == "read");
            Assert.DoesNotContain(audience.Scopes, s => s.Value == "write");
        }

        [Fact]
        public void SetScopes_WithEmptyCollection_ClearsAllScopes()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, new[] { new Scope("read"), new Scope("write") }, _time.GetUtcNow());

            // Act
            var audience = client.SetScopes(audienceName, Array.Empty<Scope>(), _time.GetUtcNow());

            // Assert
            Assert.Empty(audience.Scopes);
        }

        [Fact]
        public void SetScopes_WithNonExistentAudience_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                client.SetScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow()));

            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public void SetScopes_UpdatesClientUpdatedAt()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            client.SetScopes(audienceName, new[] { new Scope("read") }, expectedTime);

            // Assert
            Assert.Equal(expectedTime, client.UpdatedAt);
        }
    }

    public class GrantScopes : ClientTests
    {
        [Fact]
        public void GrantScopes_WithNewScopes_AddsToAudience()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var scopes = new[] { new Scope("read"), new Scope("write") };
            
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            var audience = client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
            Assert.Contains(audience.Scopes, s => s.Value == "write");
        }

        [Fact]
        public void GrantScopes_WithDuplicateScopes_IgnoresDuplicates()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var scopes = new[] { new Scope("read"), new Scope("write"), new Scope("read") };
            
            client.AddAudience(audienceName, _time.GetUtcNow());

            // Act
            var audience = client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Assert - HashSet prevents duplicates
            Assert.Equal(2, audience.Scopes.Count);
        }

        [Fact]
        public void GrantScopes_WithExistingScopes_IsIdempotent()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow());

            // Act - grant same scope again
            var audience = client.GrantScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
        }

        [Fact]
        public void GrantScopes_WithNonExistentAudience_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                client.GrantScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow()));

            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public void GrantScopes_UpdatesClientUpdatedAt()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            client.GrantScopes(audienceName, new[] { new Scope("read") }, expectedTime);

            // Assert
            Assert.Equal(expectedTime, client.UpdatedAt);
        }

        [Fact]
        public void GrantScopes_UpdatesAudienceUpdatedAt()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var audience = client.AddAudience(audienceName, _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            client.GrantScopes(audienceName, new[] { new Scope("read") }, expectedTime);

            // Assert
            Assert.Equal(expectedTime, audience.UpdatedAt);
        }
    }

    public class RevokeScopes : ClientTests
    {
        [Fact]
        public void RevokeScopes_WithExistingScopes_RemovesFromAudience()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var scopes = new[] { new Scope("read"), new Scope("write") };
            
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Act
            var audience = client.RevokeScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
            Assert.Contains(audience.Scopes, s => s.Value == "write");
            Assert.DoesNotContain(audience.Scopes, s => s.Value == "read");
        }

        [Fact]
        public void RevokeScopes_WithAllScopes_LeavesAudienceEmpty()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            var scopes = new[] { new Scope("read"), new Scope("write") };
            
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, scopes, _time.GetUtcNow());

            // Act
            var audience = client.RevokeScopes(audienceName, scopes, _time.GetUtcNow());

            // Assert
            Assert.Empty(audience.Scopes);
            Assert.Contains(client.Audiences, a => a.Name == audienceName); // Audience still exists
        }

        [Fact]
        public void RevokeScopes_WithNonExistentScopes_IsIdempotent()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow());

            // Act - revoke scope that doesn't exist
            var audience = client.RevokeScopes(audienceName, new[] { new Scope("write") }, _time.GetUtcNow());

            // Assert - no error, scope count unchanged
            Assert.Single(audience.Scopes);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
        }

        [Fact]
        public void RevokeScopes_WithNonExistentAudience_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                client.RevokeScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow()));

            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public void RevokeScopes_UpdatesClientUpdatedAt()
        {
            // Arrange
            var client = new ClientBuilder().Build();
            var audienceName = new AudienceName("api");
            client.AddAudience(audienceName, _time.GetUtcNow());
            client.GrantScopes(audienceName, new[] { new Scope("read") }, _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            client.RevokeScopes(audienceName, new[] { new Scope("read") }, expectedTime);

            // Assert
            Assert.Equal(expectedTime, client.UpdatedAt);
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