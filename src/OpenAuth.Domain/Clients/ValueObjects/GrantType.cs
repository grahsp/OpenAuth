using OpenAuth.Domain.OAuth;

namespace OpenAuth.Domain.Clients.ValueObjects;

public readonly record struct GrantType
{
    public string Value { get; }
    public bool RequiresRedirectUri { get; }
    
    public bool IsValid => Value is not null;

    private GrantType(string value, bool requiresRedirectUri)
    {
        Value = value;
        RequiresRedirectUri = requiresRedirectUri;
    }
    
    public static readonly GrantType AuthorizationCode = new(GrantTypes.AuthorizationCode, true);
    public static readonly GrantType ClientCredentials = new(GrantTypes.ClientCredentials, false);
    public static readonly GrantType RefreshToken = new(GrantTypes.RefreshToken, false);
    
    public static GrantType Parse(string value)
    {
        if (!TryParse(value, out var grantType))
            throw new ArgumentException($"Invalid grant type: '{value}'");
        
        return grantType;
    }

    public static bool TryParse(string value, out GrantType grantType)
    {
        grantType = value switch
        {
            GrantTypes.AuthorizationCode => AuthorizationCode,
            GrantTypes.ClientCredentials => ClientCredentials,
            GrantTypes.RefreshToken => RefreshToken,
            _ => default
        };
        
        return grantType.IsValid;
    }
    
    public static implicit operator string(GrantType grant) => grant.Value;    
    public override string ToString() => Value;
}