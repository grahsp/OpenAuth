using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Signing;

/// <summary>
/// Default implementation of <see cref="ISigningCredentialsFactory"/>.
/// Uses registered <see cref="ISigningCredentialsStrategy"/> implementations
/// to convert domain-level <see cref="SigningKey"/> instances into framework-level
/// <see cref="SigningCredentials"/>.
/// </summary>
public class SigningCredentialsFactory : ISigningCredentialsFactory
{
    private readonly Dictionary<KeyType, ISigningCredentialsStrategy> _strategies;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SigningCredentialsFactory"/> class.
    /// </summary>
    /// <param name="strategies">
    /// A collection of <see cref="ISigningCredentialsStrategy"/> implementations
    /// that handle supported <see cref="KeyType"/> values.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if duplicate strategies are registered for the same <see cref="KeyType"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no strategies are provided.
    /// </exception>
    public SigningCredentialsFactory(IEnumerable<ISigningCredentialsStrategy> strategies)
    {
        try
        {
            _strategies = strategies
                .ToDictionary(s => s.KeyType);
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException("Duplicate signing credentials strategy registered.", e);
        }
        
        if (_strategies.Count == 0)
            throw new InvalidOperationException("No signing credentials strategies were registered."); 
    }
    
    /// <inheritdoc />
    public SigningCredentials Create(SigningKey signingKey)
    {
        ArgumentNullException.ThrowIfNull(signingKey);
        
        if (!_strategies.TryGetValue(signingKey.KeyMaterial.Kty, out var strategy))
            throw new InvalidOperationException(
                $"""
                 No signing credentials strategy found for key type '{signingKey.KeyMaterial.Kty}'
                                 (algorithm: {signingKey.KeyMaterial.Alg}).
                 """);

        return strategy.GetSigningCredentials(signingKey.Id.ToString(), signingKey.KeyMaterial);
    }
}