using OpenAuth.Server.Discovery.Discovery;
using OpenAuth.Server.Discovery.Jwks;

namespace OpenAuth.Server.Discovery;

public static class DiscoveryEndpoints
{
	public static IEndpointRouteBuilder MapDiscoveryEndpoints(this IEndpointRouteBuilder app)
	{
		var root = app.MapGroup("/.well-known");

		root.MapOpenIdConfigurationEndpoint();
		root.MapJwksEndpoint();
		
		return app;
	}
}