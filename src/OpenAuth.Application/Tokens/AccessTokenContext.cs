namespace OpenAuth.Application.Tokens;

public sealed record AccessTokenContext(
    string ClientId,
    string Audience,
    string? Subject,
    int LifetimeInSeconds,
    IReadOnlyCollection<string> Scopes
);