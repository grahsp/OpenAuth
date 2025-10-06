using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Domain.SigningKeys;

namespace OpenAuth.Application.Security.Signing;

/// <summary>
/// Defines a contract for creating framework-level
/// <see cref="SigningCredentials"/> from domain-level <see cref="SigningKey"/> instances.
/// </summary>
public interface ISigningCredentialsFactory
{
    /// <summary>
    /// Creates <see cref="SigningCredentials"/> from signing key data.
    /// </summary>
    /// <param name="keyData">The signing key data containing the key material and algorithm information.</param>
    /// <returns>
    /// A <see cref="SigningCredentials"/> instance configured with the appropriate cryptographic key
    /// and algorithm for JWT token signing.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="keyData"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the key type specified in <paramref name="keyData"/> is not supported.
    /// </exception>
    SigningCredentials Create(SigningKeyData keyData);
}