using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.Integration.OAuth;

namespace OpenAuth.Test.Integration.Extensions;

public static class TestHostExtensions
{
	public static async Task<InternalOAuthModule> CreateClientAsync(this TestHost host, Action<OAuthClientBuilder>? configure = null)
	{
		return await host.CreateModuleAsync(async scope =>
		{
			var sp = scope.ServiceProvider;

			var builder = new OAuthClientBuilder(sp);
			configure?.Invoke(builder);

			var registered = await builder.CreateAsync();

			var api = new ApiResourceBuilder().Build();
			
			var context = sp.GetRequiredService<AppDbContext>();
			context.ApiResources.Add(api);
			await context.SaveChangesAsync();
			
			var clientService = sp.GetRequiredService<IClientService>();
			var scopes = new ScopeCollection(api.Permissions.Select(p => p.Scope));
			await clientService.GrantApiAccessAsync(registered.Client.Id, api.Id, scopes);

			return new InternalOAuthModule(scope, registered);
		});
	}

}