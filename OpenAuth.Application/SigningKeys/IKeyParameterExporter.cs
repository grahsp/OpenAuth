namespace OpenAuth.Application.SigningKeys;

public interface IKeyParameterExporter
{
    KeyParameters Export(string privateKeyPem);
}