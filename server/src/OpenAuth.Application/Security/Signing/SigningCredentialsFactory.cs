using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Security.Signing;

/// <summary>
/// Default implementation of <see cref="ISigningCredentialsFactory"/>.
/// Uses registered <see cref="ISigningKeyHandler"/> implementations
/// to convert domain-level <see cref="SigningKey"/> instances into framework-level
/// <see cref="SigningCredentials"/>.
/// </summary>
public class SigningCredentialsFactory : ISigningCredentialsFactory
{
    private readonly Dictionary<KeyType, ISigningKeyHandler> _handlers;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SigningCredentialsFactory"/> class.
    /// </summary>
    /// <param name="handlers">
    /// A collection of <see cref="ISigningKeyHandler"/> implementations
    /// that handle supported <see cref="KeyType"/> values.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if multiple handlers are registered for the same <see cref="KeyType"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handlers provided.
    /// </exception>
    public SigningCredentialsFactory(IEnumerable<ISigningKeyHandler> handlers)
    {
        try
        {
            _handlers = handlers.ToDictionary(s => s.KeyType);
        }
        catch (ArgumentException e)
        {
            throw new ArgumentException("Multiple handlers registered using the same key type.", e);
        }
        
        if (_handlers.Count == 0)
            throw new InvalidOperationException("No signing key handlers registered."); 
    }
    
    /// <inheritdoc />
    public SigningCredentials Create(SigningKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        
        if (!_handlers.TryGetValue(key.KeyMaterial.Kty, out var strategy))
            throw new InvalidOperationException($"No handler registered for key type '{key.KeyMaterial.Kty}'.");
        
        return strategy.CreateSigningCredentials(key);
    }
}