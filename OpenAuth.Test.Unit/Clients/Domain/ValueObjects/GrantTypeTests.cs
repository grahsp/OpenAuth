using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class GrantTypeTests
{
    [Fact]
    public void Create_WithAuthorizationCode_ReturnsCorrectValue()
    {
        // Act
        var grantType = GrantType.Create("authorization_code");
        
        // Assert
        Assert.Equal("authorization_code", grantType.Value);
    }
    
    [Theory]
    [InlineData("authorization_code")]
    [InlineData("client_credentials")]
    public void Create_WithValidGrantType_Succeeds(string value)
    {
        // Act
        var grantType = GrantType.Create(value);
        
        // Assert
        Assert.Equal(value, grantType.Value);
    }
    
    [Theory]
    [InlineData("invalid_grant")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithInvalidGrantType_ThrowsArgumentException(string? value)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => GrantType.Create(value!));
        Assert.Contains("Unknown", exception.Message);
    }
    
    [Fact]
    public void AuthorizationCode_ReturnsSameInstance()
    {
        // Act
        var grant1 = GrantType.AuthorizationCode;
        var grant2 = GrantType.AuthorizationCode;
        
        // Assert
        Assert.Equal(grant1, grant2);
    }
    
    [Fact]
    public void AuthorizationCode_HasCorrectValue()
    {
        // Act
        var grantType = GrantType.AuthorizationCode;
        
        // Assert
        Assert.Equal("authorization_code", grantType.Value);
    }
    
    [Fact]
    public void ClientCredentials_HasCorrectValue()
    {
        // Act
        var grantType = GrantType.ClientCredentials;
        
        // Assert
        Assert.Equal("client_credentials", grantType.Value);
    }
    
    [Fact]
    public void GrantType_CanBeUsedInHashSet()
    {
        // Arrange
        var set = new HashSet<GrantType>
        {
            GrantType.AuthorizationCode,
            GrantType.AuthorizationCode // duplicate
        };
        
        // Assert
        Assert.Single(set);
    }
}