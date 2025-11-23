using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.ApplicationType;

public class SinglePageClientApplicationType : ClientApplicationType
{
    public override string Name => "spa";

    public override IReadOnlyCollection<GrantType> AllowedGrants =>
        [GrantType.AuthorizationCode, GrantType.RefreshToken];

    public override bool RequiresPermissions => false;
    public override bool AllowsClientSecrets => false;

    public override IReadOnlyCollection<GrantType> DefaultGrantTypes =>
        [GrantType.AuthorizationCode, GrantType.RefreshToken];
}