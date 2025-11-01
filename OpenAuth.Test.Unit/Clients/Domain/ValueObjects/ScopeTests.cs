using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class ScopeTests
{
    [Fact]
    public void Scope_Normalizes_Value()
    {
        var scope = new Scope("   REaD   ");
        Assert.Equal("read", scope.NormalizedValue);
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
            new Scope("   "));
    }
    
    [Fact]
    public void Scope_ThrowsOn_ValueTooShort()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Scope("a"));
    }
    
    [Fact]
    public void Scope_ThrowsOn_ValueTooLong()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Scope(new string('a',999)));
    }
}
