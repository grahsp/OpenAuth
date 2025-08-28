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
        builder.HasKey(x => x.KeyId);

        builder.Property(x => x.KeyId)
            .HasConversion(
                id => id.Value,
                value => new SigningKeyId(value))
            .ValueGeneratedNever()
            .IsRequired();

        
        // Relations
        builder.HasOne(x => x.Client)
            .WithMany(y => y.SigningKeys)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
        

        // Properties
        builder.Property(x => x.Algorithm)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PublicKey)
            .IsRequired(false);
        
        builder.Property(x => x.PrivateKey)
            .IsRequired();
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
            .IsRequired(false);
    }
}