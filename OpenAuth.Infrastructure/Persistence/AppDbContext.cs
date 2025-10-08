using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Infrastructure.Persistence;

public class AppDbContext : DbContext
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
}