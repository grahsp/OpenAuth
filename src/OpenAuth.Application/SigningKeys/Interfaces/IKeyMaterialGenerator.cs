using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Application.SigningKeys.Interfaces;

public interface IKeyMaterialGenerator
{
    KeyType KeyType { get; }
    KeyMaterial Create(SigningAlgorithm algorithm);
}