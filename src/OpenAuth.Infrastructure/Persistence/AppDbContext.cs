using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.Users;

namespace OpenAuth.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Secret> ClientSecrets { get; set; }
    public DbSet<SigningKey> SigningKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ValidateAggregates();
        return await base.SaveChangesAsync(ct);
    }

    private void ValidateAggregates()
    {
        var clients = ChangeTracker
            .Entries<Client>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var client in clients)
            client.ValidateClient();
    }
}