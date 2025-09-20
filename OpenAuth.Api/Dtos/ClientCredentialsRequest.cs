using System.ComponentModel.DataAnnotations;

namespace OpenAuth.Api.Dtos;

public record ClientCredentialsRequest (
    [Required] string ClientId,
    [Required] string ClientSecret,
    [Required] string GrantType,
    string? Audience = null,
    string[]? Scopes = null
);