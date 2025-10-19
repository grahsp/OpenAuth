using System.Text;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.OAuth.Jwt;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class JwtFactoryTests
{
    private const string SecretKey = "this-is-a-test-secret-key-that-is-very-secret!";
    
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;

    private readonly FakeTimeProvider _time;
    private readonly IJwtFactory _sut;

    public JwtFactoryTests()
    {
        _keyService = Substitute.For<ISigningKeyQueryService>();
        _credentialsFactory = Substitute.For<ISigningCredentialsFactory>();

        var keyData = new SigningKeyData(SigningKeyId.New(), KeyType.HMAC, SigningAlgorithm.HS256, new Key(SecretKey));
        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(keyData);

        var signingCredentials = CreateSigningCredentials(keyData);
        _credentialsFactory.Create(keyData)
            .Returns(signingCredentials);
        
        _time = new FakeTimeProvider();
        _sut = new JwtFactory(_keyService, _credentialsFactory);
    }

    private SigningCredentials CreateSigningCredentials(SigningKeyData keyData)
    {
        var bytes= Encoding.UTF8.GetBytes(keyData.Key.Value);
        var securityKey = new SymmetricSecurityKey(bytes) { KeyId = keyData.Kid.Value.ToString() };
        
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private JwtDescriptor CreateValidDescriptor()
    {
        return new JwtBuilder("issuer")
            .WithClient(ClientId.New())
            .WithSubject("subject")
            .WithAudience(new AudienceName("api"))
            .Build(_time);
    }


    [Fact]
    public async Task Create_WithValidDescriptor_ReturnsAccessTokenResult()
    {
        var lifetime = TimeSpan.FromMinutes(5);
        var descriptor = new JwtBuilder("issuer")
            .WithClient(ClientId.New())
            .WithSubject("Subject")
            .WithAudience(new AudienceName("api"))
            .WithLifetime(lifetime)
            .Build(_time);

        var result = await _sut.Create(descriptor);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(TokenType.Bearer, result.TokenType);
        Assert.Equal(lifetime.Seconds, result.ExpiresIn);
    }

    [Fact]
    public async Task Create_CallsKeyService()
    {
        var descriptor = CreateValidDescriptor();
        
        await _sut.Create(descriptor);
        
        await _keyService.Received(1).GetCurrentKeyDataAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenNoActiveKey_ThrowsException()
    {
        var descriptor = CreateValidDescriptor();
        
        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns((SigningKeyData)null!);
        
        var result = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Create(descriptor));
        
        Assert.Contains("signing key", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_CallsSigningCredentialsFactory()
    {
        var descriptor = CreateValidDescriptor();

        await _sut.Create(descriptor);
        
        _credentialsFactory.Received(1).Create(Arg.Any<SigningKeyData>());
    }
}