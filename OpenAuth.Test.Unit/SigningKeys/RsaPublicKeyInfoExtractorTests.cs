using OpenAuth.Application.Security.Jwks;
using OpenAuth.Domain.Enums;
using OpenAuth.Domain.ValueObjects;
using OpenAuth.Infrastructure.Security.Jwks;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.SigningKeys;

public class RsaPublicKeyInfoExtractorTests
{
    private readonly RsaPublicKeyInfoExtractor _exporter = new();
    private readonly Key _privateKey = new("-----BEGIN PRIVATE KEY-----\n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\nJDRDWAI2egHJGKo9fSL6ipciFDIEpw==\n-----END PRIVATE KEY-----");


    [Fact]
    public void Extract_FromPrivateKey_ReturnsValidRsaPublicKeyInfo()
    {
        // Arrange
        var kid = new SigningKeyId(Guid.NewGuid());
        var keyMaterial = new KeyMaterial(_privateKey, SigningAlgorithm.RS256, KeyType.RSA);

        // Act
        var result = _exporter.Extract(kid, keyMaterial);

        // Assert
        var info = Assert.IsType<RsaPublicKeyInfo>(result);
        Assert.NotEmpty(info.N);
        Assert.NotEmpty(info.E);
    }

    [Fact]
    public void Extract_FromPublicKey_ReturnsSameInfoAsPrivateKey()
    {
        // Arrange
        var kid = new SigningKeyId(Guid.NewGuid());
        var privateMaterial = new KeyMaterial(_privateKey, SigningAlgorithm.RS256, KeyType.RSA);
        var fromPrivate = (RsaPublicKeyInfo)_exporter.Extract(kid, privateMaterial);

        var publicPem = CryptoTestUtils.RsaPublicKeyInfoToPem(fromPrivate);
        var publicMaterial = new KeyMaterial(publicPem, SigningAlgorithm.RS256, KeyType.RSA);

        // Act
        var fromPublic = (RsaPublicKeyInfo)_exporter.Extract(kid, publicMaterial);

        // Assert
        Assert.Equal(fromPrivate.N, fromPublic.N);
        Assert.Equal(fromPrivate.E, fromPublic.E);
    }

    [Fact]
    public void Extract_RoundTripsPublicKeyInfo()
    {
        // Arrange
        var kid = new SigningKeyId(Guid.NewGuid());
        var keyMaterial = new KeyMaterial(_privateKey, SigningAlgorithm.RS256, KeyType.RSA);
        var info = (RsaPublicKeyInfo)_exporter.Extract(kid, keyMaterial);

        // Act
        var pem = CryptoTestUtils.RsaPublicKeyInfoToPem(info);
        var roundtripMaterial = new KeyMaterial(pem, SigningAlgorithm.RS256, KeyType.RSA);
        var roundtripped = (RsaPublicKeyInfo)_exporter.Extract(kid, roundtripMaterial);

        // Assert
        Assert.Equal(info.N, roundtripped.N);
        Assert.Equal(info.E, roundtripped.E);
    }

    [Fact]
    public void Extract_Throws_WhenInvalidKey()
    {
        // Arrange
        var key = new Key("not-a-valid-pem");
        var keyMaterial = new KeyMaterial(key, SigningAlgorithm.RS256, KeyType.RSA);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _exporter.Extract(new SigningKeyId(Guid.NewGuid()), keyMaterial));
    }
}