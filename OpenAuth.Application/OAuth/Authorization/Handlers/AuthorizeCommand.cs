namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public record AuthorizeCommand(
    string ClientId,
    string Subject,
    string RedirectUri,
    string? Audience,
    string? Scope,
    string? State,
    string? CodeChallenge,
    string? CodeChallengeMethod
);