using OpenAuth.Domain.Clients;

namespace OpenAuth.Domain.Tests.Clients;

public class ScopeTests
{
    [Fact]
    public void Scope_Normalizes_Value()
    {
        var scope = new Scope("   REaD   ");
        Assert.Equal("read", scope.Value);
    }

    [Fact]
    public void Scope_ThrowsOn_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Scope(null!));
    }
    
    [Fact]
    public void Scope_ThrowsOn_ValueWhitespace()
    {
        Assert.Throws<ArgumentException>(() =>
            new Scope(new string(' ', Scope.Min)));
    }
    
    [Fact]
    public void Scope_ThrowsOn_ValueTooShort()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Scope(new string('a', Scope.Min - 1)));
    }
    
    [Fact]
    public void Scope_ThrowsOn_ValueTooLong()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Scope(new string('a', Scope.Max + 1)));
    }
}
