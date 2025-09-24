using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Configurations;

public class SigningKeyConfiguration : IEntityTypeConfiguration<SigningKey>
{
    public void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        // ID
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SigningKeyId(value))
            .ValueGeneratedNever()
            .IsRequired();

        // Properties
        builder.Property(x => x.Algorithm)
            .HasConversion<int>()
            .IsRequired();
        
        builder.Property(x => x.Key)
            .HasConversion(
                key => key.Value,
                value => new Key(value))
            .ValueGeneratedNever()
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
            .IsRequired();
        
        builder.Property(x => x.RevokedAt)
            .IsRequired(false);
    }
}