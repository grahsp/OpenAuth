using Microsoft.AspNetCore.Identity;
using OpenAuth.Domain.Users;

namespace OpenAuth.Infrastructure.Persistence.Seeders;

public class IdentitySeeder(UserManager<User> users, RoleManager<IdentityRole<Guid>> roles) : ISeeder
{
	public async Task SeedAsync(CancellationToken ct)
	{
		const string role = "admin";
		const string name = "admin";

		if (!await roles.RoleExistsAsync(role))
		{
			var result = await roles.CreateAsync(new IdentityRole<Guid>(role));
			EnsureSuccess(result);
		}

		var user = await users.FindByNameAsync(name);

		if (user is null)
		{
			user = new User { UserName = name };

			var result = await users.CreateAsync(user, "admin");
			EnsureSuccess(result);
		}

		if (!await users.IsInRoleAsync(user, role))
		{
			var result = await users.AddToRoleAsync(user, role);
			EnsureSuccess(result);
		}
	}
	
	private static void EnsureSuccess(IdentityResult result)
	{
		if (!result.Succeeded)
		{
			var errors = string.Join(", ", result.Errors.Select(e => e.Description));
			throw new InvalidOperationException(errors);
		}
	}
}