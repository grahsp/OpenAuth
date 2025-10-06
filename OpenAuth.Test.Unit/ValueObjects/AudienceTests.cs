using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Clients.Audiences;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Test.Unit.ValueObjects;

public class AudienceTests
{
    private FakeTimeProvider _time = new();

    public class AudienceCreation : AudienceTests
    {
        [Fact]
        public void Create_SetsPropertiesCorrectly()
        {
            // Arrange
            var name = new AudienceName("api");
            var now = _time.GetUtcNow();

            // Act
            var audience = Audience.Create(name, now);

            // Assert
            Assert.Equal(name, audience.Name);
            Assert.NotEqual(Guid.Empty, audience.Id.Value);
            Assert.Equal(now, audience.CreatedAt);
            Assert.Equal(now, audience.UpdatedAt);
            Assert.Empty(audience.Scopes);
        }

        [Fact]
        public void Create_GeneratesUniqueIds()
        {
            // Arrange
            var name = new AudienceName("api");
            var now = _time.GetUtcNow();

            // Act
            var audience1 = Audience.Create(name, now);
            var audience2 = Audience.Create(name, now);

            // Assert
            Assert.NotEqual(audience1.Id, audience2.Id);
        }
    }

    public class GrantScopesTests : AudienceTests
    {
        [Fact]
        public void GrantScopes_WithNewScopes_AddsToCollection()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            var scopes = new[] { new Scope("read"), new Scope("write") };

            // Act
            audience.GrantScopes(scopes, _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
            Assert.Contains(audience.Scopes, s => s.Value == "write");
        }

        [Fact]
        public void GrantScopes_WithDuplicates_IgnoresDuplicates()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            var scopes = new[] { new Scope("read"), new Scope("write"), new Scope("read") };

            // Act
            audience.GrantScopes(scopes, _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
        }

        [Fact]
        public void GrantScopes_WithExistingScopes_IsIdempotent()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read")], _time.GetUtcNow());

            // Act - grant same scope again
            audience.GrantScopes([new Scope("read")], _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
        }

        [Fact]
        public void GrantScopes_UpdatesUpdatedAt()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            audience.GrantScopes([new Scope("read")], expectedTime);

            // Assert
            Assert.Equal(expectedTime, audience.UpdatedAt);
        }

        [Fact]
        public void GrantScopes_WithEmptyCollection_DoesNothing()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            var originalUpdateTime = audience.UpdatedAt;

            _time.Advance(TimeSpan.FromSeconds(1));

            // Act
            audience.GrantScopes([], _time.GetUtcNow());

