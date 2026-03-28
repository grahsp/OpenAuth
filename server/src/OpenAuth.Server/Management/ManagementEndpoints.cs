using OpenAuth.Server.Management.ApiResources;
using OpenAuth.Server.Management.Clients;
using OpenAuth.Server.Management.Secrets;
using OpenAuth.Server.Management.SigningKeys;

namespace OpenAuth.Server.Management;

public static class ManagementEndpoints
{
	public static IEndpointRouteBuilder MapManagementEndpoints(this IEndpointRouteBuilder app)
	{
		var root = app.MapGroup("/api")
			.RequireAuthorization(policy => policy.RequireRole("admin"))
			.RequireCors("Dashboard");
		
		root.MapClientEndpoints();
		root.MapApiEndpoints();
		root.MapSigningKeyEndpoints();
		root.MapSecretEndpoints();

		return app;
	}
}