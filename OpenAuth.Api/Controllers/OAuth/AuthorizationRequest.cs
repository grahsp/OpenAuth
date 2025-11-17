namespace OpenAuth.Api.Controllers.OAuth;

public record AuthorizationRequest
(
    string ClientId,
    string Subject,
    string RedirectUri,
    string? Audience,
    string? Scope,
    string? State,
    string? CodeChallenge,
    string? CodeChallengeMethod
);