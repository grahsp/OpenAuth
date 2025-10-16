namespace OpenAuth.Domain.Clients.ValueObjects;

public static class GrantTypes
{
    public const string ClientCredentials = "client_credentials";
    public const string AuthorizationCode = "authorization_code";
    public const string RefreshToken = "refresh_token";
}

public record GrantType
{
    public string Value { get; }
    public bool SupportsPublicClient { get; }
    public bool SupportsPkce { get; }

    private GrantType(string value, bool supportsPublicClient, bool supportsPkce)
    {
        Value = value;
        SupportsPublicClient = supportsPublicClient;
        SupportsPkce = supportsPkce;
    }
    
    public static readonly GrantType AuthorizationCode = new(GrantTypes.AuthorizationCode, true, true);
    public static readonly GrantType ClientCredentials = new(GrantTypes.ClientCredentials, false, false);
    public static readonly GrantType RefreshToken = new(GrantTypes.RefreshToken, true, true);
    
    public static GrantType Create(string value)
    {
        return value switch
        {
            GrantTypes.AuthorizationCode => AuthorizationCode,
            GrantTypes.ClientCredentials => ClientCredentials,
            GrantTypes.RefreshToken => RefreshToken,
            _ => throw new ArgumentException($"Unknown grant type: {value}.", nameof(value))
        };
    }
}