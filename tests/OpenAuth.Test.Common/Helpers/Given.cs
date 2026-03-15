using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Domain.Apis;
using OpenAuth.Domain.Clients;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Builders;

namespace OpenAuth.Test.Common.Helpers;

public static class Given
{
	public static async Task<ApiResource> ApiAsync(IServiceProvider sp, Action<ApiResourceBuilder>? config = null)
	{
		var builder = new ApiResourceBuilder();
		config?.Invoke(builder);

		var api = builder.Build();

		var context = sp.GetRequiredService<AppDbContext>();
		
		context.ApiResources.Add(api);
		await context.SaveChangesAsync();

		return api;
	}
	
	public static async Task<Client> ClientAsync(IServiceProvider sp, Action<ClientBuilder>? config = null)
	{
		var builder = new ClientBuilder();
		config?.Invoke(builder);

		var client = builder.Build();

		var context = sp.GetRequiredService<AppDbContext>();
		
		context.Clients.Add(client);
		await context.SaveChangesAsync();

		return client;
	}
}