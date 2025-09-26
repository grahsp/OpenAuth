using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Security.Jwks;

public interface IPublicKeyExporter
{
    KeyType KeyType { get; }
    KeyParameters Export(Key key);
}