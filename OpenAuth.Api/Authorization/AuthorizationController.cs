using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.OAuth.Authorization.Handlers;

namespace OpenAuth.Api.Authorization;

[Controller]
[Route("authorize")]
public class AuthorizationController(IAuthorizationHandler handler) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizeRequestDto request)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.Path + Request.QueryString
            });
        }

        var subject = User.Identity.Name;
        if (string.IsNullOrWhiteSpace(subject))
            return BadRequest("Subject is missing.");
        
        var command = request.ToCommand(subject);

        try
        {
            var grant = await handler.AuthorizeAsync(command);
            var redirectUri = grant.ToRedirectUri(request.State);
        
            return Redirect(redirectUri);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }       
    }
}