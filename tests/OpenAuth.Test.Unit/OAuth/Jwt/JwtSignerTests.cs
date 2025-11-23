using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Builders;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.OAuth.Jwt;

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

        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(keyData);

        // Setup signing credentials
        var signingCredentials = CreateSigningCredentials(keyData);
        _credentialsFactory.Create(keyData).Returns(signingCredentials);

        // Fake time
        _time = new FakeTimeProvider(DateTimeOffset.UtcNow);

        _sut = new JwtSigner(_keyService, _credentialsFactory);
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

    private JwtBuilder CreateValidBuilder()
    {
        return new JwtBuilder("issuer", _time)
            .AddClaim(OAuthClaimTypes.Sub, "subject")
            .AddClaim(OAuthClaimTypes.Aud, "api");
    }

    private JwtDescriptor CreateValidDescriptor()
    {
        return CreateValidBuilder().Build();
    }

    [Fact]
    public async Task Create_SetsCorrectMetadata()
    {
        var lifetime = TimeSpan.FromMinutes(5);

        var descriptor = CreateValidBuilder()
            .WithLifetime(lifetime)
            .Build();

        var result = await _sut.Create(descriptor);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task Create_IncludesClaimsFromDescriptor()
    {
        var descriptor = CreateValidDescriptor();

        var result = await _sut.Create(descriptor);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result);

        Assert.Equal(descriptor.Claims.Count, jwt.Claims.Count());
    }

    [Fact]
    public async Task Create_WithMultipleScopes_IncludesAllScopes()
    {
        var descriptor = CreateValidBuilder()
            .AddClaim(OAuthClaimTypes.Scope, "read")
            .AddClaim(OAuthClaimTypes.Scope, "write")
            .Build();

        var result = await _sut.Create(descriptor);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result);

        var scopes = jwt.Claims.Where(c => c.Type == OAuthClaimTypes.Scope).ToArray();
        Assert.Contains(scopes, c => c.Value == "read");
        Assert.Contains(scopes, c => c.Value == "write");
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
            .Returns((SigningKeyData)null!);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Create(descriptor));

        Assert.Contains("signing key", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_CallsSigningCredentialsFactory()
    {
        var descriptor = CreateValidDescriptor();

        await _sut.Create(descriptor);

        _credentialsFactory.Received(1).Create(Arg.Any<SigningKeyData>());
    }
}
