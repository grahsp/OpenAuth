using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        // ID
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => new ClientId(value))
            .ValueGeneratedNever()
            .IsRequired();
            
        // Name
        builder.HasIndex(c => c.Name)
            .IsUnique();
            
        builder.Property(c => c.Name)
            .HasConversion(
                name => name.Value,
                value => new ClientName(value))
            .IsRequired();

        // Properties
        builder.Property(c => c.TokenLifetime)
            .HasConversion(
                t => t.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .IsRequired();
            
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
        builder.Property(c => c.Enabled).IsRequired();

        builder.Property(c => c.IsPublic).IsRequired();
        builder.Property(c => c.RequirePkce).IsRequired();
        
        builder.Ignore(c => c.AllowedGrantTypes);
        builder.Property<HashSet<GrantType>>("_allowedGrantTypes")
            .HasConversion(
                grants => string.Join(' ', grants.Select(g => g.Value)),
                value => string.IsNullOrWhiteSpace(value)
                    ? new HashSet<GrantType>()
                    : value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(GrantType.Create)
                        .ToHashSet(),
                new ValueComparer<HashSet<GrantType>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet()
                )
            )
            .HasColumnName("AllowedGrantTypes")
            .HasMaxLength(2000);

        builder.Ignore(c => c.RedirectUris);;
        builder.Property<HashSet<RedirectUri>>("_redirectUris")
            .HasConversion(
                uris => string.Join(' ', uris.Select(u => u.Value)),
                value => string.IsNullOrWhiteSpace(value)
                    ? new HashSet<RedirectUri>()
                    : value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(RedirectUri.Create)
                        .ToHashSet(),
                new ValueComparer<HashSet<RedirectUri>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet()
                )
            )
            .HasColumnName("RedirectUris")
            .HasMaxLength(2000);
        
        // Audience
        builder.Ignore(c => c.AllowedAudiences);
        builder.OwnsMany(c => c.AllowedAudiences, aud =>
        {
            aud.ToTable("Audiences");
            aud.WithOwner().HasForeignKey("ClientId"); // Shadow property

            aud.Property<Guid>("Id");
            aud.HasKey("Id");
            
            aud.Property(a => a.Name)
                .HasConversion(
                    name => name.Value,
                    value => new AudienceName(value))
                .HasMaxLength(100);

            aud.Property(a => a.Scopes)
                .HasConversion(
                    scopes => scopes.ToString(),
                    value => ScopeCollection.Parse(value ?? string.Empty))
                .HasColumnName("Scopes")
                .HasMaxLength(2000)
                .IsRequired(false);
        });
    }
}