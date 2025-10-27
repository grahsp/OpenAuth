using Microsoft.AspNetCore.Mvc;
using OpenAuth.Application.Users.Services;

namespace OpenAuth.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpGet("{userId:guid}")]
    public Task<IActionResult> Get(Guid userId)
    {
        return Task.FromResult<IActionResult>(Ok());
    }
    
    [HttpPost]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        var result = await _userService.RegisterUserAsync(request.Username, request.Email, request.Password);
        if (result is { IsSuccess: false, Error: not null })
            return BadRequest(new { code = result.Error.Code, message = result.Error.Message });

        var user = result.Value;
        var response = new RegisterUserResponse(user.Id.Value, user.Username, user.Email.Value, user.CreatedAt);
        
        return CreatedAtAction(nameof(Get), new { userId = user.Id }, response);
    }
}

public sealed record RegisterUserRequest(string Username, string Email, string Password);
public sealed record RegisterUserResponse(Guid Id, string Username, string Email, DateTimeOffset CreatedAt);