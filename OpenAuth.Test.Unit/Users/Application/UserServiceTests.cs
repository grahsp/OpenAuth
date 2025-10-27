using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Security.Hashing;
using OpenAuth.Application.Users.Interfaces;
using OpenAuth.Application.Users.Services;
using OpenAuth.Domain.Users;
using OpenAuth.Domain.Users.ValueObjects;

namespace OpenAuth.Test.Unit.Users.Application;

public class UserServiceTests
{
    private readonly IUserRepository _repo;
    private readonly IHasher _hasher;
    private readonly FakeTimeProvider _time;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _repo = Substitute.For<IUserRepository>();
        _hasher = Substitute.For<IHasher>();
        _time = new FakeTimeProvider();
        
        _sut = new UserService(_repo, _hasher, _time);

        _repo.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        
        _hasher.Hash(Arg.Any<string>())
            .Returns(info => info.Arg<string>());
    }


    private static (string username, string email, string password) CreateDefaultUserInput()
        => ("John", "john@example.com", "password");

    [Fact]
    public async Task RegisterUserAsync_WhenValidUser_PersistsAndReturnsUser()
    {
        var (username, email, password) = CreateDefaultUserInput();
        var now = _time.GetUtcNow();
        
        var result = await _sut.RegisterUserAsync(username, email, password);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        Assert.Equal(username, result.Value.Username);
        Assert.Equal(email, result.Value.Email.Value);
        Assert.Equal(password, result.Value.HashedPassword.Value);
        Assert.Equal(now, result.Value.CreatedAt);
        
        _repo.Received(1).Add(Arg.Any<User>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task RegisterUserAsync_WhenCalled_HashesPassword()
    {
        var (username, email, password) = CreateDefaultUserInput();
        const string expectedHashedPassword = "hashed_password";

        _hasher.Hash(Arg.Any<string>())
            .Returns(expectedHashedPassword);

        var result = await _sut.RegisterUserAsync(username, email, password);
        
        Assert.Equal(expectedHashedPassword, result.Value.HashedPassword.Value);
        _hasher.Received(1).Hash(password);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenDbException_ReturnsFailureResult()
    {
        const string expectedMessage = "Database error";
        _repo.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedMessage));

        var result = await _sut.RegisterUserAsync("John", "john@example.com", "password");
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("db_error", result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
    }
}