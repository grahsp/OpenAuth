using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenAuth.Test.Common.Infrastructure;

/// <summary>
/// Development-only authentication handler that reads user info from headers.
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// The authentication scheme name used for integration tests.
    /// </summary>
    public const string SchemeName = "Integration-Testing";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">Authentication scheme options.</param>
    /// <param name="logger">Logger factory.</param>
    /// <param name="encoder">URL encoder.</param>
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) {}

    /// <summary>
    /// Builds an authentication ticket from development headers.
    /// </summary>
    /// <returns>The authentication result.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-Test-UserId"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(
                AuthenticateResult.Fail("Missing X-Test-UserId header."));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
