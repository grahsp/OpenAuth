using Microsoft.AspNetCore.Identity;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.Users;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Hosting;

namespace OpenAuth.Test.Common.Helpers;

public static class Given
{
	public static async Task<Client> ClientAsync(TestScope scope, Action<ClientBuilder>? configure = null)
	{
		var builder = new ClientBuilder();
		configure?.Invoke(builder);

		var client = builder.Build();

		var context = scope.Resolve<AppDbContext>();
		context.Clients.Add(client);
		await context.SaveChangesAsync();

		return client;
	}
	
	public static async Task<User> UserAsync(TestScope scope, Guid? userId = null)
	{
		var id = userId ?? Guid.Parse(DefaultValues.UserId);
		
		var user = new User
		{
			Id = id,
			UserName = DefaultValues.UserName,
			Email = DefaultValues.UserEmail
		};
			
		var manager = scope.Resolve<UserManager<User>>();
		await manager.CreateAsync(user);

		return user;
	}

	public static async Task<SigningKey> SigningKeyAsync(TestScope scope, Action<SigningKeyBuilder>? configure = null)
	{
		var builder = new SigningKeyBuilder();
		configure?.Invoke(builder);

		var signingKey = builder.Build();

		var context = scope.Resolve<AppDbContext>();
		context.SigningKeys.Add(signingKey);
		await context.SaveChangesAsync();

		return signingKey;
	}
}