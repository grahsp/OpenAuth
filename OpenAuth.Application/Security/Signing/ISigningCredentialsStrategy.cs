using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Dtos;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Signing;

/// <summary>
/// Defines a strategy for converting domain-level <see cref="KeyMaterial"/> 
/// into framework-level <see cref="SigningCredentials"/> for a specific <see cref="KeyType"/>.
/// </summary>
public interface ISigningCredentialsStrategy
{
    /// <summary>
    /// Gets the <see cref="KeyType"/> supported by this strategy.
    /// </summary>
    KeyType KeyType { get; }
    
    /// <summary>
    /// Creates signing credentials from the provided key data.
    /// </summary>
    /// <param name="keyData">
    /// The signing key data. The key type must match this strategy's supported <see cref="KeyType"/>.
    /// </param>
    /// <returns>
    /// A <see cref="SigningCredentials"/> instance configured with the key material and algorithm.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="keyData"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the key type in <paramref name="keyData"/> does not match this strategy's supported type.
    /// </exception>
    SigningCredentials GetSigningCredentials(SigningKeyData keyData);
}