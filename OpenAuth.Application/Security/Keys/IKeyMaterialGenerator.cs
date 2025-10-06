using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.Security.Keys;

public interface IKeyMaterialGenerator
{
    KeyType KeyType { get; }
    KeyMaterial Create(SigningAlgorithm algorithm);
}