using System.Collections.Immutable;
using OpenAuth.Domain.Clients.Secrets;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record SecuritySettings
{
    public IReadOnlyCollection<Secret> Secrets;
    public TimeSpan TokenLifetime;
    
    private TimeSpan DefaultTokenLifetime => TimeSpan.FromMinutes(15);
    
    public SecuritySettings(IEnumerable<Secret> secrets, TimeSpan? tokenLifetime)
    {
        Secrets = ValidateSecrets(secrets);
        TokenLifetime = ValidateLifetime(tokenLifetime ?? DefaultTokenLifetime);
    }
    
    public SecuritySettings AddSecret(Secret secret)
    {
        var updatedSecrets = Secrets.Append(secret);
        return this with { Secrets = ValidateSecrets(updatedSecrets) };
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