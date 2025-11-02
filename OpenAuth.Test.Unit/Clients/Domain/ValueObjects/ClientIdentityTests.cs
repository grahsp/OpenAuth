using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Unit.Clients.Domain.ValueObjects;

public class ClientIdentityTests
{
    [Fact]
    public void Ctor_WhenIdIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(()
            => new ClientIdentity(null!, ClientName.Create("test-client")));
    }
    
    [Fact]
    public void Ctor_WhenNameIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(()
            => new ClientIdentity(ClientId.New(), null!));
    }
    
    [Fact]
    public void Equals_WhenValuesMatch_ReturnsTrue()
    {
        var builder = new ClientIdentityBuilder()
            .WithId(ClientId.New())
            .WithName("test-client");
        
        var a = builder.Build();
        var b = builder.Build();
        
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_WhenIdsDiffer_ReturnsFalse()
    {
        var builder = new ClientIdentityBuilder()
            .WithName("test-client");
        
        var a = builder.Build();
        var b = builder.Build();
        
        Assert.NotEqual(a, b);   
    }

    [Fact]
    public void Equals_WhenNamesDiffer_ReturnsFalse()
    {
        var builder = new ClientIdentityBuilder()
            .WithId(ClientId.New());
        
        var a = builder.WithName("A").Build();
        var b = builder.WithName("B").Build();
        
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void SetName_WhenNameIsUpdated_ReturnsNewInstance()
    {
        var identity = new ClientIdentityBuilder()
            .WithName("client")
            .Build();
        
        var updatedIdentity = identity.SetName(ClientName.Create("test-client"));
        
        Assert.NotSame(identity, updatedIdentity);       
    }
    
    [Fact]
    public void SetName_WhenNameIsSame_ReturnsSameInstance()
    {
        var identity = new ClientIdentityBuilder()
            .WithName("test-client")
            .Build();
        
        var updatedIdentity = identity.SetName(ClientName.Create("test-client"));
        
        Assert.Same(identity, updatedIdentity);
    }

    [Fact]
    public void SetName_WhenNameIsNull_ThrowsException()
    {
        var identity = new ClientIdentityBuilder().Build();
        Assert.Throws<ArgumentNullException>(() => identity.SetName(null!));
    }
}