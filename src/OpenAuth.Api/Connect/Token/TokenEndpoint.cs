using OpenAuth.Application.Exceptions;
using OpenAuth.Application.Tokens.Services;

namespace OpenAuth.Api.Connect.Token;

public static class TokenEndpoint
{
    public static IEndpointRouteBuilder MapTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/token", async (
                ITokenService service,
                [AsParameters] TokenRequest dto) =>
            {
                try
                {
                    var command = dto.ToCommand();
                    var result = await service.IssueToken(command);

                    var response = TokenResponse.Success(result.Token, result.TokenType, result.ExpiresIn);
                    return Results.Ok(response);
                }
                catch (OAuthException ex)
                {
                    return Results.BadRequest(TokenResponse.Fail(ex.Error, ex.Description));
                }
                catch (ServerErrorException ex)
                {
                    return Results.InternalServerError(TokenResponse.Fail(ex.Message));
                }
                catch (Exception)
                {
                    return Results.InternalServerError(TokenResponse.Fail("An unexpected error occurred."));
                }
            })
            .DisableAntiforgery();
        
        return app;
    }
}