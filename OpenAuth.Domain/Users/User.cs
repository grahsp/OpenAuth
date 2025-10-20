using OpenAuth.Domain.Users.ValueObjects;

namespace OpenAuth.Domain.Users;

public class User
{
    public UserId Id { get; private init; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private User() { }

    private User(string email, string username, string passwordHash)
    {
        Id = UserId.New();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTimeOffset.Now;
    }

    internal static User Create(string email, string username, string passwordHash)
    {
        return new User(email, username, passwordHash);
    }
}