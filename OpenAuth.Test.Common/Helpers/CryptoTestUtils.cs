using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.SigningKeys.ValueObjects;

namespace OpenAuth.Test.Common.Helpers;

public static class CryptoTestUtils
{
    public static Key RsaPublicKeyInfoToPem(RsaPublicKeyInfo info)
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(info.N),
            Exponent = Base64UrlEncoder.DecodeBytes(info.E)
        });

        var publicKey = rsa.ExportSubjectPublicKeyInfo();
        var pem = PemEncoding.Write("PUBLIC KEY", publicKey);
        
        return new Key(new string(pem));
    }
}