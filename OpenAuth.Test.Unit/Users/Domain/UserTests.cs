using Microsoft.Extensions.Time.Testing;
using OpenAuth.Domain.Users;
using OpenAuth.Domain.Users.ValueObjects;

namespace OpenAuth.Test.Unit.Users.Domain;

public class UserTests
{
    private readonly FakeTimeProvider _time = new();
    
    
    [Fact]
    public void Create_WithValidInput_CreatesExpectedUser()
    {
        var username = "username";
        var email = new Email("test@test.com");
        var password = new HashedPassword("hashed-password");
        
        var user = User.Create(username, email, password, _time.GetUtcNow());
        
        Assert.Equal(username, user.Username);
        Assert.Equal(email, user.Email);
        Assert.Equal(password, user.HashedPassword);
        Assert.Equal(_time.GetUtcNow(), user.CreatedAt);
        Assert.True(user.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidUsername_ThrowsException(string? username)
    {
        var email = new Email("test@test.com");
        var password = new HashedPassword("hashed-password");

        Assert.ThrowsAny<ArgumentException>(()
            => User.Create(username!, email, password, _time.GetUtcNow()));
    }
}