using Microsoft.AspNetCore.Mvc;
using OpenAuth.Api.Dtos;
using OpenAuth.Application.Clients;
using OpenAuth.Application.Security.Tokens;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("connect")]
public class TokenController : ControllerBase
{
    public TokenController(IClientService clientService, IClientSecretValidator secretValidator, ISigningKeyService signingKeyService, IJwtTokenGenerator tokenGenerator)
    {
        _clientService = clientService;
        _secretValidator = secretValidator;
        _signingKeyService = signingKeyService;
        _tokenGenerator = tokenGenerator;
    }

    private readonly IClientService _clientService;
    private readonly IClientSecretValidator _secretValidator;
    private readonly ISigningKeyService _signingKeyService;
    private readonly IJwtTokenGenerator _tokenGenerator;
    
    // Tokens
    [HttpPost("token")]
    public async Task<ActionResult<string>> IssueToken([FromBody] ClientCredentialsRequest request)
    {
        if (!request.GrantType.Equals("client_credentials", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new TokenErrorResponse("unsupported_grant_type", "Unsupported grant type used.", ""));
        
        var client = await _clientService.GetByIdAsync(new ClientId(Guid.Parse(request.ClientId)));
        if (client is null)
            return BadRequest(new TokenErrorResponse("invalid_client", "Authentication failed.", ""));

        var valid = _secretValidator.Verify(request.ClientSecret, client);
        if (!valid)
            return BadRequest(new TokenErrorResponse("invalid_client", "Authentication failed.", ""));

        var signingKey = await _signingKeyService.GetCurrentAsync();
        if (signingKey is null)
            return BadRequest(new TokenErrorResponse("server_error", "No active signing key found.", ""));
        
        var audience = client.Audiences.SingleOrDefault(x => x.Value == request.Audience);
        if (audience is null)
            return Unauthorized(new TokenErrorResponse("invalid_request", "Unknown audience.", ""));

        var scopes = request.Scopes?.Select(x => new Scope(x)).ToArray() ?? [];
        if (scopes.Any(x => !audience.Scopes.Contains(x)))
            return Unauthorized(new TokenErrorResponse("invalid_scope", "One or more scopes are invalid.", ""));
        
        var token = _tokenGenerator.GenerateToken(client, audience, scopes, signingKey);
        var response = new TokenResponse(token, "Bearer", client.TokenLifetime.Seconds);
        
        return Ok(response);
    }
}