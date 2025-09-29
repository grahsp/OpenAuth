using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Entities;
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
    /// Creates <see cref="SigningCredentials"/> for the specified key material.
    /// </summary>
    /// <param name="kid">
    /// The key identifier (<c>kid</c>) to assign to the resulting security key.
    /// Typically the <see cref="SigningKey.Id"/> from the domain model.
    /// </param>
    /// <param name="keyMaterial">
    /// The domain <see cref="KeyMaterial"/> containing the raw key, algorithm, and metadata.
    /// Must match the <see cref="KeyType"/> supported by this strategy.
    /// </param>
    /// <returns>
    /// A <see cref="SigningCredentials"/> instance configured for the specified algorithm and key.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="keyMaterial"/> has a <see cref="KeyMaterial.Kty"/> 
    /// that does not match the strategy’s <see cref="KeyType"/>.
    /// </exception>
    SigningCredentials GetSigningCredentials(string kid, KeyMaterial keyMaterial);
}