using OpenAuth.Domain.Clients.Secrets;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.ApplicationType;

public abstract class ClientApplicationType
{
    public abstract string Name { get; }

    public abstract IReadOnlyCollection<GrantType> AllowedGrants { get; }
    public abstract IReadOnlyCollection<GrantType> DefaultGrantTypes { get; }
    
    public abstract bool RequiresRedirectUris { get; }
    public abstract bool RequiresPermissions { get; }
    public abstract bool AllowsClientSecrets { get; }

    public bool IsPublic => !AllowsClientSecrets;

    public virtual OAuthConfiguration CreateDefaultConfiguration(IEnumerable<Audience> audiences,
        IEnumerable<RedirectUri> redirectUris)
        => new(audiences, DefaultGrantTypes, redirectUris, true);
    
    public void ValidateAudiences(IEnumerable<Audience> audiences)
    {
        if (RequiresPermissions && !audiences.Any())
            throw new InvalidOperationException("Client must have at least one audience.");
    }

    public void ValidateGrantTypes(IEnumerable<GrantType> grantTypes)
    {
        grantTypes = grantTypes.ToList();
        
        var invalid = grantTypes.Except(AllowedGrants).ToList();
        if (invalid.Count > 0)
        {
            var invalidNames = string.Join(", ", invalid.Select(g => g.Value));
            throw new InvalidOperationException($"Grant type(s) {invalidNames} are not allowed for this client type.");
        }
        
        if (RequiresRedirectUris && grantTypes.Any(g => !g.RequiresRedirectUri))
            throw new InvalidOperationException("Grant type(s) require redirect URIs.");
    }

    public void ValidateRedirectUris(IEnumerable<RedirectUri> redirectUris)
    {
        redirectUris = redirectUris.ToList();
        
        if (RequiresRedirectUris && !redirectUris.Any())
            throw new InvalidOperationException("Client must have at least one redirect URI.");
    }
    
    public void ValidateSecrets(IEnumerable<Secret> secrets)
    {
        secrets = secrets.ToList();
        
        if (!AllowsClientSecrets && secrets.Any())
            throw new InvalidOperationException("Client type does not allow client secrets.");
        
        if (AllowsClientSecrets && secrets.Any())
            throw new InvalidOperationException("Client must have at least one secret.");
    }
}