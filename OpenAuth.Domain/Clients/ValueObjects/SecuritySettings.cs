using System.Collections.Immutable;
using OpenAuth.Domain.Clients.Secrets;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record SecuritySettings(
    IReadOnlyCollection<Secret> Secrets,
    TimeSpan? TokenLifetime)
{
    public SecuritySettings AddSecrets(params Secret[] secrets)
        => this with {
            Secrets = Secrets
                .Union(secrets)
                .ToImmutableArray()
        };

    public SecuritySettings SetTokenLifetime(TimeSpan? lifetime)
    {
        if (lifetime is not null && (lifetime <= TimeSpan.Zero || lifetime > TimeSpan.FromDays(30)))
            throw new ArgumentOutOfRangeException(nameof(lifetime), "TokenLifetime must be positive and less than 30 days.");
            
        return this with { TokenLifetime = lifetime };
    }
}