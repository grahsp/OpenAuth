using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public class SecuritySettingsBuilder
{
    private IEnumerable<Secret> _secrets = [];
    private TimeSpan? _lifetime;
    
    public SecuritySettingsBuilder WithSecrets(params Secret[] secrets)
    {
        _secrets = secrets;
        return this;
    }
    
    public SecuritySettingsBuilder WithTokenLifetime(TimeSpan lifetime)
    {
        _lifetime = lifetime;
        return this;
    }

    public SecuritySettings Build()
    {
        var lifetime = _lifetime ?? TimeSpan.FromMinutes(10);
        
        return new SecuritySettings
        {
            Secrets = _secrets.ToList(),
            TokenLifetime = lifetime
        };
    }
}