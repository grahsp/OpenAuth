using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

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
            .ValueGeneratedNever()
            .IsRequired();

        // Audience & Scopes
        builder.OwnsMany(client => client.Audiences,
            audienceBuilder =>
            {
                audienceBuilder.Property(a => a.Value)
                    .HasColumnName("Audience")
                    .HasField("_value")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
                
                audienceBuilder.OwnsMany(audience => audience.Scopes,
                    scopeBuilder =>
                    {
                        scopeBuilder.Property(s => s.Value)
                            .HasColumnName("Scope")
                            .IsRequired();
                    });
                
                audienceBuilder.Navigation(a => a.Scopes)
                    .HasField("_scopes")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

        builder.Navigation(c => c.Audiences)
            .HasField("_audiences")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        // Properties
        builder.Property(x => x.TokenLifetime)
            .HasConversion(
                t => t.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .IsRequired();
            
        builder.Property(x => x.Enabled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}