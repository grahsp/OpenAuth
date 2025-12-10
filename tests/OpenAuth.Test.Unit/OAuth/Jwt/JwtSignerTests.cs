using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.SigningKeys;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.OAuth.Jwt;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class JwtSignerTests
{
    private const string SecretKey = "this-is-a-test-secret-key-and-is-super-secret!";
    
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;
    private readonly JwtSigner _sut;

    private readonly FakeTimeProvider _time;

    public JwtSignerTests()
    {
        _keyService = Substitute.For<ISigningKeyQueryService>();
        _credentialsFactory = Substitute.For<ISigningCredentialsFactory>();

        // Setup key service
        var keyData = new SigningKeyData(
            SigningKeyId.New(),
            KeyType.HMAC,
            SigningAlgorithm.HS256,
            new Key(SecretKey)
        );

        var signingKey = TestData.CreateValidHmacSigningKey();

        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(signingKey);

        var signingCredentials = CreateSigningCredentials(keyData);
        _credentialsFactory.Create(signingKey).Returns(signingCredentials);

        _time = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var opts = Options.Create(new JwtOptions { Issuer = "test-issuer" });
        _sut = new JwtSigner(opts, _keyService, _credentialsFactory, _time);
    }

    private SigningCredentials CreateSigningCredentials(SigningKeyData keyData)
    {
        var bytes = Encoding.UTF8.GetBytes(keyData.Key.Value);
        var securityKey = new SymmetricSecurityKey(bytes)
        {
            KeyId = keyData.Kid.Value.ToString()
        };

        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private JwtDescriptor CreateValidDescriptor(Action<Dictionary<string, object>>? configure = null)
    {
        var claims = new Dictionary<string, object>();
        configure?.Invoke(claims);

        return new JwtDescriptor(
            DefaultValues.Audience,
            DefaultValues.Subject,
            600,
            claims
        );
    }

    [Fact]
    public async Task Create_SetsCorrectMetadata()
    {
        var descriptor = CreateValidDescriptor() with { LifetimeInSeconds = 100 };

        var result = await _sut.Create(descriptor);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result);

        var unixIat = _time.GetUtcNow()
            .ToUnixTimeSeconds();
        var unixExp = unixIat + descriptor.LifetimeInSeconds;

        var iat = Assert.Single(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Iat);
        Assert.Equal(unixIat, int.Parse(iat.Value));
        
        var nbf = Assert.Single(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Nbf);
        Assert.Equal(unixIat, int.Parse(nbf.Value));
        
        var exp = Assert.Single(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Exp);
        Assert.Equal(unixExp, int.Parse(exp.Value));
        
        Assert.Single(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public async Task Create_IncludesClaimsFromDescriptor()
    {
        var descriptor = CreateValidDescriptor(opts =>
        {
            opts.Add("key", "value");
        });

        var result = await _sut.Create(descriptor);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result);

        Assert.Single(jwt.Claims, c => c.Type == "key");
    }

    [Fact]
    public async Task Create_WithMultipleScopes_IncludesAllScopes()
    {
        var descriptor = CreateValidDescriptor(claims =>
        {
            claims.Add("scope", "read write");
        });

        var result = await _sut.Create(descriptor);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result);

        var scopes = jwt.Claims.Where(c => c.Type == OAuthClaimTypes.Scope).ToArray();
        Assert.Contains(scopes, c => c.Value.Contains("read"));
        Assert.Contains(scopes, c => c.Value.Contains("write"));
    }

    [Fact]
    public async Task Create_CallsKeyService()
    {
        var descriptor = CreateValidDescriptor();

        await _sut.Create(descriptor);

        await _keyService.Received(1).GetCurrentKeyDataAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_PassesCancellationToken()
    {
        var descriptor = CreateValidDescriptor();
        var cts = new CancellationTokenSource();

        await _sut.Create(descriptor, cts.Token);

        await _keyService.Received(1).GetCurrentKeyDataAsync(cts.Token);
    }

    [Fact]
    public async Task Create_WhenNoActiveKey_ThrowsException()
    {
        var descriptor = CreateValidDescriptor();

        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns((SigningKey)null!);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Create(descriptor));

        Assert.Contains("signing key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_CallsSigningCredentialsFactory()
    {
        var descriptor = CreateValidDescriptor();

        await _sut.Create(descriptor);

        _credentialsFactory.Received(1).Create(Arg.Any<SigningKey>());
    }
}