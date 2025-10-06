using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.Dtos;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("connect")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;
    
    public TokenController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    
    [HttpPost("token")]
    public async Task<ActionResult<TokenResponse>> IssueToken([FromBody] ClientCredentialsRequest request)
    {
        var tokenRequest = new IssueTokenRequest(
            new ClientId(Guid.Parse(request.ClientId)),
            request.ClientSecret,
            new AudienceName(request.Audience),
            request.Scopes.Select(s => new Scope(s)).ToArray()
        );
        
        var result = await _tokenService.IssueToken(tokenRequest);
        return Ok(new TokenResponse(result.Token, result.TokenType, result.ExpiresIn));
    }
}