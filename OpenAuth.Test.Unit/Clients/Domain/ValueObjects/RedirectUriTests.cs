using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class RedirectUriTests
{
    [Theory]
    [InlineData("https://app.com/callback")]
    [InlineData("http://localhost:3000/auth")]
    [InlineData("https://app.com:8080/callback")]
    [InlineData("https://app.com/callback?query=value")]
    public void Create_WithValidUri_Succeeds(string uri)
    {
        // Act
        var redirectUri = RedirectUri.Create(uri);
        
        // Assert
        Assert.Equal(uri, redirectUri.Value);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ThrowsArgumentException(string? uri)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => RedirectUri.Create(uri!));
        Assert.Contains("cannot be empty", exception.Message);
    }
    
    [Theory]
    [InlineData("not a uri")]
    [InlineData("ftp://invalid.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///etc/passwd")]
    public void Create_WithInvalidScheme_ThrowsArgumentException(string uri)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => RedirectUri.Create(uri));
    }
    
    [Fact]
    public void TwoRedirectUris_WithSameValue_AreEqual()
    {
        // Arrange
        var uri1 = RedirectUri.Create("https://app.com/callback");
        var uri2 = RedirectUri.Create("https://app.com/callback");
        
        // Assert
        Assert.Equal(uri1, uri2);
        Assert.Equal(uri1.GetHashCode(), uri2.GetHashCode());
    }
    
    [Fact]
    public void RedirectUri_CanBeUsedInHashSet()
    {
        // Arrange
        var set = new HashSet<RedirectUri>
        {
            RedirectUri.Create("https://app.com/callback"),
            RedirectUri.Create("https://app.com/callback") // duplicate
        };
        
        // Assert
        Assert.Single(set);
    }
    
    [Fact]
    public void TwoRedirectUris_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var uri1 = RedirectUri.Create("https://app.com/callback");
        var uri2 = RedirectUri.Create("https://other.com/callback");
        
        // Assert
        Assert.NotEqual(uri1, uri2);
    }
}