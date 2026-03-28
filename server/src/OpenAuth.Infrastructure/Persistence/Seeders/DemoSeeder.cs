using Microsoft.EntityFrameworkCore;
using OpenAuth.Domain.ApiResources;
using OpenAuth.Domain.ApiResources.ValueObjects;
using OpenAuth.Domain.Clients;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Infrastructure.Persistence.Seeders;

public class DemoSeeder(AppDbContext context, TimeProvider time) : ISeeder
{
	private static readonly ClientId SpaClientId =
		ClientId.Parse("7b3f1d57-2068-4b9c-b86a-ac6d99838677");

	private static readonly AudienceIdentifier WeatherAudience =
		new AudienceIdentifier("http://weather-demo.com");

	private static readonly Scope WeatherReadScope =
		new Scope("weather:read");

	public async Task SeedAsync(CancellationToken ct)
	{
		var api = await EnsureWeatherApiAsync(ct);
		var spa = await EnsureSpaClientAsync(ct);
		
		EnsureClientApiAccess(spa, api);
	}

	private async Task<Client> EnsureSpaClientAsync(CancellationToken ct)
	{
		var now = time.GetUtcNow();
		
		var existing = await context.Clients
			.SingleOrDefaultAsync(x => x.Id == SpaClientId, ct);

		if (existing is not null)
			return existing;
		
		var spa = Client.CreateSpa(
			SpaClientId,
			new ClientName("SPA Demo"),
			now
		);
		
		spa.SetRedirectUris(
			[RedirectUri.Parse("http://localhost:5156/authentication/login-callback")],
			now
		);
		
		context.Clients.Add(spa);
		return spa;
	}

	private async Task<ApiResource> EnsureWeatherApiAsync(CancellationToken ct)
	{
		var existing = await context.ApiResources
			.SingleOrDefaultAsync(x => x.Audience == WeatherAudience, ct);

		if (existing is not null)
			return existing;

		var api = ApiResource.Create(
			new ApiResourceName("Weather Demo API"),
			WeatherAudience,
			[new Permission(WeatherReadScope, new ScopeDescription("Read weather data"))]
		);

		context.ApiResources.Add(api);
		return api;
	}

	private void EnsureClientApiAccess(Client client, ApiResource api)
	{
		client.SetApiAccess(api.Id, api.GetScopes(), time.GetUtcNow());
	}
}