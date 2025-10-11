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

    private GrantType(string value)
    {
        Value = value;
    }
    
    public static readonly GrantType AuthorizationCode = new(GrantTypes.AuthorizationCode);
    public static readonly GrantType ClientCredentials = new(GrantTypes.ClientCredentials);
    public static readonly GrantType RefreshToken = new(GrantTypes.RefreshToken);
    
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