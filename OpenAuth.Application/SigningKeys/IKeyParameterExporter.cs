using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.SigningKeys;

public interface IKeyParameterExporter
{
    KeyParameters Export(Key key);
}