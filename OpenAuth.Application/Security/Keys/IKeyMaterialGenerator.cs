using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Keys;

public interface IKeyMaterialGenerator
{
    IReadOnlyCollection<SigningAlgorithm> SupportedAlgorithms { get; }
    KeyMaterial Create(SigningAlgorithm algorithm);
}