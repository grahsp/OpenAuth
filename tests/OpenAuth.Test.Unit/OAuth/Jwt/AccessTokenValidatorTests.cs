using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Factories;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Infrastructure.SigningKeys.Handlers;
using OpenAuth.Infrastructure.SigningKeys.KeyMaterials;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class AccessTokenValidatorTests
{
    private readonly SigningKey _signingKey;
    
    private readonly IOptions<JwtOptions> _options;
    private readonly ISigningKeyQueryService _keyProvider;
    private readonly IValidationKeyFactory _validationKeyFactory;
    
    private readonly AccessTokenValidator _sut;
    
    public AccessTokenValidatorTests()
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        
        _signingKey = CreateSigningKey();
        var validationKey = CreateValidationKey(_signingKey);

        _options = Options.Create(new JwtOptions { Issuer = "test-issuer" });
        
        _keyProvider = Substitute.For<ISigningKeyQueryService>();
        _keyProvider.GetActiveKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns([_signingKey]);
        _keyProvider.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(_signingKey);
        
        _validationKeyFactory = Substitute.For<IValidationKeyFactory>();
        _validationKeyFactory.Create(_signingKey)
            .Returns(validationKey);

        _sut = new AccessTokenValidator(_options, _validationKeyFactory, _keyProvider);
    }

    private static SigningKey CreateSigningKey()
    {
        var factory = new SigningKeyFactory([new RsaKeyMaterialGenerator(2048)]);
        return factory.Create(SigningAlgorithm.RS256, DateTimeOffset.UtcNow);
    }

    private static SecurityKey CreateValidationKey(SigningKey key)
    {
        var factory = new ValidationKeyFactory([new RsaSigningKeyHandler()]);
        return factory.Create(key);
    }

    private string CreateJwt(SigningKey key, JwtDescriptor? descriptor = null, TimeProvider? time = null)
    {
        var factory = new SigningCredentialsFactory([new RsaSigningKeyHandler()]);
        var signingCredentials = factory.Create(key);

        descriptor ??= TestData.CreateValidJwtDescriptor();
        
        var now = time?.GetUtcNow().UtcDateTime ?? DateTimeOffset.UtcNow.UtcDateTime;
        var payload = new JwtPayload(
            issuer: _options.Value.Issuer,
            audience: descriptor.Audience,
            claims: null,
            notBefore: now,
            expires: now.AddSeconds(descriptor.LifetimeInSeconds),
            issuedAt: now
        );

        foreach (var (k, v) in descriptor.Claims)
            payload[k] = v;
        
        payload[JwtRegisteredClaimNames.Sub] = descriptor.Subject;

        var jwt = new JwtSecurityToken(
            header: new JwtHeader(signingCredentials),
            payload: payload
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }


    [Fact]
    public async Task ValidateAsync_WhenSuccess_ReturnsPrincipal()
    {
        var token = CreateJwt(_signingKey);
        var principal = await _sut.ValidateAsync(token);

        Assert.NotNull(principal);
        Assert.Contains("openid", principal.FindFirst("scope")?.Value);
    }
    
    [Fact]
    public async Task ValidateAsync_WithoutOpenIdScope_ReturnsNull()
    {
        var claims = new Dictionary<string, object> { { "scope", "profile email" } };
        var descriptor = TestData.CreateValidJwtDescriptor() with { Claims = claims };
        var token = CreateJwt(_signingKey, descriptor);

        var principal = await _sut.ValidateAsync(token);

        Assert.Null(principal);
    }

    [Fact]
    public async Task ValidateAsync_WithoutSubject_ReturnsNull()
    {
        var descriptor = TestData.CreateValidJwtDescriptor() with { Subject = null };
        var token = CreateJwt(_signingKey, descriptor);
    
        var principal = await _sut.ValidateAsync(token);
    
        Assert.Null(principal);
    }
    
    [Fact]
    public async Task ValidateAsync_WithInvalidSignature_ReturnsNull()
    {
        // Create another signing key that the validator does NOT know about
        var otherKey = CreateSigningKey();
    
        var token = CreateJwt(otherKey);
    
        var principal = await _sut.ValidateAsync(token);
    
        Assert.Null(principal);
    }
    
    [Fact]
    public async Task ValidateAsync_WithWrongIssuer_ReturnsNull()
    {
        // Override "iss"
        var claims = new Dictionary<string, object>
        {
            { "iss", "wrong-issuer" },
            { "scope", "openid" }
        };
        var descriptor = TestData.CreateValidJwtDescriptor() with { Claims = claims };
        var token = CreateJwt(_signingKey, descriptor);
    
        var principal = await _sut.ValidateAsync(token);
    
        Assert.Null(principal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ValidateAsync_WithNullOrWhitespace_ThrowsArgumentException(string? input)
    {
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.ValidateAsync(input!));
    }

    [Fact]
    public async Task ValidateAsync_WhenSignedWithOldActiveKey_ReturnsPrincipal()
    {
        var oldKey = CreateSigningKey();

        // Return both keys as active (old + new)
        _keyProvider.GetActiveKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns([oldKey, _signingKey]);

        // Validation must work for old key too
        var oldValidationKey = CreateValidationKey(oldKey);
        _validationKeyFactory.Create(oldKey)
            .Returns(oldValidationKey);

        var token = CreateJwt(oldKey);

        var principal = await _sut.ValidateAsync(token);

        Assert.NotNull(principal);
        Assert.Equal("test-subject", principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
    }
}