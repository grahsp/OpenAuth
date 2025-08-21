using OpenAuth.Domain.Clients;

namespace OpenAuth.Domain.Tests.Clients;

public class AudienceTests
{
    [Fact]
    public void Audience_Normalizes_Value()
    {
        var aud = new Audience("    API   ");
        Assert.Equal("api", aud.Value);
    }

    [Fact]
    public void Audience_ThrowsOn_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Audience(null!));
    }
    
    [Fact]
    public void Audience_ThrowsOn_ValueWhitespace()
    {
        Assert.Throws<ArgumentException>(() =>
            new Audience(new string(' ', Audience.Min)));
    }
    
    [Fact]
    public void Audience_ThrowsOn_ValueTooShort()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Audience(new string('a', Audience.Min - 1)));
    }
    
    [Fact]
    public void Audience_ThrowsOn_ValueTooLong()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Audience(new string('a', Audience.Max + 1)));
    }
}