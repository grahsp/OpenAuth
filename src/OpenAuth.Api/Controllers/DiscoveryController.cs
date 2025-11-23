using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenAuth.Api.Dtos;
using OpenAuth.Infrastructure.Configurations;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route(".well-known")]
public class DiscoveryController : ControllerBase
{
    public DiscoveryController(IOptions<JwtOptions> config)
    {
        _config = config.Value;
    }

    private readonly JwtOptions _config;
    
    
    [HttpGet("openid-configuration")]
    public ActionResult<DiscoveryResult> Get()
    {
        return new DiscoveryResult
        (
            _config.Issuer,
            $"{_config.Issuer}/.well-known/jwks.json",
            $"{_config.Issuer}/connect/token"
        );
    }
}