using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.ApplicationType;

public abstract class ClientApplicationType
{
    public abstract string Name { get; }

    public abstract IReadOnlyCollection<GrantType> AllowedGrants { get; }
    public abstract IReadOnlyCollection<GrantType> DefaultGrantTypes { get; }
    
    public abstract bool RequiresPermissions { get; }
    public abstract bool AllowsClientSecrets { get; }
}