using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Jwks.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.SigningKeys.Application;

public class ValidationKeyFactoryTests
{
    private readonly ISigningKeyHandler _rsaHandler = Substitute.For<ISigningKeyHandler>();
    private readonly ISigningKeyHandler _hmacHandler = Substitute.For<ISigningKeyHandler>();
    private readonly SecurityKey _expectedKey;

    private readonly ValidationKeyFactory _sut;

    public ValidationKeyFactoryTests()
    {
        _expectedKey = new SymmetricSecurityKey(new byte[32]);
        
        _rsaHandler.KeyType.Returns(KeyType.RSA);
        _rsaHandler.CreateValidationKey(Arg.Any<SigningKey>())
            .Returns(_expectedKey);
        
        _hmacHandler.KeyType.Returns(KeyType.HMAC);
        _hmacHandler.CreateValidationKey(Arg.Any<SigningKey>())
            .Returns(_expectedKey);

        _sut = new ValidationKeyFactory([_rsaHandler, _hmacHandler]);
    }
    
    [Fact]
    public void Constructor_WhenNoHandlers_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ValidationKeyFactory([]));
    } 
    
    [Fact]
    public void Constructor_WhenDuplicateKeyTypes_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new ValidationKeyFactory([_rsaHandler, _rsaHandler]));
    }

    [Fact]
    public void Constructor_WhenKeyTypesAreUnique_DoesNotThrow()
    {
        var factory = new ValidationKeyFactory([_rsaHandler, _hmacHandler]);

        Assert.NotNull(factory);
    }

    [Fact]
    public void Create_WhenKeyTypeMatches_HandlerIsInvoked()
    {
        var signingKey = TestData.CreateValidRsaSigningKey();

        var result = _sut.Create(signingKey);

        Assert.Equal(_expectedKey, result);
        _rsaHandler.Received(1).CreateValidationKey(signingKey);
    }

    [Fact]
    public void Create_WhenSigningKeyIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Create(null!));
    }

    [Fact]
    public void Create_WhenNoMatchingHandler_ThrowsInvalidOperationException()
    {
        var signingKey = TestData.CreateValidRsaSigningKey();

        var factory = new ValidationKeyFactory([_hmacHandler]);

        Assert.Throws<InvalidOperationException>(() => factory.Create(signingKey));
    }
}