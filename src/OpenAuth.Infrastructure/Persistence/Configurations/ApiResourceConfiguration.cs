using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Apis.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Configurations;

public class ApiResourceConfiguration : IEntityTypeConfiguration<ApiResource>
{
	public void Configure(EntityTypeBuilder<ApiResource> builder)
	{
		builder.ToTable("ApiResources");	
		builder.HasKey(a => a.Id);

		builder.Property(a => a.Id)
			.HasConversion(
				id => id.Value,
				value => new ApiResourceId(value))
			.ValueGeneratedNever()
			.IsRequired();

		builder.HasIndex(a => a.Audience)
			.IsUnique();

		builder.Property(a => a.Audience)
			.HasConversion(
				a => a.Value,
				v => new AudienceIdentifier(v))
			.HasMaxLength(200)
			.IsRequired();

		builder.Property(a => a.ResourceName)
			.HasConversion(
				n => n.Value,
				v => new ApiResourceName(v))
			.HasMaxLength(100)
			.IsRequired();

		builder.OwnsMany(a => a.Permissions, perm =>
		{
			perm.ToTable("ApiPermissions");

			perm.WithOwner().HasForeignKey("ApiResourceId");

			perm.Property<Guid>("Id");
			perm.HasKey("Id");

			perm.Property(p => p.Scope)
				.HasConversion(
					scope => scope.Value,
					value => new Scope(value))
				.IsRequired();

			perm.Property(p => p.Description)
				.HasConversion(
					desc => desc.Value,
					value => new ScopeDescription(value))
				.IsRequired();
		});
	}
}