            // Assert
            Assert.Empty(audience.Scopes);
            // UpdatedAt still changes (Touch is called)
            Assert.NotEqual(originalUpdateTime, audience.UpdatedAt);
        }

        [Fact]
        public void GrantScopes_AccumulatesScopes()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read")], _time.GetUtcNow());

            // Act - grant additional scopes
            audience.GrantScopes([new Scope("write"), new Scope("delete")], _time.GetUtcNow());

            // Assert
            Assert.Equal(3, audience.Scopes.Count);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
            Assert.Contains(audience.Scopes, s => s.Value == "write");
            Assert.Contains(audience.Scopes, s => s.Value == "delete");
        }
    }

    public class RevokeScopesTests : AudienceTests
    {
        [Fact]
        public void RevokeScopes_WithExistingScopes_RemovesFromCollection()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read"), new Scope("write")], _time.GetUtcNow());

            // Act
            audience.RevokeScope([new Scope("read")], _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
            Assert.Contains(audience.Scopes, s => s.Value == "write");
            Assert.DoesNotContain(audience.Scopes, s => s.Value == "read");
        }

        [Fact]
        public void RevokeScope_WithAllScopes_LeavesCollectionEmpty()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            var scopes = new[] { new Scope("read"), new Scope("write") };
            audience.GrantScopes(scopes, _time.GetUtcNow());

            // Act
            audience.RevokeScope(scopes, _time.GetUtcNow());

            // Assert
            Assert.Empty(audience.Scopes);
        }

        [Fact]
        public void RevokeScope_WithNonExistentScopes_IsIdempotent()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read")], _time.GetUtcNow());

            // Act - revoke scope that doesn't exist
            audience.RevokeScope([new Scope("write")], _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
        }

        [Fact]
        public void RevokeScope_UpdatesUpdatedAt()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read")], _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            audience.RevokeScope([new Scope("read")], expectedTime);

            // Assert
            Assert.Equal(expectedTime, audience.UpdatedAt);
        }

        [Fact]
        public void RevokeScope_WithEmptyCollection_DoesNothing()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read")], _time.GetUtcNow());
            var originalUpdateTime = audience.UpdatedAt;

            _time.Advance(TimeSpan.FromSeconds(1));

            // Act
            audience.RevokeScope([], _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
            // UpdatedAt still changes (Touch is called)
            Assert.NotEqual(originalUpdateTime, audience.UpdatedAt);
        }

        [Fact]
        public void RevokeScope_WithPartialMatch_RemovesOnlyMatchingScopes()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes(
                [new Scope("read"), new Scope("write"), new Scope("delete")], 
                _time.GetUtcNow());

            // Act - revoke some scopes, including one that doesn't exist
            audience.RevokeScope(
                [new Scope("read"), new Scope("admin")], 
                _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
            Assert.Contains(audience.Scopes, s => s.Value == "write");
            Assert.Contains(audience.Scopes, s => s.Value == "delete");
            Assert.DoesNotContain(audience.Scopes, s => s.Value == "read");
        }
    }

    public class SetScopesTests : AudienceTests
    {
        [Fact]
        public void SetScopes_ReplacesAllExistingScopes()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read"), new Scope("write")], _time.GetUtcNow());

            // Act
            var newScopes = new[] { new Scope("admin"), new Scope("delete") };
            audience.SetScopes(newScopes, _time.GetUtcNow());

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
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            audience.GrantScopes([new Scope("read"), new Scope("write")], _time.GetUtcNow());

            // Act
            audience.SetScopes([], _time.GetUtcNow());

            // Assert
            Assert.Empty(audience.Scopes);
        }

        [Fact]
        public void SetScopes_WithDuplicates_IgnoresDuplicates()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());

            // Act
            var scopes = new[] { new Scope("read"), new Scope("write"), new Scope("read") };
            audience.SetScopes(scopes, _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
        }

        [Fact]
        public void SetScopes_UpdatesUpdatedAt()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());

            _time.Advance(TimeSpan.FromSeconds(1));
            var expectedTime = _time.GetUtcNow();

            // Act
            audience.SetScopes([new Scope("read")], expectedTime);

            // Assert
            Assert.Equal(expectedTime, audience.UpdatedAt);
        }

        [Fact]
        public void SetScopes_CanBeCalledMultipleTimes()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());

            // Act - set multiple times
            audience.SetScopes([new Scope("read")], _time.GetUtcNow());
            audience.SetScopes([new Scope("write")], _time.GetUtcNow());
            audience.SetScopes([new Scope("admin")], _time.GetUtcNow());

            // Assert
            Assert.Single(audience.Scopes);
            Assert.Contains(audience.Scopes, s => s.Value == "admin");
        }

        [Fact]
        public void SetScopes_WithSameScopes_IsIdempotent()
        {
            // Arrange
            var audience = Audience.Create(new AudienceName("api"), _time.GetUtcNow());
            var scopes = new[] { new Scope("read"), new Scope("write") };
            audience.SetScopes(scopes, _time.GetUtcNow());

            // Act - set same scopes again
            audience.SetScopes(scopes, _time.GetUtcNow());

            // Assert
            Assert.Equal(2, audience.Scopes.Count);
            Assert.Contains(audience.Scopes, s => s.Value == "read");
            Assert.Contains(audience.Scopes, s => s.Value == "write");
        }
    }
}