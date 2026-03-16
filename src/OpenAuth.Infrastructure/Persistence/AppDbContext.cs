using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Application.Abstractions;
using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.Users;

namespace OpenAuth.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<ApiResource> ApiResources { get; set; }
    public DbSet<ClientApiAccess> ClientApiAccesses { get; set; }
    public DbSet<Secret> ClientSecrets { get; set; }
    public DbSet<SigningKey> SigningKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public async override Task<int> SaveChangesAsync(CancellationToken ct = default)
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