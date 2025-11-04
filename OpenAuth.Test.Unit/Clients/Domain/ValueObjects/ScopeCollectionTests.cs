using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class ScopeCollectionTests
{
    [Fact]
    public void Constructor_RemovesDuplicates_AndTrimsWhitespace()
    {
        // Arrange
        var input = new[] { " read ", "write", "read" };

        // Act
        var scopes = new ScopeCollection(input);

        // Assert
        Assert.Equal(2, scopes.Scopes.Count);
        Assert.Contains("read", scopes.Scopes);
        Assert.Contains("write", scopes.Scopes);
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
        Assert.Equal(expected.Length, scopes.Scopes.Count);
        foreach (var s in expected)
            Assert.Contains(s, scopes.Scopes);
    }

    [Fact]
    public void Contains_ReturnsTrue_WhenScopeExists()
    {
        var scopes = ScopeCollection.Parse("read write");

        Assert.True(scopes.Contains("read"));
        Assert.False(scopes.Contains("admin"));
    }

    [Fact]
    public void ToString_ProducesSpaceSeparatedString()
    {
        var scopes = new ScopeCollection(["read", "write"]);

        Assert.Equal("read write", scopes.ToString());
    }

    [Fact]
    public void Equality_IsBasedOnScopeSet_NotOrder()
    {
        var a = new ScopeCollection(["read", "write"]);
        var b = new ScopeCollection(["write", "read"]);

        Assert.Equal(a, b);
    }
}