using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Infrastructure.Security.Keys;

public class RsaKeyParameterExporter : IKeyParameterExporter
{
    public KeyParameters Export(Key key)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(key.Value);

        var parameters = rsa.ExportParameters(false);
        if (parameters.Modulus is null || parameters.Exponent is null)
            throw new InvalidOperationException("RSA parameters are invalid: missing modulus or exponent.");
 
        return new KeyParameters(
            N: Base64UrlEncoder.Encode(parameters.Modulus),
            E: Base64UrlEncoder.Encode(parameters.Exponent)
        );
    }
}