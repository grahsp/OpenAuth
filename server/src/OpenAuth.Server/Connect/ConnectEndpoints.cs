using OpenAuth.Server.Connect.Authorize;
using OpenAuth.Server.Connect.Logout;
using OpenAuth.Server.Connect.Token;
using OpenAuth.Server.Connect.UserInfo;

namespace OpenAuth.Server.Connect;

public static class ConnectEndpoints
{
	public static IEndpointRouteBuilder MapConnectEndpoints(this IEndpointRouteBuilder app)
	{
		var root = app.MapGroup("/connect");
		
		root.MapAuthorizeEndpoint();
		
		root.MapLogoutEndpoint();
		
		root.MapTokenEndpoint()
			.RequireCors("Public");
		
		root.MapUserInfoEndpoint()
			.RequireCors("Public");
		
		return app;
	}
}