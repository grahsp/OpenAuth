using OpenAuth.Application.Security.Hashing;
using OpenAuth.Application.Shared.Models;
using OpenAuth.Application.Users.Interfaces;
using OpenAuth.Domain.Users;
using OpenAuth.Domain.Users.ValueObjects;

namespace OpenAuth.Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IHasher _hasher;
    private readonly TimeProvider _time;
    
    public UserService(IUserRepository repository, IHasher hasher, TimeProvider time)
    {
        _repository = repository;
        _hasher = hasher;
        _time = time;
    }
    
    
    public async Task<Result<User>> RegisterUserAsync(string username, string email, string password, CancellationToken ct = default)
    {
        var hash = _hasher.Hash(password);
        var hashedPassword = new HashedPassword(hash);
        
        var user = User.Create(username, new Email(email), hashedPassword, _time.GetUtcNow());

        try
        {
            _repository.Add(user);
            await _repository.SaveChangesAsync(ct);
        }
        // TODO: Add real error handling
        catch (Exception ex)
        {
            return Result<User>.Fail(new Error("db_error", ex.Message));
        }

        return Result<User>.Ok(user);
    }
}