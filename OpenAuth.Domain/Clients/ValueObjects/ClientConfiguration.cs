namespace OpenAuth.Domain.Clients.ValueObjects;

public sealed record ClientConfiguration(
    ClientName Name,
    SecuritySettings Security,
    OAuthConfiguration OAuth
);