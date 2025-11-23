using Microsoft.AspNetCore.Mvc;

namespace OpenAuth.Api.Connect.Token;

public record TokenRequest
{
    // Base
    [FromForm(Name = "grant_type")]
    public string? GrantType { get; init; }
    
    [FromForm(Name = "client_id")]
    public string? ClientId { get; init; }
    
    [FromForm(Name = "audience")]
    public string? Audience { get; init; }
    
    [FromForm(Name = "scope")]
    public string? Scope { get; init; }
    
    // Confidential
    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; init; }
    
    // Public
    [FromForm(Name = "code")]
    public string? Code { get; init; }
    
    [FromForm(Name = "redirect_uri")]
    public string? RedirectUri { get; init; }
    
    [FromForm(Name = "code_verifier")]
    public string? CodeVerifier { get; init; }
}