using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Application.Tokens.Services;

/// <summary>
/// Defines a contract for creating framework-level
/// <see cref="SecurityKey"/> from domain-level <see cref="SigningKey"/> instances.
/// </summary>
public interface IValidationKeyFactory
{
    /// <summary>
    /// Creates a <see cref="SecurityKey"/> for token validation
    /// using the appropriate <see cref="ISigningKeyHandler"/> for the specified <see cref="SigningKey"/>.
    /// </summary>
    /// <param name="signingKey">The domain-level signing key.</param>
    /// <returns>A framework-level <see cref="SecurityKey"/> suitable for JWT validation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="signingKey"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no handler is registered for the signing key's <see cref="KeyType"/>.
    /// </exception>
    SecurityKey Create(SigningKey signingKey);
}