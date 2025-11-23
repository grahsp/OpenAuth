using System.Diagnostics.CodeAnalysis;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.Shared.Interfaces;

namespace OpenAuth.Domain.Clients.ValueObjects;

public record GrantType : ICreate<string, GrantType>
{
    public string Value { get; private init; }
    public bool RequiresRedirectUri { get; private init; }

    private GrantType(string value, bool requiresRedirectUri)
    {
        Value = value;
        RequiresRedirectUri = requiresRedirectUri;
    }
    
    public static readonly GrantType AuthorizationCode = new(GrantTypes.AuthorizationCode, true);
    public static readonly GrantType ClientCredentials = new(GrantTypes.ClientCredentials, false);
    public static readonly GrantType RefreshToken = new(GrantTypes.RefreshToken, false);
    
    public static GrantType Create(string value)
    {
        if (!TryCreate(value, out var grantType))
            throw new ArgumentException($"Invalid grant type: '{value}'");
        
        return grantType;
    }

    public static bool TryCreate(string value, [NotNullWhen(true)] out GrantType? grantType)
    {
        grantType = value switch
        {
            GrantTypes.AuthorizationCode => AuthorizationCode,
            GrantTypes.ClientCredentials => ClientCredentials,
            GrantTypes.RefreshToken => RefreshToken,
            _ => null
        };
        
        return grantType is not null;
    }
}