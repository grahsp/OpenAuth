using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Signing;

public class SigningCredentialsFactory : ISigningCredentialsFactory
{
    private readonly IDictionary<KeyType, ISigningCredentialsStrategy> _strategies;
    
    public SigningCredentialsFactory(IEnumerable<ISigningCredentialsStrategy> strategies)
    {
        try
        {
            _strategies = strategies
                .ToDictionary(s => s.KeyType);
        }
        catch (ArgumentException e)
        {
            throw new InvalidOperationException("Duplicate signing credentials strategy registered.", e);
        }
        
        if (_strategies.Count == 0)
            throw new InvalidOperationException("No signing credentials strategies were registered."); 
    }
    
    public SigningCredentials Create(SigningKey signingKey)
    {
        if (!_strategies.TryGetValue(signingKey.KeyMaterial.Kty, out var strategy))
            throw new InvalidOperationException(
                $"""
                 No signing credentials strategy found for key type '{signingKey.KeyMaterial.Kty}'
                                 (algorithm: {signingKey.KeyMaterial.Alg}).
                 """);

        return strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial);
    }
}