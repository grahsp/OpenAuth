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
        builder.OwnsOne(x => x.KeyMaterial, kb =>
        {
            kb.Property(k => k.Key)
                .HasConversion(
                    v => v.Value,
                    v => new Key(v))
                .HasColumnName("Key")
                .IsRequired();

            kb.Property(km => km.Alg)
                .HasConversion<string>()
                .HasColumnName("Algorithm")
                .IsRequired();

            kb.Property(km => km.Kty)
                .HasConversion<string>()
                .HasColumnName("KeyType")
                .IsRequired();
        });
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
            .IsRequired();
        
        builder.Property(x => x.RevokedAt)
            .IsRequired(false);
    }
}