using System.Text.Json;
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
            .HasMaxLength(64)
            .IsRequired();

        
        // Properties
        builder.Property(x => x.TokenLifetime)
            .HasConversion(
                t => t.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .IsRequired();
        
        builder.Property(x => x.Grants)
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions)null!),
                json => JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json, (JsonSerializerOptions)null!)
                        ?? new Dictionary<string, List<string>>())
            .HasColumnType("nvarchar(max)")
            .IsRequired();
            
        builder.Property(x => x.Enabled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}