namespace OpenAuth.Domain.Clients.ValueObjects;

public record GrantType
{
    public string Value { get; }

    private GrantType() { }

    private GrantType(string value)
    {
        Value = value;
    }
    
    public static GrantType AuthorizationCode()
        => new("authorization_code");
    
    public static GrantType ClientCredentials()
        => new("client_credentials");
    
    // EF conversion
    public static GrantType Create(string value)
    {
        return value switch
        {
            "authorization_code" => AuthorizationCode(),
            "client_credentials" => ClientCredentials(),
            _ => throw new ArgumentException("Unknown grant type.")
        };
    }
}