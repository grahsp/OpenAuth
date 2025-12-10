namespace OpenAuth.Domain.OAuth;

public record JwtDescriptor(
    string? Audience,
    string? Subject,
    int LifetimeInSeconds,
    Dictionary<string, object> Claims
);