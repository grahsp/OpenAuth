using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Users.Interfaces;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Infrastructure.Users.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _context.Users
            .Where(u => u.Username == username)
            .SingleOrDefaultAsync(ct);

    public void Add(User user)
        => _context.Users.Add(user);

    public void Remove(User user)
        => _context.Users.Remove(user);
    
    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}