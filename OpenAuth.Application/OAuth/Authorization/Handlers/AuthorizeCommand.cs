namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public record AuthorizeCommand(
    string ResponseType,
    string ClientId,
    string Subject,
    string RedirectUri,
    string? Audience,
    string? Scope,
    string? CodeChallenge,
    string? CodeChallengeMethod
);