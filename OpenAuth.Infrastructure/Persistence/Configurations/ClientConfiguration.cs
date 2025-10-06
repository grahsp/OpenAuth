using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.Audiences;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        // ID
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ClientId(value))
            .ValueGeneratedNever()
            .IsRequired();
            
        // Name
        builder.HasIndex(x => x.Name)
            .IsUnique();
            
        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => new ClientName(value))
            .IsRequired();

        // Properties
        builder.Property(x => x.TokenLifetime)
            .HasConversion(
                t => t.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .IsRequired();
            
        builder.Property(x => x.Enabled)
            .IsRequired();
        
        // Audience
        builder.OwnsMany(c => c.Audiences, a =>
        {
            a.ToTable("Audiences");
            
            a.WithOwner()
                .HasForeignKey("ClientId"); // Shadow property
            
            a.HasKey(nameof(Audience.Id));
            
            a.Property(aud => aud.Id)
                .HasConversion(
                    id => id.Value,
                    value => new AudienceId(value));
            
            a.Property(aud => aud.Name)
                .HasConversion(
                    name => name.Value,
                    value => new AudienceName(value))
                .HasMaxLength(100);
            
            // Scopes configuration
            a.Property<HashSet<Scope>>("_scopes")
                .HasConversion(
                    scopes => string.Join(' ', scopes.Select(s => s.Value)),
                    value => string.IsNullOrWhiteSpace(value)
                        ? new HashSet<Scope>()
                        : value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => new Scope(v))
                            .ToHashSet(),
                    new ValueComparer<HashSet<Scope>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToHashSet()
                    )
                )
                .HasColumnName("Scopes")
                .HasMaxLength(2000);
            
            a.Ignore(aud => aud.Scopes);
            
            a.Property(aud => aud.CreatedAt).IsRequired();
            a.Property(aud => aud.UpdatedAt).IsRequired();
        });
        
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
        builder.Property(c => c.Enabled).IsRequired();
    }
}