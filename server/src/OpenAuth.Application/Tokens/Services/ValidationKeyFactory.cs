using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;

namespace OpenAuth.Application.Tokens.Services;

/// <summary>
/// Default implementation of <see cref="IValidationKeyFactory"/>.
/// Uses registered <see cref="ISigningKeyHandler"/> implementations
/// to convert domain-level <see cref="SigningKey"/> instances into framework-level
/// <see cref="SecurityKey"/> representations suitable for token validation.
/// </summary>
public sealed class ValidationKeyFactory : IValidationKeyFactory
{
    private readonly Dictionary<KeyType, ISigningKeyHandler> _handlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationKeyFactory"/> class.
    /// </summary>
    /// <param name="handlers">
    /// A collection of <see cref="ISigningKeyHandler"/> implementations
    /// that support creation of validation keys for their respective <see cref="KeyType"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if multiple handlers are registered for the same <see cref="KeyType"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handlers are provided.
    /// </exception>
    public ValidationKeyFactory(IEnumerable<ISigningKeyHandler> handlers)
    {
        try
        {
            _handlers = handlers.ToDictionary(h => h.KeyType);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Multiple validation key handlers registered using the same key type.", ex);
        }

        if (_handlers.Count == 0)
            throw new InvalidOperationException("No validation key handlers were registered.");
    }

    /// <inheritdoc/>>
    public SecurityKey Create(SigningKey signingKey)
    {
        ArgumentNullException.ThrowIfNull(signingKey);

        if (!_handlers.TryGetValue(signingKey.KeyMaterial.Kty, out var handler))
            throw new InvalidOperationException($"No handler registered for key type '{signingKey.KeyMaterial.Kty}'.");

        return handler.CreateValidationKey(signingKey);
    }
}