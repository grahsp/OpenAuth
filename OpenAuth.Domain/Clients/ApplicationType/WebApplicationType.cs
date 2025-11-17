using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Domain.Clients.ApplicationType;

public class WebApplicationType : ClientApplicationType
{
    public override string Name => "web";

    public override IReadOnlyCollection<GrantType> AllowedGrants =>
        [GrantType.AuthorizationCode, GrantType.ClientCredentials, GrantType.RefreshToken];

    public override bool RequiresPermissions => false;
    public override bool AllowsClientSecrets => true;

    public override IReadOnlyCollection<GrantType> DefaultGrantTypes =>
        [GrantType.AuthorizationCode, GrantType.RefreshToken];
}