using Microsoft.AspNetCore.Mvc;

namespace OpenAuth.Api.Connect.Authorize;

public record AuthorizeRequest
{
    [FromQuery(Name = "response_type")]
    public string ResponseType { get; init; } = null!;

    [FromQuery(Name = "client_id")]
    public string ClientId { get; init; } = null!;
    
    [FromQuery(Name = "redirect_uri")]
    public string RedirectUri { get; init; } = null!;

    [FromQuery(Name = "scope")]
    public string Scopes { get; init; } = null!;
    
    [FromQuery(Name = "state")]
    public string? State { get; init; }
    
    [FromQuery(Name = "code_challenge")]
    public string? CodeChallenge { get; init; }
    
    [FromQuery(Name = "code_challenge_method")]
    public string? CodeChallengeMethod { get; init; }
    
    [FromQuery(Name = "nonce")]
    public string? Nonce { get; init; }
}