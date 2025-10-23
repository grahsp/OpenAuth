using OpenAuth.Domain.Users.ValueObjects;

namespace OpenAuth.Domain.Users;

public class User
{
    public UserId Id { get; private init; }
    public string Username { get; private set; }
    public Email Email { get; private set; }
    public HashedPassword HashedPassword { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private User() { }

    private User(string username, Email email, HashedPassword hashedPassword)
    {
        Id = UserId.New();
        Username = username;
        Email = email;
        HashedPassword = hashedPassword;
        IsActive = true;
        CreatedAt = DateTimeOffset.Now;
    }

    internal static User Create(string username, Email email, HashedPassword passwordHash)
    {
        return new User(username, email, passwordHash);
    }
}