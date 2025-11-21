using OpenAuth.Application.Tokens.Services;

namespace OpenAuth.Api.Connect.Token;

public static class TokenEndpoint
{
    public static IEndpointRouteBuilder MapTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/token", async (
            ITokenService service,
            HttpRequest req,
            [AsParameters] TokenRequest dto) =>
        {
            var form = await req.ReadFormAsync(); 
            var command = dto.ToCommand();

            var result = await service.IssueToken(command);

            return Results.Ok(result);
        })
        .DisableAntiforgery();
        
        return app;
    }
}