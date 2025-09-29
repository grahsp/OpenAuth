using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Entities;

namespace OpenAuth.Application.Security.Signing;

/// <summary>
/// Defines a contract for creating framework-level
/// <see cref="SigningCredentials"/> from domain-level <see cref="SigningKey"/> instances.
/// </summary>
public interface ISigningCredentialsFactory
{
    /// <summary>
    /// Creates <see cref="SigningCredentials"/> for the specified domain signing key.
    /// </summary>
    /// <param name="signingKey">
    /// The domain <see cref="SigningKey"/> containing the key material, algorithm,
    /// and metadata required to construct credentials.
    /// </param>
    /// <returns>
    /// A <see cref="SigningCredentials"/> instance usable by the
    /// <see cref="System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler"/> and related APIs.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="signingKey"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no strategy exists for the key type of <paramref name="signingKey"/>.
    /// </exception>
    SigningCredentials Create(SigningKey signingKey);
}