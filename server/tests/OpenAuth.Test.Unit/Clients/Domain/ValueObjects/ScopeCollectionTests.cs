using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class ScopeCollectionTests
{
    [Fact]
    public void Constructor_RemovesDuplicates_AndTrimsWhitespace()
    {
        // Arrange
        var read = new Scope("read");
        var write = new Scope("write");
        
        var input = new[] { new Scope(" read "), read, write };

        // Act
        var scopes = new ScopeCollection(input);

        // Assert
        Assert.Equal(2, scopes.Count);
        Assert.Contains(read, scopes);
        Assert.Contains(write, scopes);
    }

    [Theory]
    [InlineData("read write", new[] { "read", "write" })]
    [InlineData(" read   write  ", new[] { "read", "write" })]
    [InlineData("", new string[0])]
    [InlineData("   ", new string[0])]
    public void Parse_ParsesSpaceSeparatedScopes(string input, string[] expected)
    {
        // Act
        var scopes = ScopeCollection.Parse(input);

        // Assert
        Assert.Equal(expected.Length, scopes.Count);
        foreach (var s in expected)
            Assert.Contains(new Scope(s), scopes);
    }

    [Fact]
    public void Contains_ReturnsTrue_WhenScopeExists()
    {
        // Arrange
        var scopes = ScopeCollection.Parse("read write");

        // Assert
        Assert.True(scopes.Contains(new Scope("read")));
        Assert.False(scopes.Contains(new Scope("admin")));
    }

    [Fact]
    public void ToString_ProducesSpaceSeparatedString()
    {
        // Arrange
        var scopes = ScopeCollection.Parse("read write");

        // Act
        var result = scopes.ToString();

        // Assert
        Assert.Equal("read write", result);
    }

    [Fact]
    public void Equality_IsBasedOnScopeSet_NotOrder()
    {
        // Arrange
        var a = ScopeCollection.Parse("read write");
        var b = ScopeCollection.Parse("write read");

        // Assert
        Assert.Equal(a, b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Enumerator_IteratesAllScopes()
    {
        // Arrange
        var scopes = ScopeCollection.Parse("read write");

        // Act
        var result = scopes.ToList();

        // Assert
        Assert.Contains(new Scope("read"), result);
        Assert.Contains(new Scope("write"), result);
    }
}