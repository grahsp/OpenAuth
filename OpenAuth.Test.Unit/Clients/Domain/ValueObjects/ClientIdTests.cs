using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class ClientIdTests
{
    [Fact]
    public void TryCreate_WhenInputValid_ReturnsTrueAndClientId()
    {
        var expected = Guid.NewGuid().ToString();
        var result = ClientId.TryCreate(expected, out var actual);

        Assert.True(result);
        Assert.Equal(expected, actual.ToString());
    }

    [Fact]
    public void TryCreate_WhenInputEmptyGuid_ReturnsFalse()
    {
        var result = ClientId.TryCreate(Guid.Empty.ToString(), out var actual);

        Assert.False(result);
        Assert.Equal(Guid.Empty, actual.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this-is-not-a-guid")]
    public void TryCreate_WhenInputInvalid_ReturnsFalse(string? input)
    {
        var result = ClientId.TryCreate(input!, out var actual);
        
        Assert.False(result);
        Assert.Equal(Guid.Empty, actual.Value);
    }
}