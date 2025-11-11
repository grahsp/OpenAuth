namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record ClientConfiguration(
    ClientIdentity Identity,
    SecuritySettings Security,
    OAuthConfiguration OAuth,
    bool Enabled,
    DateTimeOffset CreatedAt
);