using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Oidc;

public static class OidcScopes
{
    public static readonly Scope OpenId = new("openid");
    public static readonly Scope Profile = new("profile");
    public static readonly Scope Email = new("email");
    public static readonly Scope Phone = new("phone");
    public static readonly Scope Address = new("address");

    public static readonly ScopeCollection All =
        new([OpenId, Profile, Email, Phone, Address]);
}