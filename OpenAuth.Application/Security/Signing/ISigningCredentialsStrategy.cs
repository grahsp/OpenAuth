using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Signing;

public interface ISigningCredentialsStrategy
{
    KeyType KeyType { get; }
    SigningCredentials GetSigningCredentials(string kid, KeyMaterial keyMaterial);
}