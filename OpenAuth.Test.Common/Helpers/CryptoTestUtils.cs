using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.SigningKeys;
using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Test.Common.Helpers;

public static class CryptoTestUtils
{
    public static Key ParametersToPem(KeyParameters parameters)
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(parameters.N),
            Exponent = Base64UrlEncoder.DecodeBytes(parameters.E)
        });

        var publicKey = rsa.ExportSubjectPublicKeyInfo();
        var pem = PemEncoding.Write("PUBLIC KEY", publicKey);
        return new Key(new string(pem));
    }
}