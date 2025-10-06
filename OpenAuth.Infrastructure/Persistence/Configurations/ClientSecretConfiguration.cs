using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Configurations;

public class ClientSecretConfiguration : IEntityTypeConfiguration<ClientSecret>
{
    public void Configure(EntityTypeBuilder<ClientSecret> builder)
    {
        // ID
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SecretId(value))
            .ValueGeneratedNever()
            .IsRequired();

        
        // Relations
        builder.HasOne(x => x.Client)
            .WithMany(y => y.Secrets)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        
        // Properties
        builder.Property(x => x.Hash)
            .HasConversion(
                hash => hash.Value,
                value => new SecretHash(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.Property(x => x.ExpiresAt)
            .IsRequired();
        
        builder.Property(x => x.RevokedAt)
            .IsRequired(false);
    }
}