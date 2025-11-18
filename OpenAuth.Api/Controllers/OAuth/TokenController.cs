using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Controllers.OAuth;

[ApiController]
[Route("connect")]
public class TokenController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<TokenResponse>> IssueToken([FromBody] string code)
    {
        // Client Credentials
        var ccRequest = new ClientCredentialsTokenRequest()
        {
            ClientId = new ClientId(Guid.Parse("55b12c3f-11ba-4b98-8453-728ff2e2f2be")),
            ClientSecret = "sdrN2svMaLqIGSc1DOpRTVvcrZQV1X6MI02vSe3IyS0",
            RequestedAudience = new AudienceName("test-audience"),
            RequestedScopes = ScopeCollection.Parse("read")
        };

        // Authorization Code
        var acRequest = new AuthorizationCodeTokenRequest()
        {
            ClientId = new ClientId(Guid.Parse("c2b09d6b-e5cb-4154-a03f-7a824fc9c5df")),
            Code = code,
            Subject = "Test-Subject",
            RedirectUri = RedirectUri.Create("https://example.com/callback"),
            RequestedAudience = new AudienceName("api"),
            RequestedScopes = ScopeCollection.Parse("read"),
            CodeVerifier = "this-is-a-secret-code"
            // ClientSecret = "68loWhVtCow-q3AzYy3RE7vd7tV94tZ9WtX1Mj9PEJA",
        };
        
        var result = await tokenService.IssueToken(ccRequest);
        return Ok(new TokenResponse(result.Token, result.TokenType, result.ExpiresIn));
    }
}