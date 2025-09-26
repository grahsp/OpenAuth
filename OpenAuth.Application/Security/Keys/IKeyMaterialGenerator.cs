using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Keys;

public interface IKeyMaterialGenerator
{
    KeyType KeyType { get; }
    KeyMaterial Create(SigningAlgorithm algorithm);
}