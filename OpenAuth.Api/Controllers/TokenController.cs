using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.OAuth.Authorization.Dtos;
using OpenAuth.Application.OAuth.Authorization.Handlers;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("connect")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthorizationHandler _authorizationHandler;
    
    public TokenController(ITokenService tokenService, IAuthorizationHandler authorizationHandler)
    {
        _tokenService = tokenService;
        _authorizationHandler = authorizationHandler;
    }

    
    [HttpPost("token")]
    public async Task<ActionResult<TokenResponse>> IssueToken([FromBody] string code)
    {
        // Client Credentials
        var ccRequest = new ClientCredentialsTokenRequest()
        {
            ClientId = new ClientId(Guid.Parse("c2b09d6b-e5cb-4154-a03f-7a824fc9c5df")),
            ClientSecret = "68loWhVtCow-q3AzYy3RE7vd7tV94tZ9WtX1Mj9PEJA",
            RequestedAudience = new AudienceName("api"),
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
        
        var result = await _tokenService.IssueToken(ccRequest);
        return Ok(new TokenResponse(result.Token, result.TokenType, result.ExpiresIn));
    }

    [HttpPost("authorize")]
    public async Task<ActionResult> Authorize()
    {
        var codeChallenge = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes("this-is-a-secret-code")));
        var request = new AuthorizationRequest(
            new ClientId(Guid.Parse("c2b09d6b-e5cb-4154-a03f-7a824fc9c5df")),
            RedirectUri.Create("https://example.com/callback"),
            new AudienceName("api"),
            ScopeCollection.Parse("read"),
            Pkce.Create(codeChallenge, CodeChallengeMethod.S256)
        );

        var result = await _authorizationHandler.AuthorizeAsync(request, "subject");
        return Ok(result);
    }
}