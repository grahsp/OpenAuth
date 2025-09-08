using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientSecret> ClientSecrets { get; set; }
    public DbSet<SigningKey> SigningKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}