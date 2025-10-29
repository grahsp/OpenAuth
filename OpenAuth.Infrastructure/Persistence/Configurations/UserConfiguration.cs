using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Users;
using OpenAuth.Domain.Users.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value))
            .ValueGeneratedNever();

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        
        builder.Property(u => u.Username).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => new Email(value))
            .IsRequired();

        builder.Property(u => u.HashedPassword)
            .HasConversion(
                password => password.Value,
                value => new HashedPassword(value))
            .IsRequired();
    }
}