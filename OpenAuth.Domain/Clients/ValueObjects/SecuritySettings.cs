using System.Collections.Immutable;
using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record SecuritySettings
{
    public IReadOnlyCollection<Secret> Secrets { get; init; } = [];
    public TimeSpan TokenLifetime { get; init; } = TimeSpan.FromMinutes(15);


    public SecuritySettings AddSecret(Secret secret)
    {
        var updatedSecrets = Secrets.Append(secret);
        return this with { Secrets = ValidateSecrets(updatedSecrets) };
    }

    public bool RevokeSecret(SecretId secretId, DateTimeOffset utcNow)
    {
        var secret = Secrets.FirstOrDefault(x => x.Id == secretId)
            ?? throw new InvalidOperationException($"Secret { secretId } does not exist in client.");

        if (!secret.IsActive(utcNow))
            return false;
        
        secret.Revoke(utcNow);
        return true;
    }

    private IReadOnlyCollection<Secret> ValidateSecrets(IEnumerable<Secret> secrets)
    {
        var items = secrets.ToImmutableList();
        
        var distinct = items.DistinctBy(s => s.Id);
        if (distinct.Count() != items.Count)
            throw new ArgumentException("Duplicate secret IDs found.");

        return items;
    }

    public SecuritySettings SetTokenLifetime(TimeSpan lifetime)
        => this with { TokenLifetime = ValidateLifetime(lifetime)};

    private TimeSpan ValidateLifetime(TimeSpan lifetime)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero, nameof(lifetime));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lifetime, TimeSpan.FromDays(30), nameof(lifetime));
        
        return lifetime;
    }
}