namespace OpenAuth.Api.Authorization;

public record AuthorizeRequestDto
(
    string ClientId,
    string RedirectUri,
    string? Audience,
    string? Scope,
    string? State,
    string? CodeChallenge,
    string? CodeChallengeMethod
);