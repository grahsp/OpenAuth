using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Application.Abstractions;
using OpenAuth.Application.Clients.Commands.SetClientApiAccess;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Infrastructure.Persistence;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;
using OpenAuth.Test.Common.Hosting;
using OpenAuth.Test.E2E.OAuth;

namespace OpenAuth.Test.E2E.Extensions;

public static class TestHostExtensions
{
	public static async Task<ExternalOAuthModule> CreateApiClientAsync(this TestHost host, Action<OAuthClientBuilder>? configure = null)
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
			
			var handler = sp.GetRequiredService<ICommandHandler<SetClientApiAccessCommand>>();
			var scopes = new ScopeCollection(api.Permissions.Select(p => p.Scope));
			
			var command = new SetClientApiAccessCommand(registered.Client.Id, api.Id, scopes);
			await handler.HandleAsync(command, CancellationToken.None);
			
			// TODO: hard coded redirect uri into all clients
			var service = sp.GetRequiredService<IClientService>();
			await service.SetRedirectUrisAsync(registered.Client.Id, [RedirectUri.Parse(DefaultValues.RedirectUri)]);

			return new ExternalOAuthModule(host.CreateClient(), registered);
		});
	}
}