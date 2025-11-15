using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.ApplicationType;

public class MachineToMachineClientApplicationType : ClientApplicationType
{
    public override string Name => "m2m";

    public override IReadOnlyCollection<GrantType> AllowedGrants =>
        [GrantType.ClientCredentials, GrantType.RefreshToken];

    public override bool RequiresRedirectUris => false;
    public override bool RequiresPermissions => true;
    public override bool AllowsClientSecrets => true;

    public override IReadOnlyCollection<GrantType> DefaultGrantTypes =>
        [GrantType.ClientCredentials];
}